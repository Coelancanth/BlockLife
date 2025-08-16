using System;

namespace BlockLife.Core.Domain.Common
{
    /// <summary>
    /// Represents a 2D integer coordinate position in the grid.
    /// Immutable value object that provides safe coordinate operations.
    /// </summary>
    public readonly record struct Vector2Int(int X, int Y)
    {
        public static readonly Vector2Int Zero = new(0, 0);
        public static readonly Vector2Int One = new(1, 1);
        public static readonly Vector2Int Up = new(0, 1);
        public static readonly Vector2Int Down = new(0, -1);
        public static readonly Vector2Int Left = new(-1, 0);
        public static readonly Vector2Int Right = new(1, 0);

        /// <summary>
        /// Calculates the Manhattan distance between two positions.
        /// </summary>
        public int ManhattanDistanceTo(Vector2Int other) =>
            Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        /// <summary>
        /// Checks if this position is adjacent to another position (including diagonals).
        /// </summary>
        public bool IsAdjacentTo(Vector2Int other) =>
            ManhattanDistanceTo(other) <= 1 && this != other;

        /// <summary>
        /// Checks if this position is orthogonally adjacent (not diagonal).
        /// </summary>
        public bool IsOrthogonallyAdjacentTo(Vector2Int other) =>
            ManhattanDistanceTo(other) == 1;

        /// <summary>
        /// Gets all orthogonally adjacent positions.
        /// </summary>
        public Vector2Int[] GetOrthogonallyAdjacentPositions() => new[]
        {
            this + Up,
            this + Down,
            this + Left,
            this + Right
        };

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
        public static Vector2Int operator *(Vector2Int a, int scalar) => new(a.X * scalar, a.Y * scalar);

        public override string ToString() => $"({X}, {Y})";
    }
}
