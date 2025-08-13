using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using System;

namespace BlockLife.Core.Features.Block.Placement.Effects;

public sealed record BlockRemovedEffect(
    Guid BlockId,
    Vector2Int Position,
    BlockType Type,
    DateTime RemovedAt
);