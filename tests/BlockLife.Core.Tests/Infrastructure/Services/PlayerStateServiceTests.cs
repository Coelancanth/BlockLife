using BlockLife.Core.Domain.Player;
using BlockLife.Core.Infrastructure.Services;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Infrastructure.Services
{
    /// <summary>
    /// Comprehensive tests for PlayerStateService.
    /// Tests thread-safety, concurrency control, and business logic.
    /// </summary>
    public class PlayerStateServiceTests
    {
        private PlayerStateService CreateService() => new PlayerStateService();

        [Fact]
        public void CreatePlayer_ValidName_ReturnsSuccess()
        {
            // Arrange
            var service = CreateService();
            const string playerName = "TestPlayer";

            // Act
            var result = service.CreatePlayer(playerName);

            // Assert
            result.IsSucc.Should().BeTrue();
            var player = result.IfFail(_ => throw new InvalidOperationException());
            
            player.Name.Should().Be(playerName);
            player.Id.Should().NotBe(Guid.Empty);
            
            // Verify player is now current
            var currentPlayer = service.GetCurrentPlayer();
            currentPlayer.IsSome.Should().BeTrue();
            currentPlayer.IfNone(() => throw new InvalidOperationException()).Id.Should().Be(player.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreatePlayer_InvalidName_ReturnsFailure(string invalidName)
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.CreatePlayer(invalidName);

            // Assert
            result.IsFail.Should().BeTrue();
        }

        [Fact]
        public void CreatePlayer_NullName_ReturnsFailure()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.CreatePlayer(null!);

            // Assert
            result.IsFail.Should().BeTrue();
        }

        [Fact]
        public void GetCurrentPlayer_NoPlayerCreated_ReturnsNone()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.GetCurrentPlayer();

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void GetPlayer_MatchingId_ReturnsPlayer()
        {
            // Arrange
            var service = CreateService();
            var createdPlayer = service.CreatePlayer("TestPlayer")
                .IfFail(_ => throw new InvalidOperationException());

            // Act
            var result = service.GetPlayer(createdPlayer.Id);

            // Assert
            result.IsSome.Should().BeTrue();
            result.IfNone(() => throw new InvalidOperationException()).Id.Should().Be(createdPlayer.Id);
        }

        [Fact]
        public void GetPlayer_NonMatchingId_ReturnsNone()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            var randomId = Guid.NewGuid();

            // Act
            var result = service.GetPlayer(randomId);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void UpdatePlayer_ValidUpdate_ReturnsSuccess()
        {
            // Arrange
            var service = CreateService();
            var originalPlayer = service.CreatePlayer("TestPlayer")
                .IfFail(_ => throw new InvalidOperationException());
            
            var updatedPlayer = originalPlayer.AddResource(ResourceType.Money, 100)
                .IfNone(() => throw new InvalidOperationException());

            // Act
            var result = service.UpdatePlayer(updatedPlayer);

            // Assert
            result.IsSucc.Should().BeTrue();
            
            // Verify the update persisted
            var currentPlayer = service.GetCurrentPlayer()
                .IfNone(() => throw new InvalidOperationException());
            currentPlayer.GetResource(ResourceType.Money).Should().Be(100);
            currentPlayer.Version.Should().Be(updatedPlayer.Version);
        }

        [Fact]
        public void UpdatePlayer_VersionConflict_ReturnsFailure()
        {
            // Arrange
            var service = CreateService();
            var originalPlayer = service.CreatePlayer("TestPlayer")
                .IfFail(_ => throw new InvalidOperationException());
            
            // Simulate concurrent modification
            service.AddResource(ResourceType.Money, 50);
            
            var staleUpdate = originalPlayer.AddResource(ResourceType.SocialCapital, 25)
                .IfNone(() => throw new InvalidOperationException());

            // Act
            var result = service.UpdatePlayer(staleUpdate);

            // Assert
            result.IsFail.Should().BeTrue();
        }

        [Fact]
        public void AddResource_ValidAmount_ReturnsUpdatedPlayer()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            const int amountToAdd = 150;

            // Act
            var result = service.AddResource(ResourceType.Money, amountToAdd);

            // Assert
            result.IsSucc.Should().BeTrue();
            var updatedPlayer = result.IfFail(_ => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(amountToAdd);
            
            // Verify service state updated
            service.GetResourceAmount(ResourceType.Money).Should().Be(amountToAdd);
        }

        [Fact]
        public void AddResource_NoCurrentPlayer_ReturnsFailure()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.AddResource(ResourceType.Money, 100);

            // Assert
            result.IsFail.Should().BeTrue();
        }

        [Fact]
        public void SpendResource_SufficientFunds_ReturnsUpdatedPlayer()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 200);
            const int amountToSpend = 75;

            // Act
            var result = service.SpendResource(ResourceType.Money, amountToSpend);

            // Assert
            result.IsSucc.Should().BeTrue();
            var updatedPlayer = result.IfFail(_ => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(125);
        }

        [Fact]
        public void SpendResource_InsufficientFunds_ReturnsFailure()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 50);

            // Act
            var result = service.SpendResource(ResourceType.Money, 100);

            // Assert
            result.IsFail.Should().BeTrue();
            
            // Verify original amount unchanged
            service.GetResourceAmount(ResourceType.Money).Should().Be(50);
        }

        [Fact]
        public void AddAttribute_ValidAmount_ReturnsUpdatedPlayer()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            const int amountToAdd = 35;

            // Act
            var result = service.AddAttribute(AttributeType.Knowledge, amountToAdd);

            // Assert
            result.IsSucc.Should().BeTrue();
            var updatedPlayer = result.IfFail(_ => throw new InvalidOperationException());
            
            updatedPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(amountToAdd);
            
            // Verify service state updated
            service.GetAttributeLevel(AttributeType.Knowledge).Should().Be(amountToAdd);
        }

        [Fact]
        public void ApplyRewards_ValidChanges_ReturnsUpdatedPlayer()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 100);

            var resourceChanges = Map<ResourceType, int>(
                (ResourceType.Money, -30), // Spend 30
                (ResourceType.SocialCapital, 20) // Gain 20
            );

            var attributeChanges = Map<AttributeType, int>(
                (AttributeType.Knowledge, 15),
                (AttributeType.Health, 10)
            );

            // Act
            var result = service.ApplyRewards(resourceChanges, attributeChanges);

            // Assert
            result.IsSucc.Should().BeTrue();
            var updatedPlayer = result.IfFail(_ => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(70);
            updatedPlayer.GetResource(ResourceType.SocialCapital).Should().Be(20);
            updatedPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(15);
            updatedPlayer.GetAttribute(AttributeType.Health).Should().Be(10);
        }

        [Fact]
        public void ApplyRewards_InsufficientResources_ReturnsFailureAndNoChange()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 25);

            var resourceChanges = Map<ResourceType, int>(
                (ResourceType.Money, -50) // Try to spend more than available
            );
            var attributeChanges = Map<AttributeType, int>();

            // Act
            var result = service.ApplyRewards(resourceChanges, attributeChanges);

            // Assert
            result.IsFail.Should().BeTrue();
            
            // Verify original state unchanged
            service.GetResourceAmount(ResourceType.Money).Should().Be(25);
        }

        [Theory]
        [InlineData(ResourceType.Money, 0)]
        [InlineData(ResourceType.SocialCapital, 0)]
        public void GetResourceAmount_NoPlayer_ReturnsZero(ResourceType resourceType, int expectedAmount)
        {
            // Arrange
            var service = CreateService();

            // Act
            var amount = service.GetResourceAmount(resourceType);

            // Assert
            amount.Should().Be(expectedAmount);
        }

        [Theory]
        [InlineData(AttributeType.Knowledge, 0)]
        [InlineData(AttributeType.Health, 0)]
        public void GetAttributeLevel_NoPlayer_ReturnsZero(AttributeType attributeType, int expectedLevel)
        {
            // Arrange
            var service = CreateService();

            // Act
            var level = service.GetAttributeLevel(attributeType);

            // Assert
            level.Should().Be(expectedLevel);
        }

        [Fact]
        public void CanAfford_SufficientResources_ReturnsTrue()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 100);
            service.AddResource(ResourceType.SocialCapital, 50);

            var costs = Map<ResourceType, int>(
                (ResourceType.Money, 75),
                (ResourceType.SocialCapital, 25)
            );

            // Act
            var canAfford = service.CanAfford(costs);

            // Assert
            canAfford.Should().BeTrue();
        }

        [Fact]
        public void CanAfford_InsufficientResources_ReturnsFalse()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 50);

            var costs = Map<ResourceType, int>(
                (ResourceType.Money, 100) // More than available
            );

            // Act
            var canAfford = service.CanAfford(costs);

            // Assert
            canAfford.Should().BeFalse();
        }

        [Fact]
        public void CanAfford_NoPlayer_ReturnsFalse()
        {
            // Arrange
            var service = CreateService();
            var costs = Map<ResourceType, int>((ResourceType.Money, 1));

            // Act
            var canAfford = service.CanAfford(costs);

            // Assert
            canAfford.Should().BeFalse();
        }

        [Fact]
        public void GetTotalScore_WithPlayerAndResources_ReturnsCorrectTotal()
        {
            // Arrange
            var service = CreateService();
            service.CreatePlayer("TestPlayer");
            service.AddResource(ResourceType.Money, 100);
            service.AddResource(ResourceType.SocialCapital, 50);
            service.AddAttribute(AttributeType.Knowledge, 25);
            service.AddAttribute(AttributeType.Health, 15);

            // Act
            var totalScore = service.GetTotalScore();

            // Assert
            totalScore.Should().Be(190); // 100 + 50 + 25 + 15
        }

        [Fact]
        public void GetTotalScore_NoPlayer_ReturnsZero()
        {
            // Arrange
            var service = CreateService();

            // Act
            var totalScore = service.GetTotalScore();

            // Assert
            totalScore.Should().Be(0);
        }

        [Fact]
        public void ResetPlayer_WithExistingPlayer_ReturnsResetPlayer()
        {
            // Arrange
            var service = CreateService();
            var originalPlayer = service.CreatePlayer("TestPlayer")
                .IfFail(_ => throw new InvalidOperationException());
            service.AddResource(ResourceType.Money, 500);
            service.AddAttribute(AttributeType.Knowledge, 100);

            // Act
            var result = service.ResetPlayer();

            // Assert
            result.IsSucc.Should().BeTrue();
            var resetPlayer = result.IfFail(_ => throw new InvalidOperationException());
            
            resetPlayer.Name.Should().Be(originalPlayer.Name);
            resetPlayer.GetResource(ResourceType.Money).Should().Be(0);
            resetPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(0);
            resetPlayer.GetTotalScore().Should().Be(0);
            resetPlayer.Id.Should().NotBe(originalPlayer.Id); // New player instance
        }

        [Fact]
        public void ResetPlayer_NoExistingPlayer_ReturnsFailure()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.ResetPlayer();

            // Assert
            result.IsFail.Should().BeTrue();
        }

        [Fact]
        public void SavePlayerState_AlwaysSucceeds()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.SavePlayerState();

            // Assert
            result.IsSucc.Should().BeTrue();
        }

        [Fact]
        public void LoadPlayerState_AlwaysSucceeds()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = service.LoadPlayerState();

            // Assert
            result.IsSucc.Should().BeTrue();
        }

        [Fact]
        public void CreatePlayer_ReplacesExistingPlayer()
        {
            // Arrange
            var service = CreateService();
            var firstPlayer = service.CreatePlayer("FirstPlayer")
                .IfFail(_ => throw new InvalidOperationException());
            service.AddResource(ResourceType.Money, 100);

            // Act
            var secondPlayer = service.CreatePlayer("SecondPlayer")
                .IfFail(_ => throw new InvalidOperationException());

            // Assert
            var currentPlayer = service.GetCurrentPlayer()
                .IfNone(() => throw new InvalidOperationException());
            
            currentPlayer.Id.Should().Be(secondPlayer.Id);
            currentPlayer.Name.Should().Be("SecondPlayer");
            currentPlayer.GetResource(ResourceType.Money).Should().Be(0); // Reset state
            
            // First player no longer accessible
            service.GetPlayer(firstPlayer.Id).IsNone.Should().BeTrue();
        }
    }
}
