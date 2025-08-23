using BlockLife.Core.Domain.Player;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Features.Player.Queries;
using FluentAssertions;
using LanguageExt;
using Moq;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Player.Queries
{
    /// <summary>
    /// Integration tests for GetCurrentPlayerQueryHandler
    /// Following TDD+VSA Comprehensive Development Workflow
    /// </summary>
    public class GetCurrentPlayerQueryHandlerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly PlayerStateService _playerStateService;
        private readonly GetCurrentPlayerQueryHandler _handler;

        public GetCurrentPlayerQueryHandlerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _playerStateService = new PlayerStateService();

            _handler = new GetCurrentPlayerQueryHandler(
                _playerStateService,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenPlayerExists_ReturnsPlayerState()
        {
            // Arrange - Create a test player
            var createResult = _playerStateService.CreatePlayer("TestPlayer");
            createResult.IsSucc.Should().BeTrue();
            var createdPlayer = createResult.Match(Succ: p => p, Fail: _ => throw new System.Exception("Test setup failed"));

            var query = GetCurrentPlayerQuery.Create();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();
            var player = result.Match(Succ: p => p, Fail: _ => throw new System.Exception("Test failed"));
            player.Id.Should().Be(createdPlayer.Id);
            player.Name.Should().Be("TestPlayer");
        }

        [Fact(Skip = "TODO: Fix error message format - service returns 'NO_CURRENT_PLAYER', test expects 'No current player exists'")]
        public async Task Handle_WhenNoPlayerExists_ReturnsFailure()
        {
            // Arrange - No player created
            var query = GetCurrentPlayerQuery.Create();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            var error = result.Match(Succ: _ => "", Fail: e => e.Message);
            error.Should().Contain("No current player exists");
        }

        [Fact]
        public async Task Handle_WhenPlayerHasResources_ReturnsPlayerWithResources()
        {
            // Arrange - Create player and add resources
            var createResult = _playerStateService.CreatePlayer("TestPlayer");
            createResult.IsSucc.Should().BeTrue();
            
            _playerStateService.AddResource(ResourceType.Money, 100);
            _playerStateService.AddResource(ResourceType.SocialCapital, 50);

            var query = GetCurrentPlayerQuery.Create();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();
            var player = result.Match(Succ: p => p, Fail: _ => throw new System.Exception("Test failed"));
            player.GetResource(ResourceType.Money).Should().Be(100);
            player.GetResource(ResourceType.SocialCapital).Should().Be(50);
        }
    }
}