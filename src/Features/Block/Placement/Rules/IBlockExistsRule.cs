using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;

namespace BlockLife.Core.Features.Block.Placement.Rules;

public interface IBlockExistsRule
{
    Fin<Domain.Block.Block> Validate(Vector2Int position);
    Fin<Domain.Block.Block> Validate(Guid blockId);
}