using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Placement.Notifications;

public sealed record BlockRemovedNotification(
    Guid BlockId,
    Vector2Int Position,
    BlockType Type,
    int Tier,
    DateTime RemovedAt
) : INotification;
