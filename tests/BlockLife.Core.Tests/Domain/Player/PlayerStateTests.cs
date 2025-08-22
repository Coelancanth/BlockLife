using BlockLife.Core.Domain.Player;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using Xunit;

namespace BlockLife.Core.Tests.Domain.Player
{
    /// <summary>
    /// Comprehensive tests for PlayerState domain model.
    /// Tests immutability, validation, and business rules.
    /// </summary>
    public class PlayerStateTests
    {
        [Fact]
        public void CreateNew_ValidName_ReturnsPlayerWithInitialState()
        {
            // Arrange
            const string playerName = "TestPlayer";

            // Act
            var player = PlayerState.CreateNew(playerName);

            // Assert
            player.Should().NotBeNull();
            player.Id.Should().NotBe(Guid.Empty);
            player.Name.Should().Be(playerName);
            player.Version.Should().Be(1);
            player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            player.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            
            // All resources should start at 0
            player.GetResource(ResourceType.Money).Should().Be(0);
            player.GetResource(ResourceType.SocialCapital).Should().Be(0);
            
            // All attributes should start at 0
            player.GetAttribute(AttributeType.Knowledge).Should().Be(0);
            player.GetAttribute(AttributeType.Health).Should().Be(0);
            player.GetAttribute(AttributeType.Happiness).Should().Be(0);
            player.GetAttribute(AttributeType.Energy).Should().Be(0);
            player.GetAttribute(AttributeType.Nutrition).Should().Be(0);
            player.GetAttribute(AttributeType.Fitness).Should().Be(0);
            player.GetAttribute(AttributeType.Mindfulness).Should().Be(0);
            player.GetAttribute(AttributeType.Creativity).Should().Be(0);
            
            player.GetTotalScore().Should().Be(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateNew_InvalidName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            Action act = () => PlayerState.CreateNew(invalidName);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Player name cannot be null or empty*");
        }

        [Fact]
        public void CreateNew_NullName_ThrowsArgumentException()
        {
            // Act & Assert
            Action act = () => PlayerState.CreateNew(null!);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Player name cannot be null or empty*");
        }

        [Fact]
        public void CreateNew_NameWithWhitespace_TrimsWhitespace()
        {
            // Arrange
            const string nameWithWhitespace = "  Player Name  ";

            // Act
            var player = PlayerState.CreateNew(nameWithWhitespace);

            // Assert
            player.Name.Should().Be("Player Name");
        }

        [Fact]
        public void AddResource_ValidAmount_ReturnsUpdatedState()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");
            const int amountToAdd = 100;

            // Act
            var result = player.AddResource(ResourceType.Money, amountToAdd);

            // Assert
            result.IsSome.Should().BeTrue();
            var updatedPlayer = result.IfNone(() => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(amountToAdd);
            updatedPlayer.Version.Should().Be(player.Version + 1);
            updatedPlayer.LastModifiedAt.Should().BeAfter(player.LastModifiedAt);
            
            // Original state unchanged (immutable)
            player.GetResource(ResourceType.Money).Should().Be(0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void AddResource_InvalidAmount_ReturnsNone(int invalidAmount)
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");

            // Act
            var result = player.AddResource(ResourceType.Money, invalidAmount);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void AddResource_IntegerOverflow_ReturnsNone()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, int.MaxValue)
                .IfNone(() => throw new InvalidOperationException());

            // Act
            var result = player.AddResource(ResourceType.Money, 1);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void SpendResource_SufficientFunds_ReturnsUpdatedState()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 100)
                .IfNone(() => throw new InvalidOperationException());
            const int amountToSpend = 50;

            // Act
            var result = player.SpendResource(ResourceType.Money, amountToSpend);

            // Assert
            result.IsSome.Should().BeTrue();
            var updatedPlayer = result.IfNone(() => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(50);
            updatedPlayer.Version.Should().Be(player.Version + 1);
        }

        [Fact]
        public void SpendResource_InsufficientFunds_ReturnsNone()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 50)
                .IfNone(() => throw new InvalidOperationException());
            const int amountToSpend = 100;

            // Act
            var result = player.SpendResource(ResourceType.Money, amountToSpend);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void SpendResource_InvalidAmount_ReturnsNone(int invalidAmount)
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 1000)
                .IfNone(() => throw new InvalidOperationException());

            // Act
            var result = player.SpendResource(ResourceType.Money, invalidAmount);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void AddAttribute_ValidAmount_ReturnsUpdatedState()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");
            const int amountToAdd = 25;

            // Act
            var result = player.AddAttribute(AttributeType.Knowledge, amountToAdd);

            // Assert
            result.IsSome.Should().BeTrue();
            var updatedPlayer = result.IfNone(() => throw new InvalidOperationException());
            
            updatedPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(amountToAdd);
            updatedPlayer.Version.Should().Be(player.Version + 1);
            updatedPlayer.LastModifiedAt.Should().BeAfter(player.LastModifiedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void AddAttribute_InvalidAmount_ReturnsNone(int invalidAmount)
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");

            // Act
            var result = player.AddAttribute(AttributeType.Knowledge, invalidAmount);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void ApplyChanges_ValidChanges_ReturnsUpdatedState()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 100)
                .IfNone(() => throw new InvalidOperationException());

