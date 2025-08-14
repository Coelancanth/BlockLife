using BlockLife.Core.Domain.Common;
using BlockLife.Core.Infrastructure.Services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Features.Block.Placement.Rules;

public class PositionIsEmptyRule : IPositionIsEmptyRule
{
    private readonly IGridStateService _gridState;

    public PositionIsEmptyRule(IGridStateService gridState)
    {
        _gridState = gridState;
    }

    public Fin<Unit> Validate(Vector2Int position) =>
        !_gridState.IsPositionOccupied(position)
            ? FinSucc(Unit.Default)
            : FinFail<Unit>(Error.New("POSITION_OCCUPIED", $"Position {position} is already occupied"));
}
