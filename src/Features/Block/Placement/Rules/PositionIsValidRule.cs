using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Placement.Rules;

public class PositionIsValidRule : IPositionIsValidRule
{
    private readonly IGridStateService _gridState;
    
    public PositionIsValidRule(IGridStateService gridState)
    {
        _gridState = gridState;
    }
    
    public Fin<Unit> Validate(Vector2Int position) =>
        _gridState.IsValidPosition(position)
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("INVALID_POSITION", $"Position {position} is outside grid bounds"));
}