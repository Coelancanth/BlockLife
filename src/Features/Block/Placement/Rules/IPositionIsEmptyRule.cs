using BlockLife.Core.Domain.Common;
using LanguageExt;

namespace BlockLife.Core.Features.Block.Placement.Rules;

public interface IPositionIsEmptyRule
{
    Fin<Unit> Validate(Vector2Int position);
}
