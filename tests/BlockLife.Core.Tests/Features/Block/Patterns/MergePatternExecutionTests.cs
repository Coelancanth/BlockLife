using System;
using System.Linq;
using BlockLife.Core.Domain.Block;
using BlockLife.Core.Domain.Common;
using BlockLife.Core.Features.Block.Patterns;
using BlockLife.Core.Features.Block.Patterns.Recognizers;
using BlockLife.Core.Features.Block.Patterns.Executors;
using BlockLife.Core.Features.Block.Patterns.Services;
using BlockLife.Core.Infrastructure.Services;
using BlockLife.Core.Features.Player;
using LanguageExt;
using MediatR;
using Moq;
using Xunit;
using static LanguageExt.Prelude;

namespace BlockLife.Core.Tests.Features.Block.Patterns
{
    /// <summary>
    /// Tests for merge pattern execution - when match patterns should merge instead of clear.
    /// Following the Glossary: Merge replaces Match behavior when merge-to-next-tier is unlocked.
    /// </summary>
    public class MergePatternExecutionTests
    {
        private readonly Mock<IGridStateService> _gridService;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IMergeUnlockService> _mergeUnlockService;
        private readonly Mock<IPlayerStateService> _playerStateService;
        private readonly PatternExecutionResolver _resolver;

        public MergePatternExecutionTests()
        {
            _gridService = new Mock<IGridStateService>();
            _mediator = new Mock<IMediator>();
            _mergeUnlockService = new Mock<IMergeUnlockService>();
            _playerStateService = new Mock<IPlayerStateService>();
            
            // Create resolver that will decide between match and merge
            var matchExecutor = new MatchPatternExecutor(_mediator.Object);
            var mergeExecutor = new MergePatternExecutor(_mediator.Object);
            _resolver = new PatternExecutionResolver(_mergeUnlockService.Object, matchExecutor, mergeExecutor);
        }

        [Fact]
        public void WhenMergeToTier2Unlocked_Match3Tier1Blocks_ShouldMergeNotMatch()
        {
            // Arrange - 3 Work-T1 blocks in a row
            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => 
                throw new InvalidOperationException("Failed to create pattern"));
            
            // Merge-to-T2 is unlocked for Work blocks (we'll use tier 1 as default)
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 2))
                .Returns(true);

            // Act - Resolver should choose merge executor, not match executor
            var executor = _resolver.ResolveExecutor(pattern);

            // Assert
            Assert.IsType<MergePatternExecutor>(executor);
            Assert.IsNotType<MatchPatternExecutor>(executor);
        }

        [Fact]
        public void WhenMergeToTier2NotUnlocked_Match3Tier1Blocks_ShouldMatchNotMerge()
        {
            // Arrange - 3 Work-T1 blocks in a row
            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => 
                throw new InvalidOperationException("Failed to create pattern"));
            
            // Merge-to-T2 is NOT unlocked
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 2))
                .Returns(false);

            // Act - Resolver should choose match executor (default behavior)
            var executor = _resolver.ResolveExecutor(pattern);

            // Assert
            Assert.IsType<MatchPatternExecutor>(executor);
            Assert.IsNotType<MergePatternExecutor>(executor);
        }

        [Fact]
        public void MergePattern_With4Blocks_ShouldStillMerge()
        {
            // Arrange - 4 Work-T1 blocks (MORE than 3, should still work!)
            var positions = Seq(
                new Vector2Int(0, 0), 
                new Vector2Int(1, 0), 
                new Vector2Int(2, 0),
                new Vector2Int(3, 0)
            );
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => 
                throw new InvalidOperationException("Failed to create pattern"));
            
            // Merge is unlocked
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 2))
                .Returns(true);

            // Act
            var executor = _resolver.ResolveExecutor(pattern);

            // Assert - Should merge 3 blocks, leave 1 behind
            Assert.IsType<MergePatternExecutor>(executor);
        }

        [Fact]
        public void MergePattern_With5Blocks_ShouldMergeFirst3()
        {
            // Arrange - 5 Work-T2 blocks in L-shape
            var positions = Seq(
                new Vector2Int(0, 0), 
                new Vector2Int(1, 0), 
                new Vector2Int(2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2)
            );
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => 
                throw new InvalidOperationException("Failed to create pattern"));
            
            // Merge-to-T3 is unlocked
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 3))
                .Returns(true);

            // Act
            var executor = _resolver.ResolveExecutor(pattern);

            // Assert
            Assert.IsType<MergePatternExecutor>(executor);
            // Executor should handle selecting which 3 to merge
        }

        [Fact]
        public void Tier2Blocks_WithoutMergeToTier3_ShouldStillMatch()
        {
            // Arrange - 3 Work-T2 blocks 
            var positions = Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            var pattern = MatchPattern.Create(positions, BlockType.Work).IfNone(() => 
                throw new InvalidOperationException("Failed to create pattern"));
            
            // Merge-to-T3 is NOT unlocked (even though merge-to-T2 might be)
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 3))
                .Returns(false);

            // Act
            var executor = _resolver.ResolveExecutor(pattern);

            // Assert - Should match (clear) not merge
            Assert.IsType<MatchPatternExecutor>(executor);
        }

        [Fact]
        public void DifferentBlockTypes_HaveSeparateMergeUnlocks()
        {
            // Arrange - Work has merge unlocked, Study doesn't
            var workPattern = MatchPattern.Create(
                Seq(new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)),
                BlockType.Work
            ).IfNone(() => throw new InvalidOperationException("Failed to create work pattern"));
            
            var studyPattern = MatchPattern.Create(
                Seq(new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)),
                BlockType.Study
            ).IfNone(() => throw new InvalidOperationException("Failed to create study pattern"));

            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Work, 2))
                .Returns(true);
            _mergeUnlockService.Setup(s => s.IsMergeToTierUnlocked(BlockType.Study, 2))
                .Returns(false);

            // Act
            var workExecutor = _resolver.ResolveExecutor(workPattern);
            var studyExecutor = _resolver.ResolveExecutor(studyPattern);

            // Assert
            Assert.IsType<MergePatternExecutor>(workExecutor);
            Assert.IsType<MatchPatternExecutor>(studyExecutor);
        }
    }
}