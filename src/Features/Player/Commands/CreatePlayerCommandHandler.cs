using BlockLife.Core.Domain.Player;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Player.Commands
{
    /// <summary>
    /// Handler for CreatePlayerCommand - Implements functional CQRS pattern with Fin&lt;T&gt; monads.
    /// Creates a new player with initial state for single-player mode.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class CreatePlayerCommandHandler : IRequestHandler<CreatePlayerCommand, Fin<PlayerState>>
    {
        private readonly IPlayerStateService _playerStateService;
        private readonly ILogger _logger;

        public CreatePlayerCommandHandler(
            IPlayerStateService playerStateService,
            ILogger logger)
        {
            _playerStateService = playerStateService;
            _logger = logger;
        }

        public Task<Fin<PlayerState>> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
        {
            _logger.Debug("Processing CreatePlayerCommand for player name: {PlayerName}", request.PlayerName);

            // Step 1: Validate player name
            var validationResult = ValidatePlayerName(request.PlayerName);
            if (validationResult.IsFail)
            {
                var error = validationResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Warning("Player name validation failed: {Error}", error.Message);
                return Task.FromResult(FinFail<PlayerState>(error));
            }

            // Step 2: Create player via service
            var createResult = _playerStateService.CreatePlayer(request.PlayerName);
            if (createResult.IsFail)
            {
                var error = createResult.Match<Error>(Succ: _ => Error.New("UNKNOWN", "Unknown error"), Fail: e => e);
                _logger.Error("Failed to create player {PlayerName}: {Error}", request.PlayerName, error.Message);
                return Task.FromResult(FinFail<PlayerState>(error));
            }

            var newPlayer = createResult.Match(Succ: p => p, Fail: _ => throw new InvalidOperationException());

            _logger.Debug("Successfully created player {PlayerName} with ID {PlayerId}",
                newPlayer.Name, newPlayer.Id);

            return Task.FromResult(FinSucc(newPlayer));
        }

        private Fin<LanguageExt.Unit> ValidatePlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return FinFail<LanguageExt.Unit>(Error.New("INVALID_PLAYER_NAME", "Player name cannot be null, empty, or whitespace"));

            if (playerName.Trim().Length < 2)
                return FinFail<LanguageExt.Unit>(Error.New("INVALID_PLAYER_NAME", "Player name must be at least 2 characters long"));

            if (playerName.Trim().Length > 50)
                return FinFail<LanguageExt.Unit>(Error.New("INVALID_PLAYER_NAME", "Player name cannot exceed 50 characters"));

            return FinSucc(LanguageExt.Unit.Default);
        }
    }
}