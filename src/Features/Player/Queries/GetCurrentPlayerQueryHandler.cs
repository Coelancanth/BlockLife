using BlockLife.Core.Domain.Player;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Player.Queries
{
    /// <summary>
    /// Handler for GetCurrentPlayerQuery - Implements functional CQRS pattern with Fin&lt;T&gt; monads.
    /// Provides read-only access to current player state for UI presentation.
    /// Following TDD+VSA Comprehensive Development Workflow.
    /// </summary>
    public class GetCurrentPlayerQueryHandler : IRequestHandler<GetCurrentPlayerQuery, Fin<PlayerState>>
    {
        private readonly IPlayerStateService _playerStateService;
        private readonly ILogger _logger;

        public GetCurrentPlayerQueryHandler(
            IPlayerStateService playerStateService,
            ILogger logger)
        {
            _playerStateService = playerStateService;
            _logger = logger;
        }

        public Task<Fin<PlayerState>> Handle(GetCurrentPlayerQuery request, CancellationToken cancellationToken)
        {
            _logger.Debug("Processing GetCurrentPlayerQuery");

            var playerResult = _playerStateService.GetCurrentPlayer();
            var result = playerResult.Match(
                Some: player =>
                {
                    _logger.Debug("Found current player: {PlayerName} with total score {TotalScore}",
                        player.Name, player.GetTotalScore());
                    return FinSucc(player);
                },
                None: () =>
                {
                    _logger.Debug("No current player found");
                    return FinFail<PlayerState>(Error.New("NO_CURRENT_PLAYER", "No current player exists"));
                }
            );
            
            return Task.FromResult(result);
        }
    }
}