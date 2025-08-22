using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Tests.Properties;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using System;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Properties
{
    /// <summary>
    /// Simple property-based tests that demonstrate the concept works.
    /// These tests use custom generators to avoid FsCheck's automatic generation issues.
    /// Migrated to FsCheck 3.x API
    /// </summary>
    public class SimplePropertyTests
    {
        [Fact]
        public void ValidPositions_AreWithinBounds()
        {
            Prop.ForAll(
                BlockLifeGenerators.ValidPosition(10, 10).ToArbitrary(),
                position =>
                {
                    return position.X >= 0 && position.X < 10 &&
                           position.Y >= 0 && position.Y < 10;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void InvalidPositions_AreOutsideBounds()
        {
            Prop.ForAll(
                BlockLifeGenerators.InvalidPosition(5, 5).ToArbitrary(),
                position =>
                {
                    return position.X < 0 || position.X >= 5 ||
                           position.Y < 0 || position.Y >= 5;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void PrimaryBlockTypes_AreActuallyPrimary()
        {
            Prop.ForAll(
                BlockLifeGenerators.PrimaryBlockType().ToArbitrary(),
                blockType =>
                {
                    return blockType.IsPrimaryType();
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void SpecialBlockTypes_AreActuallySpecial()
        {
            Prop.ForAll(
                BlockLifeGenerators.SpecialBlockType().ToArbitrary(),
                blockType =>
                {
                    return blockType.IsSpecialType();
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void ValidBlocks_HaveValidPositionsAndPrimaryTypes()
        {
            Prop.ForAll(
                BlockLifeGenerators.ValidBlock(8, 8).ToArbitrary(),
                block =>
                {
                    var positionValid = block.Position.X >= 0 && block.Position.X < 8 &&
                                       block.Position.Y >= 0 && block.Position.Y < 8;
                    var typeValid = block.Type.IsPrimaryType();
                    var hasId = block.Id != Guid.Empty;
                    var hasCreatedAt = block.CreatedAt != default;
                    var hasLastModified = block.LastModifiedAt != default;

                    return positionValid && typeValid && hasId && hasCreatedAt && hasLastModified;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void NonOverlappingPositions_AreActuallyNonOverlapping()
        {
            Prop.ForAll(
                BlockLifeGenerators.NonOverlappingPositions(4, 4, 6).ToArbitrary(),
                positions =>
                {
                    // Check that all positions are unique
                    var uniquePositions = positions.Distinct().ToArray();
                    return uniquePositions.Length == positions.Length;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void AdjacentPositions_AreActuallyAdjacent()
        {
            Prop.ForAll(
                BlockLifeGenerators.AdjacentPositions(6, 6).ToArbitrary(),
                positionPair =>
                {
                    var (pos1, pos2) = positionPair;
                    var distance = Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
                    return distance == 1;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void Property_Vector2Int_BasicMathWorks()
        {
            // Test specific Vector2Int properties with concrete examples
            var origin = new Vector2Int(0, 0);
            var adjacent = new Vector2Int(1, 0);
            var distant = new Vector2Int(5, 3);

            // Distance properties
            origin.ManhattanDistanceTo(origin).Should().Be(0);
            origin.ManhattanDistanceTo(adjacent).Should().Be(1);
            origin.ManhattanDistanceTo(distant).Should().Be(8); // |5-0| + |3-0|

            // Symmetry
            origin.ManhattanDistanceTo(distant).Should().Be(distant.ManhattanDistanceTo(origin));

            // Adjacency
            origin.IsOrthogonallyAdjacentTo(adjacent).Should().BeTrue();
            origin.IsOrthogonallyAdjacentTo(distant).Should().BeFalse();
            origin.IsOrthogonallyAdjacentTo(origin).Should().BeFalse();
        }

        [Fact]
        public void Property_ManhattanDistance_TriangleInequality()
        {
            // Test triangle inequality with specific examples
            var a = new Vector2Int(0, 0);
            var b = new Vector2Int(3, 4);
            var c = new Vector2Int(6, 1);

            var directDistance = a.ManhattanDistanceTo(c);
            var indirectDistance = a.ManhattanDistanceTo(b) + b.ManhattanDistanceTo(c);

            directDistance.Should().BeLessThanOrEqualTo(indirectDistance);
        }
    }
}