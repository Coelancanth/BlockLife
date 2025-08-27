using BlockLife.Core.Domain.Turn;
using BlockLife.Core.Features.Turn.Services;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BlockLife.Core.Tests.Features.Turn.Services
{
    /// <summary>
    /// Comprehensive tests for TurnManager service implementation.
    /// Tests business rules, state management, thread safety, and error conditions.
    /// Part of VS_006 Phase 1 - Domain Model testing.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Turn")]
    [Trait("Layer", "Services")]
    public class TurnManagerTests
    {
        [Fact]
        public void GetCurrentTurn_NotInitialized_ReturnsNone()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var result = turnManager.GetCurrentTurn();

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void Reset_Always_InitializesToTurnOne()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var result = turnManager.Reset();

            // Assert
            result.IsSucc.Should().BeTrue();
            
            var turn = result.IfFail(_ => throw new Exception("Should not fail"));
            turn.Number.Should().Be(1);
            turn.IsFirst.Should().BeTrue();
            
            // Verify current turn is set
            var currentTurn = turnManager.GetCurrentTurn();
            currentTurn.IsSome.Should().BeTrue();
            currentTurn.IfNone(() => throw new Exception()).Number.Should().Be(1);
        }

        [Fact]
        public void GetTurnsElapsed_NotInitialized_ReturnsZero()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var elapsed = turnManager.GetTurnsElapsed();

            // Assert
            elapsed.Should().Be(0);
        }

        [Fact]
        public void GetTurnsElapsed_AfterReset_ReturnsOne()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            var elapsed = turnManager.GetTurnsElapsed();

            // Assert
            elapsed.Should().Be(1);
        }

        [Fact]
        public void HasActionBeenPerformed_InitialState_ReturnsFalse()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            var hasAction = turnManager.HasActionBeenPerformed();

            // Assert
            hasAction.Should().BeFalse();
        }

        [Fact]
        public void MarkActionPerformed_FirstTime_SetsFlag()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            turnManager.MarkActionPerformed();

            // Assert
            turnManager.HasActionBeenPerformed().Should().BeTrue();
        }

        [Fact]
        public void MarkActionPerformed_MultipleCallsSameTurn_RemainsTrue()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            turnManager.MarkActionPerformed();
            turnManager.MarkActionPerformed(); // Should not cause error
            turnManager.MarkActionPerformed();

            // Assert
            turnManager.HasActionBeenPerformed().Should().BeTrue();
        }

        [Fact]
        public void CanAdvanceTurn_NoActionPerformed_ReturnsFalse()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            var canAdvance = turnManager.CanAdvanceTurn();

            // Assert
            canAdvance.Should().BeFalse();
        }

        [Fact]
        public void CanAdvanceTurn_ActionPerformed_ReturnsTrue()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();
            turnManager.MarkActionPerformed();

            // Act
            var canAdvance = turnManager.CanAdvanceTurn();

            // Assert
            canAdvance.Should().BeTrue();
        }

        [Fact]
        public void CanAdvanceTurn_NotInitialized_ReturnsFalse()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var canAdvance = turnManager.CanAdvanceTurn();

            // Assert
            canAdvance.Should().BeFalse();
        }

        [Fact]
        public void AdvanceTurn_NoActionPerformed_ReturnsFail()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            var result = turnManager.AdvanceTurn();

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("no action performed"));
        }

        [Fact]
        public void AdvanceTurn_NotInitialized_ReturnsFail()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var result = turnManager.AdvanceTurn();

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("not initialized"));
        }

        [Fact]
        public void AdvanceTurn_ValidConditions_AdvancesToNextTurn()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();
            turnManager.MarkActionPerformed();

            // Act
            var result = turnManager.AdvanceTurn();

            // Assert
            result.IsSucc.Should().BeTrue();
            
            var newTurn = result.IfFail(_ => throw new Exception("Should not fail"));
            newTurn.Number.Should().Be(2);
            
            // Current turn should be updated
            var currentTurn = turnManager.GetCurrentTurn().IfNone(() => throw new Exception());
            currentTurn.Number.Should().Be(2);
            
            // Turns elapsed should be incremented
            turnManager.GetTurnsElapsed().Should().Be(2);
        }

        [Fact]
        public void AdvanceTurn_ValidConditions_ResetsActionFlag()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();
            turnManager.MarkActionPerformed();

            // Act
            turnManager.AdvanceTurn();

            // Assert
            turnManager.HasActionBeenPerformed().Should().BeFalse();
            turnManager.CanAdvanceTurn().Should().BeFalse(); // Can't advance again without new action
        }

        [Fact]
        public void FullTurnCycle_MultipleAdvances_WorksCorrectly()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act & Assert - Turn 1 -> 2
            turnManager.MarkActionPerformed();
            var turn2Result = turnManager.AdvanceTurn();
            turn2Result.IsSucc.Should().BeTrue();
            turnManager.GetTurnsElapsed().Should().Be(2);
            turnManager.HasActionBeenPerformed().Should().BeFalse();

            // Act & Assert - Turn 2 -> 3  
            turnManager.MarkActionPerformed();
            var turn3Result = turnManager.AdvanceTurn();
            turn3Result.IsSucc.Should().BeTrue();
            turnManager.GetTurnsElapsed().Should().Be(3);
            turnManager.HasActionBeenPerformed().Should().BeFalse();

            // Act & Assert - Turn 3 -> 4
            turnManager.MarkActionPerformed();
            var turn4Result = turnManager.AdvanceTurn();
            turn4Result.IsSucc.Should().BeTrue();
            turnManager.GetTurnsElapsed().Should().Be(4);
        }

        [Fact]
        public void LoadState_ValidTurn_LoadesSuccessfully()
        {
            // Arrange
            var turnManager = new TurnManager();
            var turnToLoad = BlockLife.Core.Domain.Turn.Turn.Create(10).IfFail(_ => throw new Exception());

            // Act
            var result = turnManager.LoadState(turnToLoad, actionPerformed: true);

            // Assert
            result.IsSucc.Should().BeTrue();
            result.IfFail(_ => throw new Exception()).Number.Should().Be(10);
            
            turnManager.GetCurrentTurn().IfNone(() => throw new Exception()).Number.Should().Be(10);
            turnManager.HasActionBeenPerformed().Should().BeTrue();
            turnManager.GetTurnsElapsed().Should().Be(10);
        }

        [Fact]
        public void LoadState_NullTurn_ReturnsFail()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var result = turnManager.LoadState(null!, actionPerformed: false);

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("null turn"));
        }

        [Fact]
        public void LoadState_InvalidTurnNumber_ReturnsFail()
        {
            // Arrange
            var turnManager = new TurnManager();
            var invalidTurn = new BlockLife.Core.Domain.Turn.Turn { Number = -5, CreatedAt = DateTime.UtcNow };

            // Act  
            var result = turnManager.LoadState(invalidTurn, actionPerformed: false);

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("Invalid turn number"));
        }

        [Fact]
        public void GetTurnStateSummary_InitializedWithAction_ReturnsCorrectSummary()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();
            turnManager.MarkActionPerformed();

            // Act
            var summary = turnManager.GetTurnStateSummary();

            // Assert
            summary.Should().Contain("Turn 1");
            summary.Should().Contain("Action: Performed");
        }

        [Fact]
        public void GetTurnStateSummary_InitializedWithoutAction_ReturnsCorrectSummary()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act
            var summary = turnManager.GetTurnStateSummary();

            // Assert
            summary.Should().Contain("Turn 1");
            summary.Should().Contain("Action: Pending");
        }

        [Fact]
        public void GetTurnStateSummary_NotInitialized_ReturnsNotInitializedMessage()
        {
            // Arrange
            var turnManager = new TurnManager();

            // Act
            var summary = turnManager.GetTurnStateSummary();

            // Assert
            summary.Should().Contain("not initialized");
        }

        [Fact]
        public void ThreadSafety_ConcurrentOperations_DoNotCorruptState()
        {
            // Arrange
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act - Simulate concurrent operations
            Parallel.For(0, 100, i =>
            {
                turnManager.MarkActionPerformed();
                var canAdvance = turnManager.CanAdvanceTurn();
                var currentTurn = turnManager.GetCurrentTurn();
                var elapsed = turnManager.GetTurnsElapsed();
            });

            // Assert - State should remain consistent
            turnManager.GetCurrentTurn().IsSome.Should().BeTrue();
            turnManager.HasActionBeenPerformed().Should().BeTrue();
            turnManager.GetTurnsElapsed().Should().Be(1); // Should still be on turn 1
        }

        [Fact]
        public void BusinessRule_OneActionPerTurn_EnforcedCorrectly()
        {
            // Arrange  
            var turnManager = new TurnManager();
            turnManager.Reset();

            // Act & Assert - Turn 1: Action -> Advance
            turnManager.HasActionBeenPerformed().Should().BeFalse();
            turnManager.CanAdvanceTurn().Should().BeFalse();
            
            turnManager.MarkActionPerformed();
            turnManager.HasActionBeenPerformed().Should().BeTrue();
            turnManager.CanAdvanceTurn().Should().BeTrue();
            
            var advanceResult = turnManager.AdvanceTurn();
            advanceResult.IsSucc.Should().BeTrue();
            
            // Turn 2: No action yet -> Cannot advance  
            turnManager.HasActionBeenPerformed().Should().BeFalse();
            turnManager.CanAdvanceTurn().Should().BeFalse();
            
            var prematureAdvance = turnManager.AdvanceTurn();
            prematureAdvance.IsFail.Should().BeTrue();
        }
    }
}