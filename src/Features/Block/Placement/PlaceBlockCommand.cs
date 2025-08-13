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
    // FIXED: Generate stable ID once and cache it
    private readonly Lazy<Guid> _blockId = new(() => Guid.NewGuid());
    
    public Guid BlockId => RequestedId ?? _blockId.Value;
}