            var resourceChanges = Map<ResourceType, int>(
                (ResourceType.Money, -50), // Spend 50
                (ResourceType.SocialCapital, 25) // Gain 25
            );

            var attributeChanges = Map<AttributeType, int>(
                (AttributeType.Knowledge, 10),
                (AttributeType.Health, 15)
            );

            // Act
            var result = player.ApplyChanges(resourceChanges, attributeChanges);

            // Assert
            result.IsSome.Should().BeTrue();
            var updatedPlayer = result.IfNone(() => throw new InvalidOperationException());
            
            updatedPlayer.GetResource(ResourceType.Money).Should().Be(50);
            updatedPlayer.GetResource(ResourceType.SocialCapital).Should().Be(25);
            updatedPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(10);
            updatedPlayer.GetAttribute(AttributeType.Health).Should().Be(15);
            updatedPlayer.Version.Should().Be(player.Version + 1);
        }

        [Fact]
        public void ApplyChanges_InsufficientResources_ReturnsNone()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 25)
                .IfNone(() => throw new InvalidOperationException());

            var resourceChanges = Map<ResourceType, int>(
                (ResourceType.Money, -50) // Try to spend 50 when we only have 25
            );

            var attributeChanges = Map<AttributeType, int>(
                (AttributeType.Knowledge, 10)
            );

            // Act
            var result = player.ApplyChanges(resourceChanges, attributeChanges);

            // Assert
            result.IsNone.Should().BeTrue();
            // Original state unchanged
            player.GetResource(ResourceType.Money).Should().Be(25);
        }

        [Fact]
        public void ApplyChanges_InvalidAttributeChanges_ReturnsNone()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");

            var resourceChanges = Map<ResourceType, int>();
            var attributeChanges = Map<AttributeType, int>(
                (AttributeType.Knowledge, -5) // Attributes cannot decrease
            );

            // Act
            var result = player.ApplyChanges(resourceChanges, attributeChanges);

            // Assert
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void CanAfford_SufficientResources_ReturnsTrue()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 100)
                .Bind(p => p.AddResource(ResourceType.SocialCapital, 50))
                .IfNone(() => throw new InvalidOperationException());

            var costs = Map<ResourceType, int>(
                (ResourceType.Money, 75),
                (ResourceType.SocialCapital, 25)
            );

            // Act
            var canAfford = player.CanAfford(costs);

            // Assert
            canAfford.Should().BeTrue();
        }

        [Fact]
        public void CanAfford_InsufficientResources_ReturnsFalse()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 50)
                .IfNone(() => throw new InvalidOperationException());

            var costs = Map<ResourceType, int>(
                (ResourceType.Money, 75) // Need more than we have
            );

            // Act
            var canAfford = player.CanAfford(costs);

            // Assert
            canAfford.Should().BeFalse();
        }

        [Fact]
        public void GetTotalScore_CombinesAllResourcesAndAttributes()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer")
                .AddResource(ResourceType.Money, 100)
                .Bind(p => p.AddResource(ResourceType.SocialCapital, 50))
                .Bind(p => p.AddAttribute(AttributeType.Knowledge, 25))
                .Bind(p => p.AddAttribute(AttributeType.Health, 15))
                .IfNone(() => throw new InvalidOperationException());

            // Act
            var totalScore = player.GetTotalScore();

            // Assert
            totalScore.Should().Be(190); // 100 + 50 + 25 + 15
        }

        [Fact]
        public void Touch_UpdatesLastModifiedAndVersion()
        {
            // Arrange
            var player = PlayerState.CreateNew("TestPlayer");
            var originalLastModified = player.LastModifiedAt;
            var originalVersion = player.Version;

            // Act
            var touchedPlayer = player.Touch();

            // Assert
            touchedPlayer.LastModifiedAt.Should().BeAfter(originalLastModified);
            touchedPlayer.Version.Should().Be(originalVersion + 1);
            touchedPlayer.Id.Should().Be(player.Id);
            touchedPlayer.Name.Should().Be(player.Name);
        }

        [Fact]
        public void PlayerState_IsImmutable_OriginalUnchanged()
        {
            // Arrange
            var originalPlayer = PlayerState.CreateNew("TestPlayer");
            var originalMoney = originalPlayer.GetResource(ResourceType.Money);
            var originalVersion = originalPlayer.Version;

            // Act - perform various operations
            var modifiedPlayer = originalPlayer
                .AddResource(ResourceType.Money, 100)
                .Bind(p => p.AddAttribute(AttributeType.Knowledge, 50))
                .IfNone(() => throw new InvalidOperationException());

            // Assert - original is unchanged
            originalPlayer.GetResource(ResourceType.Money).Should().Be(originalMoney);
            originalPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(0);
            originalPlayer.Version.Should().Be(originalVersion);
            
            // Modified player has changes
            modifiedPlayer.GetResource(ResourceType.Money).Should().Be(100);
            modifiedPlayer.GetAttribute(AttributeType.Knowledge).Should().Be(50);
            modifiedPlayer.Version.Should().Be(originalVersion + 2); // Two operations
        }
    }
}
