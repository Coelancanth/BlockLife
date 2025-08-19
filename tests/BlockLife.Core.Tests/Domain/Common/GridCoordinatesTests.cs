using System;
using BlockLife.Core.Domain.Common;
using FluentAssertions;
using Xunit;

namespace BlockLife.Core.Tests.Domain.Common
{
    public class GridCoordinatesTests
    {
        private readonly Vector2Int _gridSize = new(10, 10);

        [Theory]
        [InlineData(0, 0)]    // Bottom-left corner
        [InlineData(9, 9)]    // Top-right corner
        [InlineData(5, 5)]    // Center
        [InlineData(0, 9)]    // Top-left corner
        [InlineData(9, 0)]    // Bottom-right corner
        public void IsValid_WithValidPositions_ReturnsTrue(int x, int y)
        {
            // Arrange
            var pos = new Vector2Int(x, y);

            // Act
            var result = GridCoordinates.IsValid(pos, _gridSize);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(-1, 0)]   // Left of grid
        [InlineData(10, 0)]   // Right of grid
        [InlineData(0, -1)]   // Below grid
        [InlineData(0, 10)]   // Above grid
        [InlineData(-5, -5)]  // Far outside
        public void IsValid_WithInvalidPositions_ReturnsFalse(int x, int y)
        {
            // Arrange
            var pos = new Vector2Int(x, y);

            // Act
            var result = GridCoordinates.IsValid(pos, _gridSize);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AssertValid_WithValidPosition_DoesNotThrow()
        {
            // Arrange
            var pos = new Vector2Int(5, 5);

            // Act & Assert
            var act = () => GridCoordinates.AssertValid(pos, _gridSize, "Test");
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(-1, 0, "X=-1 out of range")]
        [InlineData(10, 0, "X=10 out of range")]
        [InlineData(0, -1, "Y=-1 out of range")]
        [InlineData(0, 10, "Y=10 out of range")]
        public void AssertValid_WithInvalidPosition_ThrowsArgumentException(int x, int y, string expectedMessage)
        {
            // Arrange
            var pos = new Vector2Int(x, y);

            // Act & Assert
            var act = () => GridCoordinates.AssertValid(pos, _gridSize, "Test");
            act.Should().Throw<ArgumentException>()
               .WithMessage($"*{expectedMessage}*");
        }

        [Theory]
        [InlineData(0, 0, 0)]     // Bottom-left = index 0
        [InlineData(9, 0, 9)]     // Bottom-right = index 9
        [InlineData(0, 1, 10)]    // Second row, first column = index 10
        [InlineData(5, 5, 55)]    // Middle = index 55
        [InlineData(9, 9, 99)]    // Top-right = index 99
        public void ToIndex_ConvertsCorrectly(int x, int y, int expectedIndex)
        {
            // Arrange
            var pos = new Vector2Int(x, y);

            // Act
            var index = GridCoordinates.ToIndex(pos, _gridSize.X);

            // Assert
            index.Should().Be(expectedIndex);
        }

        [Theory]
        [InlineData(0, 0, 0)]     // Index 0 = bottom-left
        [InlineData(9, 9, 0)]     // Index 9 = bottom-right
        [InlineData(10, 0, 1)]    // Index 10 = second row, first column
        [InlineData(55, 5, 5)]    // Index 55 = middle
        [InlineData(99, 9, 9)]    // Index 99 = top-right
        public void FromIndex_ConvertsCorrectly(int index, int expectedX, int expectedY)
        {
            // Act
            var pos = GridCoordinates.FromIndex(index, _gridSize.X);

            // Assert
            pos.X.Should().Be(expectedX);
            pos.Y.Should().Be(expectedY);
        }

        [Fact]
        public void ToIndex_AndFromIndex_AreInverses()
        {
            // Test that converting to index and back gives the original position
            for (int y = 0; y < _gridSize.Y; y++)
            {
                for (int x = 0; x < _gridSize.X; x++)
                {
                    var original = new Vector2Int(x, y);
                    var index = GridCoordinates.ToIndex(original, _gridSize.X);
                    var recovered = GridCoordinates.FromIndex(index, _gridSize.X);
                    
                    recovered.Should().Be(original, 
                        $"Position ({x},{y}) -> index {index} -> position should be ({x},{y})");
                }
            }
        }

        [Fact]
        public void GetOrthogonalNeighbors_CenterPosition_ReturnsFourNeighbors()
        {
            // Arrange
            var pos = new Vector2Int(5, 5);

            // Act
            var neighbors = GridCoordinates.GetOrthogonalNeighbors(pos, _gridSize);

            // Assert
            neighbors.Should().HaveCount(4);
            neighbors.Should().Contain(new Vector2Int(4, 5)); // Left
            neighbors.Should().Contain(new Vector2Int(6, 5)); // Right
            neighbors.Should().Contain(new Vector2Int(5, 4)); // Down
            neighbors.Should().Contain(new Vector2Int(5, 6)); // Up
        }

        [Fact]
        public void GetOrthogonalNeighbors_CornerPosition_ReturnsTwoNeighbors()
        {
            // Arrange
            var pos = new Vector2Int(0, 0); // Bottom-left corner

            // Act
            var neighbors = GridCoordinates.GetOrthogonalNeighbors(pos, _gridSize);

            // Assert
            neighbors.Should().HaveCount(2);
            neighbors.Should().Contain(new Vector2Int(1, 0)); // Right
            neighbors.Should().Contain(new Vector2Int(0, 1)); // Up
        }

        [Fact]
        public void GetAllNeighbors_CenterPosition_ReturnsEightNeighbors()
        {
            // Arrange
            var pos = new Vector2Int(5, 5);

            // Act
            var neighbors = GridCoordinates.GetAllNeighbors(pos, _gridSize);

            // Assert
            neighbors.Should().HaveCount(8);
        }

        [Fact]
        public void GetAllNeighbors_CornerPosition_ReturnsThreeNeighbors()
        {
            // Arrange
            var pos = new Vector2Int(0, 0); // Bottom-left corner

            // Act
            var neighbors = GridCoordinates.GetAllNeighbors(pos, _gridSize);

            // Assert
            neighbors.Should().HaveCount(3);
            neighbors.Should().Contain(new Vector2Int(1, 0)); // Right
            neighbors.Should().Contain(new Vector2Int(0, 1)); // Up
            neighbors.Should().Contain(new Vector2Int(1, 1)); // Diagonal
        }

        [Theory]
        [InlineData(-5, -5, 0, 0)]      // Far outside -> clamped to bottom-left
        [InlineData(15, 15, 9, 9)]      // Far outside -> clamped to top-right
        [InlineData(5, -3, 5, 0)]       // Below grid -> clamped to bottom edge
        [InlineData(5, 12, 5, 9)]       // Above grid -> clamped to top edge
        [InlineData(-3, 5, 0, 5)]       // Left of grid -> clamped to left edge
        [InlineData(12, 5, 9, 5)]       // Right of grid -> clamped to right edge
        [InlineData(5, 5, 5, 5)]        // Valid position -> unchanged
        public void Clamp_ClampsToGridBounds(int inX, int inY, int expectedX, int expectedY)
        {
            // Arrange
            var pos = new Vector2Int(inX, inY);

            // Act
            var clamped = GridCoordinates.Clamp(pos, _gridSize);

            // Assert
            clamped.X.Should().Be(expectedX);
            clamped.Y.Should().Be(expectedY);
        }

        [Fact]
        public void FromIndex_WithInvalidGridWidth_ThrowsArgumentException()
        {
            // Act & Assert
            var act = () => GridCoordinates.FromIndex(5, 0);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Grid width must be positive*");
        }
    }
}