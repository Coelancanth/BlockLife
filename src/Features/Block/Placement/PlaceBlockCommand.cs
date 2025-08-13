using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Placement;

public sealed record PlaceBlockCommand(
    Vector2Int Position,
    BlockType Type = BlockType.Basic,
    Guid? RequestedId = null
) : IRequest<LanguageExt.Fin<LanguageExt.Unit>>
{
    public Guid BlockId => RequestedId ?? Guid.NewGuid();
}