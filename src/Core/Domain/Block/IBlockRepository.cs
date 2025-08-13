using BlockLife.Core.Domain.Common;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlockLife.Core.Domain.Block;

public interface IBlockRepository
{
    // Queries
    Task<Option<Block>> GetByIdAsync(Guid id);
    Task<Option<Block>> GetAtPositionAsync(Vector2Int position);
    Task<IReadOnlyList<Block>> GetAllAsync();
    Task<IReadOnlyList<Block>> GetInRegionAsync(Vector2Int topLeft, Vector2Int bottomRight);
    
    // Commands
    Task<Fin<Unit>> AddAsync(Block block);
    Task<Fin<Unit>> UpdateAsync(Block block);
    Task<Fin<Unit>> RemoveAsync(Guid id);
    Task<Fin<Unit>> RemoveAtPositionAsync(Vector2Int position);
    
    // Validation
    Task<bool> ExistsAtPositionAsync(Vector2Int position);
    Task<bool> ExistsAsync(Guid id);
}