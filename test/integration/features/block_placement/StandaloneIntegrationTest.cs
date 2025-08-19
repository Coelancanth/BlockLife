using System;
using System.Linq;
using System.Threading.Tasks;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Placement;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.godot_project.scenes.Main;
using FluentAssertions;
using GdUnit4;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace BlockLife.test.integration.features.block_placement
{
    /// <summary>
    /// Standalone integration test following the SimpleSceneTest pattern
    /// Tests direct service access without complex scene loading
    /// </summary>
    [TestSuite]
    public partial class StandaloneIntegrationTest : Node
    {
        private IServiceProvider? _serviceProvider;
        private SceneTree? _sceneTree;
        private IGridStateService? _gridState;
        private MediatR.IMediator? _mediator;
        
        [Before]
        public async Task Setup()
        {
            // Get the scene tree from the test node itself
            _sceneTree = GetTree();
            _sceneTree.Should().NotBeNull("scene tree must be available");
            
            // Get SceneRoot autoload - should be available in Godot context
            var sceneRoot = _sceneTree!.Root.GetNodeOrNull<SceneRoot>("/root/SceneRoot");
            
            if (sceneRoot == null)
            {
                // If SceneRoot doesn't exist, we're not in a proper Godot context
                GD.PrintErr("SceneRoot not found - test must be run from Godot editor with SceneRoot autoload");
                return;
            }
            
            // Get service provider using reflection (following SimpleSceneTest pattern)
            var serviceProviderField = typeof(SceneRoot).GetField("_serviceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _serviceProvider = serviceProviderField?.GetValue(sceneRoot) as IServiceProvider;
            _serviceProvider.Should().NotBeNull("service provider must be initialized");
            
            // Get required services
            if (_serviceProvider != null)
            {
                _gridState = _serviceProvider.GetRequiredService<IGridStateService>();
                _mediator = _serviceProvider.GetRequiredService<MediatR.IMediator>();
            }
            
            // Clear grid before each test
            _gridState?.ClearGrid();
            
            // Wait for frame to ensure everything is initialized
            await ToSignal(_sceneTree, SceneTree.SignalName.ProcessFrame);
        }
        
        [After]
        public async Task Cleanup()
        {
            // Clear grid state after each test
            if (_gridState != null)
            {
                _gridState.ClearGrid();
            }
            
            // Wait for cleanup to complete
            if (_sceneTree != null)
            {
                await ToSignal(_sceneTree, SceneTree.SignalName.ProcessFrame);
            }
        }
        
        [TestCase]
        public async Task TestDirectServiceAccess()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Test that we can access all required services
            var gridState = _serviceProvider.GetRequiredService<IGridStateService>();
            gridState.Should().NotBeNull("grid state service should be accessible");
            
            var mediator = _serviceProvider.GetRequiredService<MediatR.IMediator>();
            mediator.Should().NotBeNull("mediator should be accessible");
            
            var blockRepository = _serviceProvider.GetRequiredService<IBlockRepository>();
            blockRepository.Should().NotBeNull("block repository should be accessible");
            
            // Verify they're all using the same instance (no parallel containers)
            var gridStateAsRepo = gridState as IBlockRepository;
            gridStateAsRepo.Should().BeSameAs(blockRepository, 
                "grid state and block repository should be the same instance");
            
            await Task.CompletedTask;
        }
        
        [TestCase]
        public async Task TestBlockPlacementWithDirectServices()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var position = new Vector2Int(5, 5);
            var command = new PlaceBlockCommand(
                Position: position,
                Type: BlockType.Work
            );
            
            // Act - Place a block using the command
            var result = await _mediator!.Send(command);
            
            // Assert
            result.IsSucc.Should().BeTrue("block placement should succeed");
            
            var block = _gridState!.GetBlockAt(position);
            block.IsSome.Should().BeTrue("block should exist at placed position");
            block.IfSome(b => 
            {
                b.Position.Should().Be(position);
                b.Type.Should().Be(BlockType.Work);
            });
            
            // Wait for any async processing
            await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
        }
        
        [TestCase]
        public async Task TestMultipleBlockPlacements()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var positions = new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 2),
                new Vector2Int(3, 3),
                new Vector2Int(4, 4),
                new Vector2Int(5, 5)
            };
            
            // Act - Place multiple blocks
            foreach (var pos in positions)
            {
                var command = new PlaceBlockCommand(
                    Position: pos,
                    Type: BlockType.Work
                );
                
                var result = await _mediator!.Send(command);
                result.IsSucc.Should().BeTrue($"block placement at {pos} should succeed");
            }
            
            // Assert - All blocks should be placed
            foreach (var pos in positions)
            {
                var block = _gridState!.GetBlockAt(pos);
                block.IsSome.Should().BeTrue($"block should exist at position {pos}");
            }
            
            // Wait for any async processing
            await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
        }
        
        [TestCase]
        public async Task TestDuplicateBlockPrevention()
        {
            if (_serviceProvider == null)
            {
                GD.Print("Skipping test - not in proper Godot context");
                return;
            }
            
            // Arrange
            var position = new Vector2Int(7, 7);
            var command = new PlaceBlockCommand(
                Position: position,
                Type: BlockType.Work
            );
            
            // Act - Try to place block twice at same position
            var firstResult = await _mediator!.Send(command);
            var secondResult = await _mediator!.Send(command);
            
            // Assert
            firstResult.IsSucc.Should().BeTrue("first placement should succeed");
            secondResult.IsFail.Should().BeTrue("second placement should fail (duplicate)");
            
            // Verify only one block exists
            var blocks = _gridState!.GetAllBlocks();
            blocks.Count(b => b.Position == position).Should().Be(1, 
                "only one block should exist at the position");
            
            // Wait for any async processing
            await ToSignal(_sceneTree!, SceneTree.SignalName.ProcessFrame);
        }
    }
}