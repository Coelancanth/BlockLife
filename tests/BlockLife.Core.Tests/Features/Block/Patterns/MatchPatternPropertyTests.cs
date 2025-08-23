using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Property-based tests for MatchPattern and MatchPatternRecognizer.
    /// These tests validate invariants that must always hold regardless of input.
    /// Uses FsCheck 3.x API with Gen<T>.ToArbitrary() pattern.
    /// </summary>
    public class MatchPatternPropertyTests
    {
        #region Generators

        /// <summary>
        /// Generates valid BlockType values (excluding Basic for clarity in tests).
        /// </summary>
        private static Gen<BlockType> GenBlockType() =>
            Gen.Elements(
                BlockType.Work,
                BlockType.Study,
                BlockType.Health,
                BlockType.Relationship,
                BlockType.Creativity,
                BlockType.Fun
            );

        /// <summary>
        /// Generates a valid grid position within bounds.
        /// </summary>
        private static Gen<Vector2Int> GenGridPosition(int maxX = 10, int maxY = 10) =>
            from x in Gen.Choose(0, maxX - 1)
            from y in Gen.Choose(0, maxY - 1)
            select new Vector2Int(x, y);

        /// <summary>
        /// Generates a connected horizontal line of positions.
        /// </summary>
        private static Gen<Seq<Vector2Int>> GenHorizontalLine(int minLength = 3, int maxLength = 7) =>
            from startX in Gen.Choose(0, 3)
            from startY in Gen.Choose(0, 9)
            from length in Gen.Choose(minLength, maxLength)
            select Enumerable.Range(startX, length)
                .Select(x => new Vector2Int(x, startY))
                .ToSeq();

        /// <summary>
        /// Generates a connected vertical line of positions.
        /// </summary>
        private static Gen<Seq<Vector2Int>> GenVerticalLine(int minLength = 3, int maxLength = 7) =>
            from startX in Gen.Choose(0, 9)
            from startY in Gen.Choose(0, 3)
            from length in Gen.Choose(minLength, maxLength)
            select Enumerable.Range(startY, length)
                .Select(y => new Vector2Int(startX, y))
                .ToSeq();

        /// <summary>
        /// Generates a connected L-shaped pattern.
        /// </summary>
        private static Gen<Seq<Vector2Int>> GenLShape() =>
            from startX in Gen.Choose(0, 6)
            from startY in Gen.Choose(0, 6)
            from horizontalLength in Gen.Choose(2, 4)
            from verticalLength in Gen.Choose(2, 4)
            select GenerateLShape(startX, startY, horizontalLength, verticalLength);

        private static Seq<Vector2Int> GenerateLShape(int startX, int startY, int hLength, int vLength)
        {
            var positions = new List<Vector2Int>();
            // Horizontal part
            for (int i = 0; i < hLength; i++)
                positions.Add(new Vector2Int(startX + i, startY));
            // Vertical part (excluding corner)
            for (int i = 1; i < vLength; i++)
                positions.Add(new Vector2Int(startX + hLength - 1, startY + i));
            return positions.ToSeq();
        }

        /// <summary>
        /// Generates a connected cross/plus shape pattern.
        /// </summary>
        private static Gen<Seq<Vector2Int>> GenCrossShape() =>
            from centerX in Gen.Choose(2, 7)
            from centerY in Gen.Choose(2, 7)
            from armLength in Gen.Choose(1, 2)
            select GenerateCrossShape(centerX, centerY, armLength);

        private static Seq<Vector2Int> GenerateCrossShape(int centerX, int centerY, int armLength)
        {
            var positions = new List<Vector2Int>
            {
                new Vector2Int(centerX, centerY) // Center
            };
            
            // Add arms
            for (int i = 1; i <= armLength; i++)
            {
                positions.Add(new Vector2Int(centerX - i, centerY)); // Left
                positions.Add(new Vector2Int(centerX + i, centerY)); // Right
                positions.Add(new Vector2Int(centerX, centerY - i)); // Up
                positions.Add(new Vector2Int(centerX, centerY + i)); // Down
            }
            
            return positions.ToSeq();
        }

        /// <summary>
        /// Generates any connected pattern (horizontal, vertical, L-shape, or cross).
        /// </summary>
        private static Gen<Seq<Vector2Int>> GenConnectedPattern() =>
            Gen.OneOf(
                GenHorizontalLine(),
                GenVerticalLine(),
                GenLShape(),
                GenCrossShape()
            );

        /// <summary>
        /// Generates disconnected positions (two separate groups).
        /// Ensures groups are truly disconnected with at least 2 units of separation.
        /// </summary>
        private static Gen<(Seq<Vector2Int> group1, Seq<Vector2Int> group2)> GenDisconnectedGroups() =>
            from group1Start in Gen.Choose(0, 2)
            from group1Y in Gen.Choose(0, 3)
            from group1Length in Gen.Choose(3, 4)
            from group2Start in Gen.Choose(7, 8)  // Ensure horizontal gap of at least 2
            from group2Y in Gen.Choose(6, 8)      // Ensure vertical gap of at least 2
            from group2Length in Gen.Choose(3, 4)
            select (
                Enumerable.Range(group1Start, group1Length)
                    .Select(x => new Vector2Int(x, group1Y))
                    .ToSeq(),
                Enumerable.Range(group2Start, group2Length)
                    .Select(x => new Vector2Int(x, group2Y))
                    .ToSeq()
            );

        #endregion

        #region Pattern Creation Invariants

        [Fact]
        public void Create_WithConnectedPositions_AlwaysSucceeds()
        {
            Prop.ForAll(
                GenConnectedPattern().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    var result = MatchPattern.Create(positions, blockType);
                    
                    // Connected patterns of 3+ blocks should always create successfully
                    return result.IsSome &&
                           result.Map(p => p.Positions.Count == positions.Count).IfNone(false) &&
                           result.Map(p => p.MatchedBlockType == blockType).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void Create_WithLessThanThreePositions_AlwaysFails()
        {
            var genSmallPositionSet = Gen.Choose(0, 2).SelectMany(count =>
                Gen.ListOf(GenGridPosition(), count).Select(list => list.ToSeq())
            );

            Prop.ForAll(
                genSmallPositionSet.ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    var result = MatchPattern.Create(positions, blockType);
                    return result.IsNone;
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void Create_WithDisconnectedGroups_AlwaysFails()
        {
            Prop.ForAll(
                GenDisconnectedGroups().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (groups, blockType) =>
                {
                    var group1 = groups.group1;
                    var group2 = groups.group2;
                    var allPositions = group1.Concat(group2);
                    var result = MatchPattern.Create(allPositions, blockType);
                    
                    return result.IsNone;
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion

        #region Pattern Validation Invariants

        [Fact]
        public void IsValidFor_WithMatchingTypes_AlwaysTrue()
        {
            Prop.ForAll(
                GenConnectedPattern().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    var pattern = MatchPattern.Create(positions, blockType);
                    if (pattern.IsNone) return true;

                    var gridService = new GridStateService(10, 10);
                    // Place blocks of matching type
                    foreach (var pos in positions)
                    {
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = blockType,
                            Position = pos,
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }

                    return pattern.Map(p => p.IsValidFor(gridService)).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void IsValidFor_WithAnyMismatch_AlwaysFalse()
        {
            Prop.ForAll(
                GenConnectedPattern().Where(p => p.Count >= 3).ToArbitrary(),
                GenBlockType().ToArbitrary(),
                Gen.Choose(0, 100).ToArbitrary(),
                (positions, blockType, seed) =>
                {
                    if (positions.Count < 3) return true;

                    var pattern = MatchPattern.Create(positions, blockType);
                    if (pattern.IsNone) return true;

                    var gridService = new GridStateService(10, 10);
                    var random = new Random(seed);
                    var mismatchIndex = random.Next(positions.Count);
                    
                    // Place blocks with one mismatched type
                    for (int i = 0; i < positions.Count; i++)
                    {
                        var type = i == mismatchIndex 
                            ? (blockType == BlockType.Work ? BlockType.Study : BlockType.Work)
                            : blockType;
                            
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = type,
                            Position = positions[i],
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }

                    return pattern.Map(p => !p.IsValidFor(gridService)).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion

        #region Recognizer Invariants

        [Fact]
        public void Recognizer_AlwaysFindsCompleteConnectedComponent()
        {
            Prop.ForAll(
                GenConnectedPattern().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    if (positions.Count < 3) return true;

                    var gridService = new GridStateService(10, 10);
                    var recognizer = new MatchPatternRecognizer();
                    
                    // Place connected blocks
                    foreach (var pos in positions)
                    {
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = blockType,
                            Position = pos,
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }

                    var triggerPos = positions.Head;
                    var context = PatternContext.CreateDefault(triggerPos);
                    var result = recognizer.Recognize(gridService, triggerPos, context);

                    return result.IsSucc &&
                           result.Match(
                               Succ: patterns =>
                               {
                                   if (!patterns.Any()) return false;
                                   
                                   var matchPattern = patterns.Head as MatchPattern;
                                   if (matchPattern == null) return false;
                                   
                                   return matchPattern.Positions.Count == positions.Count;
                               },
                               Fail: _ => false
                           );
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void Recognizer_NeverCombinesDisconnectedGroups()
        {
            Prop.ForAll(
                GenDisconnectedGroups().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (groups, blockType) =>
                {
                    var group1 = groups.group1;
                    var group2 = groups.group2;
                    var gridService = new GridStateService(10, 10);
                    var recognizer = new MatchPatternRecognizer();
                    
                    // Place both groups
                    foreach (var pos in group1.Concat(group2))
                    {
                        var block = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = blockType,
                            Position = pos,
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(block);
                    }

                    var triggerPos = group1.Head;
                    var context = PatternContext.CreateDefault(triggerPos);
                    var result = recognizer.Recognize(gridService, triggerPos, context);

                    return result.IsSucc &&
                           result.Match(
                               Succ: patterns =>
                               {
                                   foreach (var pattern in patterns)
                                   {
                                       var matchPattern = pattern as MatchPattern;
                                       if (matchPattern == null) continue;
                                       
                                       // Pattern should contain only one group, not both
                                       var containsGroup1 = matchPattern.Positions.Exists(p => group1.Contains(p));
                                       var containsGroup2 = matchPattern.Positions.Exists(p => group2.Contains(p));
                                       
                                       if (containsGroup1 && containsGroup2)
                                           return false;
                                   }
                                   return true;
                               },
                               Fail: _ => false
                           );
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion

        #region Outcome Calculation Invariants

        [Fact]
        public void CalculateOutcome_PreservesAllPositions()
        {
            Prop.ForAll(
                GenConnectedPattern().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    var pattern = MatchPattern.Create(positions, blockType);
                    if (pattern.IsNone) return true;

                    return pattern.Map(p =>
                    {
                        var outcome = p.CalculateOutcome();
                        return outcome.RemovedPositions.ToHashSet()
                            .SetEquals(positions.ToHashSet());
                    }).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void CalculateOutcome_SizeBonusScalesCorrectly()
        {
            Prop.ForAll(
                Gen.Choose(3, 10).ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (size, blockType) =>
                {
                    // Create horizontal line of given size
                    var positions = Enumerable.Range(0, size)
                        .Select(x => new Vector2Int(x, 0))
                        .ToSeq();
                    
                    var pattern = MatchPattern.Create(positions, blockType);
                    if (pattern.IsNone) return true;

                    return pattern.Map(p =>
                    {
                        var outcome = p.CalculateOutcome();
                        var expectedBonus = size >= 5 ? 2.0 : (size >= 4 ? 1.5 : 1.0);
                        
                        return Math.Abs(outcome.BonusMultiplier - expectedBonus) < 0.01;
                    }).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion

        #region Conflict Detection Invariants

        [Fact]
        public void ConflictsWith_IsSymmetric()
        {
            Prop.ForAll(
                GenConnectedPattern().Zip(GenBlockType()).ToArbitrary(),
                GenConnectedPattern().Zip(GenBlockType()).ToArbitrary(),
                (pattern1Data, pattern2Data) =>
                {
                    var (positions1, type1) = pattern1Data;
                    var (positions2, type2) = pattern2Data;
                    var pattern1 = MatchPattern.Create(positions1, type1);
                    var pattern2 = MatchPattern.Create(positions2, type2);
                    
                    if (pattern1.IsNone || pattern2.IsNone) 
                        return true;

                    return pattern1.Map(p1 =>
                        pattern2.Map(p2 =>
                        {
                            var p1ConflictsWithP2 = p1.ConflictsWith(p2);
                            var p2ConflictsWithP1 = p2.ConflictsWith(p1);
                            
                            return p1ConflictsWithP2 == p2ConflictsWithP1;
                        }).IfNone(false)
                    ).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        [Fact]
        public void ConflictsWith_SelfAlwaysConflicts()
        {
            Prop.ForAll(
                GenConnectedPattern().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (positions, blockType) =>
                {
                    var pattern = MatchPattern.Create(positions, blockType);
                    if (pattern.IsNone) return true;

                    return pattern.Map(p => p.ConflictsWith(p)).IfNone(false);
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion

        #region CanRecognizeAt Performance Invariants

        [Fact]
        public void CanRecognizeAt_ConsistentWithRecognize()
        {
            Prop.ForAll(
                GenGridPosition().ToArbitrary(),
                GenBlockType().ToArbitrary(),
                (position, blockType) =>
                {
                    var gridService = new GridStateService(10, 10);
                    var recognizer = new MatchPatternRecognizer();
                    
                    // Place a single block
                    var block = new BlockLife.Core.Domain.Block.Block
                    {
                        Id = Guid.NewGuid(),
                        Type = blockType,
                        Position = position,
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now
                    };
                    gridService.PlaceBlock(block);
                    
                    // Add adjacent blocks to potentially form a pattern
                    var adjacentPositions = position.GetOrthogonallyAdjacentPositions()
                        .Where(p => gridService.IsValidPosition(p))
                        .Take(2);
                    
                    foreach (var adjPos in adjacentPositions)
                    {
                        var adjBlock = new BlockLife.Core.Domain.Block.Block
                        {
                            Id = Guid.NewGuid(),
                            Type = blockType,
                            Position = adjPos,
                            CreatedAt = DateTime.Now,
                            LastModifiedAt = DateTime.Now
                        };
                        gridService.PlaceBlock(adjBlock);
                    }
                    
                    var canRecognize = recognizer.CanRecognizeAt(gridService, position);
                    var context = PatternContext.CreateDefault(position);
                    var recognizeResult = recognizer.Recognize(gridService, position, context);
                    
                    return recognizeResult.Match(
                        Succ: patterns =>
                        {
                            if (patterns.Any())
                                return canRecognize;
                            else
                                return true; // No patterns found is valid
                        },
                        Fail: _ => false
                    );
                }
            ).QuickCheckThrowOnFailure();
        }

        #endregion
    }
}