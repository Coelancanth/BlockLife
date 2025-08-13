using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlockLife.Core.Application.Simulation;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Features.Block.Placement.Effects;
using BlockLife.Core.Features.Block.Placement.Notifications;
using BlockLife.Core.Features.Block.Placement.Rules;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

namespace BlockLife.Core.Tests.Features.Block.Placement
{
    /// <summary>
    /// REGRESSION TESTS: Prevent duplicate notification publishing
    /// 
    /// BUG CONTEXT:
    /// - Date: 2025-08-14
    /// - Issue: BlockPlacedNotification was published twice
    /// - Root Cause: Both SimulationManager (via effect processing) and PlaceBlockCommandHandler were publishing
    /// - Fix: Removed manual publication from handler, rely only on SimulationManager
    /// 
    /// These tests ensure notifications are published exactly once.
    /// </summary>
    public class NotificationDuplicationTests
    {
        /// <summary>
        /// REGRESSION TEST: Ensure PlaceBlockCommand only results in ONE notification
        /// 
        /// This test verifies that the notification pipeline doesn't duplicate notifications.
        /// The SimulationManager should be the only component publishing BlockPlacedNotification.
        /// </summary>
        [Fact]
        public async Task PlaceBlockCommand_PublishesNotificationExactlyOnce()
        {
            // Arrange
            var notifications = new List<BlockPlacedNotification>();
            var mockMediator = new Mock<IMediator>();
            
            // Capture all published notifications
            mockMediator.Setup(m => m.Publish(It.IsAny<BlockPlacedNotification>(), It.IsAny<CancellationToken>()))
                .Callback<INotification, CancellationToken>((notif, ct) =>
                {
                    if (notif is BlockPlacedNotification bpn)
                        notifications.Add(bpn);
                })
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<SimulationManager>>();
            var simulationManager = new SimulationManager(mockMediator.Object, mockLogger.Object);

            var mockPositionValidRule = new Mock<IPositionIsValidRule>();
            mockPositionValidRule.Setup(r => r.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            var mockPositionEmptyRule = new Mock<IPositionIsEmptyRule>();
            mockPositionEmptyRule.Setup(r => r.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            var mockGridState = new Mock<IGridStateService>();
            mockGridState.Setup(g => g.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            var handlerLogger = new Mock<ILogger<PlaceBlockCommandHandler>>();
            
            var handler = new PlaceBlockCommandHandler(
                mockPositionValidRule.Object,
                mockPositionEmptyRule.Object,
                mockGridState.Object,
                simulationManager,
                handlerLogger.Object
            );

            var command = new PlaceBlockCommand(new Vector2Int(5, 5));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue("Command should succeed");
            notifications.Count.Should().Be(1, 
                "Exactly one BlockPlacedNotification should be published (by SimulationManager only)");
            
            // Verify the notification has correct data
            var notification = notifications[0];
            notification.BlockId.Should().Be(command.BlockId);
            notification.Position.Should().Be(command.Position);
            notification.Type.Should().Be(command.Type);
        }

        /// <summary>
        /// UNIT TEST: SimulationManager publishes notification when processing effects
        /// 
        /// Verifies that SimulationManager correctly publishes notification for BlockPlacedEffect.
        /// </summary>
        [Fact]
        public async Task SimulationManager_ProcessBlockPlacedEffect_PublishesNotification()
        {
            // Arrange
            var blockId = Guid.NewGuid();
            var position = new Vector2Int(3, 3);
            var placedAt = DateTime.UtcNow;
            
            var mockMediator = new Mock<IMediator>();
            BlockPlacedNotification? publishedNotification = null;
            
            mockMediator.Setup(m => m.Publish(It.IsAny<BlockPlacedNotification>(), It.IsAny<CancellationToken>()))
                .Callback<INotification, CancellationToken>((notif, ct) =>
                {
                    publishedNotification = notif as BlockPlacedNotification;
                })
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<SimulationManager>>();
            var simulationManager = new SimulationManager(mockMediator.Object, mockLogger.Object);

            var effect = new BlockPlacedEffect(blockId, position, Domain.Block.BlockType.Basic, placedAt);

            // Act
            simulationManager.QueueEffect(effect);
            await simulationManager.ProcessQueuedEffectsAsync();

            // Assert
            publishedNotification.Should().NotBeNull("Notification should be published");
            publishedNotification.BlockId.Should().Be(blockId);
            publishedNotification.Position.Should().Be(position);
            publishedNotification.PlacedAt.Should().Be(placedAt);
            
            // Verify exactly one publish call
            mockMediator.Verify(m => 
                m.Publish(It.IsAny<BlockPlacedNotification>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        /// <summary>
        /// UNIT TEST: PlaceBlockCommandHandler doesn't publish notifications directly
        /// 
        /// Ensures the handler delegates notification publishing to SimulationManager.
        /// </summary>
        [Fact]
        public async Task PlaceBlockCommandHandler_DoesNotPublishDirectly()
        {
            // Arrange
            var mockSimulationManager = new Mock<ISimulationManager>();
            mockSimulationManager.Setup(s => s.QueueEffect(It.IsAny<BlockPlacedEffect>()))
                .Returns(FinSucc(Unit.Default));
            mockSimulationManager.Setup(s => s.ProcessQueuedEffectsAsync())
                .Returns(Task.CompletedTask);

            var mockPositionValidRule = new Mock<IPositionIsValidRule>();
            mockPositionValidRule.Setup(r => r.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            var mockPositionEmptyRule = new Mock<IPositionIsEmptyRule>();
            mockPositionEmptyRule.Setup(r => r.Validate(It.IsAny<Vector2Int>()))
                .Returns(FinSucc(Unit.Default));

            var mockGridState = new Mock<IGridStateService>();
            mockGridState.Setup(g => g.PlaceBlock(It.IsAny<Domain.Block.Block>()))
                .Returns(FinSucc(Unit.Default));

            var mockLogger = new Mock<ILogger<PlaceBlockCommandHandler>>();
            
            // Note: Handler no longer needs IMediator
            var handler = new PlaceBlockCommandHandler(
                mockPositionValidRule.Object,
                mockPositionEmptyRule.Object,
                mockGridState.Object,
                mockSimulationManager.Object,
                mockLogger.Object
            );

            var command = new PlaceBlockCommand(new Vector2Int(2, 2));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();
            
            // Verify effect was queued
            mockSimulationManager.Verify(s => 
                s.QueueEffect(It.IsAny<BlockPlacedEffect>()), 
                Times.Once);
            
            // Verify effects were processed (which triggers notification in SimulationManager)
            mockSimulationManager.Verify(s => 
                s.ProcessQueuedEffectsAsync(), 
                Times.Once);
        }
    }
}