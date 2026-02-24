using FluentAssertions;
using InfiniteTavern.Application.Models;
using InfiniteTavern.Application.Services;
using InfiniteTavern.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfiniteTavern.Tests.Services;

public class GameEventHandlerServiceTests
{
    private readonly GameEventHandlerService _service;
    private readonly Mock<ILogger<GameEventHandlerService>> _loggerMock;

    public GameEventHandlerServiceTests()
    {
        _loggerMock = new Mock<ILogger<GameEventHandlerService>>();
        _service = new GameEventHandlerService(_loggerMock.Object);
    }

    [Fact]
    public void ApplyEvent_PlayerDamage_ReducesHP()
    {
        // Arrange
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "damage",
            Target = "player",
            Amount = 5,
            Reason = "Goblin attack"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(15);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Player took 5 damage: Goblin attack");
    }

    [Fact]
    public void ApplyEvent_PlayerDamage_CannotGoBelowZero()
    {
        // Arrange
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "damage",
            Target = "player",
            Amount = 50,
            Reason = "Fatal blow"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(0);
        appliedEvents.Should().HaveCount(2);
        appliedEvents[0].Should().Be("Player took 50 damage: Fatal blow");
        appliedEvents[1].Should().Be("Player has fallen!");
    }

    [Fact]
    public void ApplyEvent_NpcDamage_KillsNpc()
    {
        // Arrange
        var session = CreateTestSession();
        var npc = new Npc
        {
            Name = "Goblin",
            IsAlive = true,
            CurrentLocation = "Forest"
        };
        session.Npcs.Add(npc);

        var gameEvent = new GameEvent
        {
            Type = "damage",
            Target = "Goblin",
            Amount = 10,
            Reason = "Sword slash"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        npc.IsAlive.Should().BeFalse();
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Goblin was defeated: Sword slash");
    }

    [Fact]
    public void ApplyEvent_Heal_RestoresHP()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.HP = 10;

        var gameEvent = new GameEvent
        {
            Type = "heal",
            Target = "player",
            Amount = 8,
            Reason = "Health Potion"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(18);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Player healed 8 HP: Health Potion");
    }

    [Fact]
    public void ApplyEvent_Heal_CannotExceedMaxHP()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.HP = 18;
        session.PlayerCharacter.MaxHP = 20;

        var gameEvent = new GameEvent
        {
            Type = "heal",
            Target = "player",
            Amount = 10,
            Reason = "Health Potion"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(20);
    }

    [Fact]
    public void ApplyEvent_ItemFound_AddsNewItem()
    {
        // Arrange
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "item_found",
            Target = "player",
            Amount = 1,
            Reason = "Iron Sword"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.Inventory.Should().ContainSingle(i => i.Name == "Iron Sword");
        var item = session.PlayerCharacter.Inventory.First(i => i.Name == "Iron Sword");
        item.Quantity.Should().Be(1);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Found: Iron Sword x1");
    }

    [Fact]
    public void ApplyEvent_ItemFound_StacksExistingItem()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Inventory.Add(new Item
        {
            Name = "Health Potion",
            Quantity = 2
        });

        var gameEvent = new GameEvent
        {
            Type = "item_found",
            Target = "player",
            Amount = 3,
            Reason = "Health Potion"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        var item = session.PlayerCharacter.Inventory.First(i => i.Name == "Health Potion");
        item.Quantity.Should().Be(5);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Found: Health Potion x3");
    }

    [Fact]
    public void ApplyEvent_ItemLost_RemovesQuantity()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Inventory.Add(new Item
        {
            Name = "Arrow",
            Quantity = 10
        });

        var gameEvent = new GameEvent
        {
            Type = "item_lost",
            Target = "player",
            Amount = 3,
            Reason = "Arrow"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        var item = session.PlayerCharacter.Inventory.First(i => i.Name == "Arrow");
        item.Quantity.Should().Be(7);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Lost: Arrow x3");
    }

    [Fact]
    public void ApplyEvent_ItemLost_RemovesItemWhenQuantityReachesZero()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Inventory.Add(new Item
        {
            Name = "Key",
            Quantity = 1
        });

        var gameEvent = new GameEvent
        {
            Type = "item_lost",
            Target = "player",
            Amount = 1,
            Reason = "Key"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.Inventory.Should().NotContain(i => i.Name == "Key");
    }

    [Fact]
    public void ApplyEvent_GoldFound_AddsGold()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Gold = 10;

        var gameEvent = new GameEvent
        {
            Type = "gold_found",
            Target = "player",
            Amount = 25,
            Reason = "Looted from chest"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.Gold.Should().Be(35);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Found 25 gold: Looted from chest");
    }

    [Fact]
    public void ApplyEvent_GoldSpent_RemovesGold()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Gold = 50;

        var gameEvent = new GameEvent
        {
            Type = "gold_spent",
            Target = "player",
            Amount = 20,
            Reason = "Bought sword"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.Gold.Should().Be(30);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Spent 20 gold: Bought sword");
    }

    [Fact]
    public void ApplyEvent_GoldSpent_CannotGoBelowZero()
    {
        // Arrange
        var session = CreateTestSession();
        session.PlayerCharacter.Gold = 5;

        var gameEvent = new GameEvent
        {
            Type = "gold_spent",
            Target = "player",
            Amount = 20,
            Reason = "Expensive item"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.Gold.Should().Be(0);
    }

    [Fact]
    public void ApplyEvent_UnknownEventType_LogsWarning()
    {
        // Arrange
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "unknown_event",
            Target = "player",
            Amount = 10,
            Reason = "Test"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        appliedEvents.Should().BeEmpty();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown event type")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ApplyEvent_CaseInsensitive_HandlesEventType()
    {
        // Arrange
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "DAMAGE",
            Target = "PLAYER",
            Amount = 5,
            Reason = "Test"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(15);
    }

    private GameSession CreateTestSession()
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            CurrentLocation = "Test Location",
            PlayerCharacter = new PlayerCharacter
            {
                Name = "Hero",
                HP = 20,
                MaxHP = 20,
                Gold = 0,
                Inventory = new List<Item>()
            },
            Npcs = new List<Npc>(),
            MemoryEntries = new List<MemoryEntry>(),
            Quests = new List<Quest>()
        };
    }
}
