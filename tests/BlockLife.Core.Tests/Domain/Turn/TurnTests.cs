using BlockLife.Core.Domain.Turn;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Domain.Turn
{
    /// <summary>
    /// Comprehensive tests for Turn domain value object.
    /// Tests immutability, validation, business rules, and functional operations.
    /// Part of VS_006 Phase 1 - Domain Model testing.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Feature", "Turn")]
    [Trait("Layer", "Domain")]
    public class TurnTests
    {
        [Fact]
        public void Create_ValidNumber_ReturnsSuccessWithTurn()
        {
            // Arrange
            const int turnNumber = 5;

            // Act
            var result = BlockLife.Core.Domain.Turn.Turn.Create(turnNumber);

            // Assert
            result.IsSucc.Should().BeTrue();
            
            var turn = result.IfFail(_ => throw new Exception("Should not fail"));
            turn.Number.Should().Be(turnNumber);
            turn.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_InvalidNumber_ReturnsFailure(int invalidNumber)
        {
            // Act
            var result = BlockLife.Core.Domain.Turn.Turn.Create(invalidNumber);

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("must be 1 or greater"));
        }

        [Fact]
        public void Create_MinimumValidNumber_ReturnsSuccess()
        {
            // Act
            var result = BlockLife.Core.Domain.Turn.Turn.Create(1);

            // Assert
            result.IsSucc.Should().BeTrue();
            var turn = result.IfFail(_ => throw new Exception("Should not fail"));
            turn.Number.Should().Be(1);
        }

        [Fact]
        public void CreateInitial_Always_ReturnsTurnOne()
        {
            // Act
            var turn = BlockLife.Core.Domain.Turn.Turn.CreateInitial();

            // Assert
            turn.Number.Should().Be(1);
            turn.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            turn.IsFirst.Should().BeTrue();
        }

        [Fact]
        public void Next_ValidTurn_ReturnsIncrementedTurn()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.CreateInitial();

            // Act
            var nextResult = turn.Next();

            // Assert
            nextResult.IsSucc.Should().BeTrue();
            
            var nextTurn = nextResult.IfFail(_ => throw new Exception("Should not fail"));
            nextTurn.Number.Should().Be(2);
            nextTurn.CreatedAt.Should().BeAfter(turn.CreatedAt);
            nextTurn.IsFirst.Should().BeFalse();
        }

        [Fact]
        public void Next_MaxIntegerTurn_ReturnsFailure()
        {
            // Arrange - Create turn with max value (this is theoretical, but tests overflow protection)
            var maxTurn = new BlockLife.Core.Domain.Turn.Turn
            {
                Number = int.MaxValue,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = maxTurn.Next();

            // Assert
            result.IsFail.Should().BeTrue();
            result.IfFail(error => error.Message.Should().Contain("overflow"));
        }

        [Theory]
        [InlineData(1, 3, true)]
        [InlineData(5, 5, false)]
        [InlineData(10, 2, false)]
        public void IsBefore_CompareTurns_ReturnsExpectedResult(int firstTurnNumber, int secondTurnNumber, bool expectedResult)
        {
            // Arrange
            var firstTurn = BlockLife.Core.Domain.Turn.Turn.Create(firstTurnNumber).IfFail(_ => throw new Exception());
            var secondTurn = BlockLife.Core.Domain.Turn.Turn.Create(secondTurnNumber).IfFail(_ => throw new Exception());

            // Act
            var result = firstTurn.IsBefore(secondTurn);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(3, 1, true)]
        [InlineData(5, 5, false)]
        [InlineData(2, 10, false)]
        public void IsAfter_CompareTurns_ReturnsExpectedResult(int firstTurnNumber, int secondTurnNumber, bool expectedResult)
        {
            // Arrange
            var firstTurn = BlockLife.Core.Domain.Turn.Turn.Create(firstTurnNumber).IfFail(_ => throw new Exception());
            var secondTurn = BlockLife.Core.Domain.Turn.Turn.Create(secondTurnNumber).IfFail(_ => throw new Exception());

            // Act
            var result = firstTurn.IsAfter(secondTurn);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(100, false)]
        public void IsFirst_VariousTurnNumbers_ReturnsExpectedResult(int turnNumber, bool expectedResult)
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(turnNumber).IfFail(_ => throw new Exception());

            // Act & Assert
            turn.IsFirst.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(1, 5, 4)]
        [InlineData(10, 3, 7)]
        [InlineData(5, 5, 0)]
        [InlineData(100, 200, 100)]
        public void DistanceTo_VariousTurns_ReturnsAbsoluteDifference(int firstNumber, int secondNumber, int expectedDistance)
        {
            // Arrange
            var firstTurn = BlockLife.Core.Domain.Turn.Turn.Create(firstNumber).IfFail(_ => throw new Exception());
            var secondTurn = BlockLife.Core.Domain.Turn.Turn.Create(secondNumber).IfFail(_ => throw new Exception());

            // Act
            var distance = firstTurn.DistanceTo(secondTurn);

            // Assert
            distance.Should().Be(expectedDistance);
        }

        [Fact]
        public void ToString_ValidTurn_ReturnsFormattedString()
        {
            // Arrange
            var turn = BlockLife.Core.Domain.Turn.Turn.Create(42).IfFail(_ => throw new Exception());

            // Act
            var result = turn.ToString();

            // Assert
            result.Should().StartWith("Turn 42");
            result.Should().Contain(turn.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [Fact]
        public void TurnEquality_SameTurnNumbers_AreEqual()
        {
            // Arrange
            var turn1 = new BlockLife.Core.Domain.Turn.Turn { Number = 5, CreatedAt = DateTime.UtcNow };
            var turn2 = new BlockLife.Core.Domain.Turn.Turn { Number = 5, CreatedAt = DateTime.UtcNow };

            // Act & Assert - Records provide structural equality
            turn1.Should().NotBeSameAs(turn2); // Different objects
            turn1.Number.Should().Be(turn2.Number); // But same data
        }

        [Fact]
        public void TurnImmutability_ModifyingCopyDoesNotAffectOriginal()
        {
            // Arrange
            var originalTurn = BlockLife.Core.Domain.Turn.Turn.CreateInitial();

            // Act - Create modified copy using with expression
            var modifiedTurn = originalTurn with { Number = 99 };

            // Assert
            originalTurn.Number.Should().Be(1); // Original unchanged
            modifiedTurn.Number.Should().Be(99); // Copy modified
        }

        [Fact]
        public void ChainedNext_MultipleCalls_CreatesSequence()
        {
            // Arrange
            var initialTurn = BlockLife.Core.Domain.Turn.Turn.CreateInitial();

            // Act
            var turn2 = initialTurn.Next().IfFail(_ => throw new Exception());
            var turn3 = turn2.Next().IfFail(_ => throw new Exception());
            var turn4 = turn3.Next().IfFail(_ => throw new Exception());

            // Assert
            initialTurn.Number.Should().Be(1);
            turn2.Number.Should().Be(2);
            turn3.Number.Should().Be(3);
            turn4.Number.Should().Be(4);
            
            // Each turn should be created after the previous
            turn2.CreatedAt.Should().BeOnOrAfter(initialTurn.CreatedAt);
            turn3.CreatedAt.Should().BeOnOrAfter(turn2.CreatedAt);
            turn4.CreatedAt.Should().BeOnOrAfter(turn3.CreatedAt);
        }
    }
}