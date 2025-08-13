using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Placement.Notifications;

public sealed record BlockPlacedNotification(
    Guid BlockId,
    Vector2Int Position,
    BlockType Type,
    DateTime PlacedAt
) : INotification;