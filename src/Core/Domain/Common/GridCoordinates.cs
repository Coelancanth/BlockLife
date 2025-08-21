using System;

namespace BlockLife.Core.Domain.Common
{
    /// <summary>
    /// Helper class for grid coordinate validation and conversion.
    /// Enforces the standard grid coordinate convention documented in Architecture.md.
    /// 
    /// Coordinate Convention:
    /// - Origin (0,0) at bottom-left
    /// - X increases rightward
    /// - Y increases upward
    /// - Access pattern: grid[x,y]
    /// </summary>
    public static class GridCoordinates
    {
        /// <summary>
        /// Validates that a position is within the grid bounds.
        /// Throws ArgumentException if invalid.
        /// </summary>
        /// <param name="pos">The position to validate</param>
        /// <param name="gridSize">The size of the grid</param>
        /// <param name="context">Context for error messages (e.g., method name)</param>
        /// <exception cref="ArgumentException">Thrown when position is out of bounds</exception>
        public static void AssertValid(Vector2Int pos, Vector2Int gridSize, string context)
        {
            if (pos.X < 0 || pos.X >= gridSize.X)
            {
                throw new ArgumentException(
                    $"{context}: X={pos.X} out of range [0,{gridSize.X})",
                    nameof(pos));
            }

            if (pos.Y < 0 || pos.Y >= gridSize.Y)
            {
                throw new ArgumentException(
                    $"{context}: Y={pos.Y} out of range [0,{gridSize.Y})",
                    nameof(pos));
            }
        }

        /// <summary>
        /// Checks if a position is within the grid bounds.
        /// </summary>
        /// <param name="pos">The position to check</param>
        /// <param name="gridSize">The size of the grid</param>
        /// <returns>True if position is valid, false otherwise</returns>
        public static bool IsValid(Vector2Int pos, Vector2Int gridSize)
        {
            return pos.X >= 0 && pos.X < gridSize.X &&
                   pos.Y >= 0 && pos.Y < gridSize.Y;
        }

        /// <summary>
        /// Converts a 2D grid position to a 1D array index.
        /// Uses row-major ordering (Y * width + X).
        /// </summary>
        /// <param name="pos">The 2D position</param>
        /// <param name="gridWidth">Width of the grid</param>
        /// <returns>1D array index</returns>
        public static int ToIndex(Vector2Int pos, int gridWidth)
        {
            return pos.Y * gridWidth + pos.X;
        }

        /// <summary>
        /// Converts a 1D array index to a 2D grid position.
        /// Inverse of ToIndex.
        /// </summary>
        /// <param name="index">The 1D array index</param>
        /// <param name="gridWidth">Width of the grid</param>
        /// <returns>2D grid position</returns>
        public static Vector2Int FromIndex(int index, int gridWidth)
        {
            if (gridWidth <= 0)
            {
                throw new ArgumentException("Grid width must be positive", nameof(gridWidth));
            }

            return new Vector2Int(
                index % gridWidth,  // X coordinate
                index / gridWidth   // Y coordinate
            );
        }

        /// <summary>
        /// Gets the four orthogonal neighbors of a position.
        /// Only returns neighbors that are within the grid bounds.
        /// </summary>
        /// <param name="pos">The center position</param>
        /// <param name="gridSize">The size of the grid</param>
        /// <returns>Array of valid neighbor positions</returns>
        public static Vector2Int[] GetOrthogonalNeighbors(Vector2Int pos, Vector2Int gridSize)
        {
            var neighbors = new[]
            {
                new Vector2Int(pos.X - 1, pos.Y),  // Left
                new Vector2Int(pos.X + 1, pos.Y),  // Right
                new Vector2Int(pos.X, pos.Y - 1),  // Down
                new Vector2Int(pos.X, pos.Y + 1)   // Up
            };

            return Array.FindAll(neighbors, n => IsValid(n, gridSize));
        }

        /// <summary>
        /// Gets all eight neighbors (orthogonal and diagonal) of a position.
        /// Only returns neighbors that are within the grid bounds.
        /// </summary>
        /// <param name="pos">The center position</param>
        /// <param name="gridSize">The size of the grid</param>
        /// <returns>Array of valid neighbor positions</returns>
        public static Vector2Int[] GetAllNeighbors(Vector2Int pos, Vector2Int gridSize)
        {
            var neighbors = new[]
            {
                // Orthogonal
                new Vector2Int(pos.X - 1, pos.Y),      // Left
                new Vector2Int(pos.X + 1, pos.Y),      // Right
                new Vector2Int(pos.X, pos.Y - 1),      // Down
                new Vector2Int(pos.X, pos.Y + 1),      // Up
                // Diagonal
                new Vector2Int(pos.X - 1, pos.Y - 1),  // Bottom-left
                new Vector2Int(pos.X + 1, pos.Y - 1),  // Bottom-right
                new Vector2Int(pos.X - 1, pos.Y + 1),  // Top-left
                new Vector2Int(pos.X + 1, pos.Y + 1)   // Top-right
            };

            return Array.FindAll(neighbors, n => IsValid(n, gridSize));
        }

        /// <summary>
        /// Clamps a position to be within grid bounds.
        /// </summary>
        /// <param name="pos">The position to clamp</param>
        /// <param name="gridSize">The size of the grid</param>
        /// <returns>Clamped position within bounds</returns>
        public static Vector2Int Clamp(Vector2Int pos, Vector2Int gridSize)
        {
            return new Vector2Int(
                Math.Max(0, Math.Min(gridSize.X - 1, pos.X)),
                Math.Max(0, Math.Min(gridSize.Y - 1, pos.Y))
            );
        }
    }
}
