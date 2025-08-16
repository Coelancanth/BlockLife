using BlockLife.Core.Domain.Common;
using MediatR;
using System;

namespace BlockLife.Core.Features.Block.Placement;

public sealed record RemoveBlockCommand(
    Vector2Int Position
) : IRequest<LanguageExt.Fin<LanguageExt.Unit>>;

// Alternative by ID
public sealed record RemoveBlockByIdCommand(
    Guid BlockId
) : IRequest<LanguageExt.Fin<LanguageExt.Unit>>;
