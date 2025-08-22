using BlockLife.Core.Domain.Player;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Features.Player.Commands;
using FluentAssertions;
using LanguageExt;
using Moq;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Player.Commands
{
    /// <summary>
    /// Integration tests for CreatePlayerCommandHandler
    /// Following TDD+VSA Comprehensive Development Workflow
    /// </summary>
    public class CreatePlayerCommandHandlerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly PlayerStateService _playerStateService;
        private readonly CreatePlayerCommandHandler _handler;

        public CreatePlayerCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _playerStateService = new PlayerStateService();

            _handler = new CreatePlayerCommandHandler(
                _playerStateService,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Handle_WhenPlayerNameValid_CreatesPlayerSuccessfully()
        {
            // Arrange
            var command = CreatePlayerCommand.Create("TestPlayer");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSucc.Should().BeTrue();
            var player = result.Match(Succ: p => p, Fail: _ => throw new System.Exception("Test failed"));
            player.Name.Should().Be("TestPlayer");
            player.Id.Should().NotBeEmpty();
            player.Version.Should().Be(1);
            player.GetTotalScore().Should().Be(0);
        }

        [Fact]
        public async Task Handle_WhenPlayerNameEmpty_ReturnsFailure()
        {
            // Arrange
            var command = CreatePlayerCommand.Create("");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            var error = result.Match(Succ: _ => "", Fail: e => e.Message);
            error.Should().Contain("Player name cannot be null");
        }

        [Fact]
        public async Task Handle_WhenPlayerNameTooShort_ReturnsFailure()
        {
            // Arrange
            var command = CreatePlayerCommand.Create("A");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFail.Should().BeTrue();
            var error = result.Match(Succ: _ => "", Fail: e => e.Message);
            error.Should().Contain("must be at least 2 characters");
        }
    }
}