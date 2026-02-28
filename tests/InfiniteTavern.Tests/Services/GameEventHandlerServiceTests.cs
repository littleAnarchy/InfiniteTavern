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
    private readonly Mock<IDiceService> _diceServiceMock;

    public GameEventHandlerServiceTests()
    {
        _loggerMock = new Mock<ILogger<GameEventHandlerService>>();
        _diceServiceMock = new Mock<IDiceService>();
        // Default: always roll 1 (enemy always misses) so existing tests still pass
        _diceServiceMock.Setup(d => d.Roll(It.IsAny<string>())).Returns(1);
        _service = new GameEventHandlerService(_loggerMock.Object, _diceServiceMock.Object);
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

    // ──────────────────────────────────────────────────────────────────────────
    // Dodge / Block
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_PlayerDamage_WithAliveAttacker_LowRoll_Dodges()
    {
        // Arrange — roll=1, threshold will be > 1, so the player dodges
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(1);
        var session = CreateTestSession();
        var enemy = new Enemy { Name = "Goblin", HP = 8, MaxHP = 8, IsAlive = true, Attack = 3 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "player", Amount = 5,
            Reason = "Goblin strike", Attacker = "Goblin"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(20); // no damage taken
        appliedEvents.Should().ContainSingle()
            .Which.Should().Contain("Goblin"); // dodged/blocked message
    }

    [Fact]
    public void ApplyEvent_PlayerDamage_WithAliveAttacker_HighRoll_TakesDamage()
    {
        // Arrange — roll=18 → guaranteed hit regardless of defense
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(18);
        var session = CreateTestSession();
        session.PlayerCharacter.Dexterity = 10; // Defense = 0
        var enemy = new Enemy { Name = "Orc", HP = 15, MaxHP = 15, IsAlive = true, Attack = 4 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "player", Amount = 4,
            Reason = "Orc axe swing", Attacker = "Orc"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(16);
        appliedEvents.Should().Contain(e => e.Contains("Player took 4 damage"));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Dead enemy guard
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_PlayerDamage_DeadAttacker_NoDamageDealt()
    {
        // Arrange
        var session = CreateTestSession();
        var dead = new Enemy { Name = "Goblin", HP = 0, MaxHP = 8, IsAlive = false, Attack = 3 };
        session.Enemies.Add(dead);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "player", Amount = 5,
            Reason = "Ghost attack", Attacker = "Goblin"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        session.PlayerCharacter.HP.Should().Be(20); // no damage
        appliedEvents.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Critical Hit — enemy attacks player (nat 20)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_PlayerDamage_Nat20_CriticalHitDoublesAmount()
    {
        // Arrange — roll=20 always → crit
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(20);
        var session = CreateTestSession();
        var enemy = new Enemy { Name = "Dragon", HP = 50, MaxHP = 50, IsAlive = true, Attack = 10 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "player", Amount = 5,
            Reason = "Dragon claw", Attacker = "Dragon"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert — amount doubled = 10
        session.PlayerCharacter.HP.Should().Be(10);
        appliedEvents.Should().ContainSingle()
            .Which.Should().StartWith("💥 Critical Hit!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Player attacks enemy
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_EnemyDamage_ReducesEnemyHP()
    {
        // roll = 10 → no crit
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(10);
        var session = CreateTestSession();
        var enemy = new Enemy { Name = "Goblin", HP = 8, MaxHP = 8, IsAlive = true, Attack = 2 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "Goblin", Amount = 3, Reason = "Sword strike"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        enemy.HP.Should().Be(5);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Be("Goblin took 3 damage: Sword strike");
    }

    [Fact]
    public void ApplyEvent_EnemyDamage_Nat20_CriticalHitDoublesAmount()
    {
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(20);
        var session = CreateTestSession();
        var enemy = new Enemy { Name = "Goblin", HP = 8, MaxHP = 8, IsAlive = true, Attack = 2 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "Goblin", Amount = 3, Reason = "Sword strike"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert — 3 * 2 = 6 damage
        enemy.HP.Should().Be(2);
        appliedEvents.Should().ContainSingle()
            .Which.Should().StartWith("💥 Critical Hit!");
    }

    [Fact]
    public void ApplyEvent_EnemyDamage_KillsEnemy_EndsCombat_AwardsXP()
    {
        _diceServiceMock.Setup(d => d.Roll("1d20")).Returns(10);
        var session = CreateTestSession();
        session.IsInCombat = true;
        session.PlayerCharacter.Strength = 14;
        var enemy = new Enemy { Name = "Goblin", HP = 3, MaxHP = 8, IsAlive = true, Attack = 2 };
        session.Enemies.Add(enemy);

        var gameEvent = new GameEvent
        {
            Type = "damage", Target = "Goblin", Amount = 5, Reason = "Finishing blow"
        };
        var appliedEvents = new List<string>();

        // Act
        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // Assert
        enemy.IsAlive.Should().BeFalse();
        session.IsInCombat.Should().BeFalse();
        appliedEvents.Should().Contain(e => e.Contains("was defeated"));
        appliedEvents.Should().Contain(e => e.Contains("Victory!"));
        appliedEvents.Should().Contain(e => e.Contains("XP"));
        session.PlayerCharacter.Experience.Should().BeGreaterThan(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Level-up via xp_gained + full heal
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_XpGained_IncreasesExperience()
    {
        var session = CreateTestSession();
        session.PlayerCharacter.Experience = 0;
        var gameEvent = new GameEvent
        {
            Type = "xp_gained", Target = "player", Amount = 40, Reason = "Explored dungeon"
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        session.PlayerCharacter.Experience.Should().Be(40);
        appliedEvents.Should().ContainSingle()
            .Which.Should().Contain("40 XP");
    }

    [Fact]
    public void ApplyEvent_XpGained_TriggersLevelUp_AndFullHealsHP()
    {
        var session = CreateTestSession();
        session.PlayerCharacter.Class = "Warrior";
        session.PlayerCharacter.Level = 1;
        session.PlayerCharacter.HP = 5; // damaged
        session.PlayerCharacter.MaxHP = 12;
        session.PlayerCharacter.Experience = 140; // 10 away from level-up (threshold = 150)

        var gameEvent = new GameEvent
        {
            Type = "xp_gained", Target = "player", Amount = 15, Reason = "Quest completed"
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        session.PlayerCharacter.Level.Should().Be(2);
        session.PlayerCharacter.HP.Should().Be(session.PlayerCharacter.MaxHP, "full heal on level-up");
        appliedEvents.Should().Contain(e => e.Contains("LEVEL UP"));
    }

    [Fact]
    public void ApplyEvent_XpGained_SkippedWhenCombatXpAlreadyAwarded()
    {
        var session = CreateTestSession();
        session.CombatXpAwarded = true;
        session.PlayerCharacter.Experience = 0;

        var gameEvent = new GameEvent
        {
            Type = "xp_gained", Target = "player", Amount = 30, Reason = "Goblin kill"
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        // XP should not be added, flag should be reset
        session.PlayerCharacter.Experience.Should().Be(0);
        session.CombatXpAwarded.Should().BeFalse();
        appliedEvents.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // is_unique item guard
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_ItemFound_UniqueItem_BlocksDuplicate()
    {
        var session = CreateTestSession();
        session.PlayerCharacter.Inventory.Add(new Item
        {
            Name = "Enchanted Sword", Type = "Weapon", Quantity = 1
        });

        var gameEvent = new GameEvent
        {
            Type = "item_found", Target = "player", Amount = 1,
            Reason = "Enchanted Sword", IsUnique = true
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        session.PlayerCharacter.Inventory.Count(i => i.Name == "Enchanted Sword").Should().Be(1);
        appliedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ApplyEvent_ItemFound_NonUniqueItem_StacksDuplicate()
    {
        var session = CreateTestSession();
        session.PlayerCharacter.Inventory.Add(new Item
        {
            Name = "Health Potion", Type = "Potion", Quantity = 1
        });

        var gameEvent = new GameEvent
        {
            Type = "item_found", Target = "player", Amount = 1,
            Reason = "Health Potion", IsUnique = false
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        var potion = session.PlayerCharacter.Inventory.First(i => i.Name == "Health Potion");
        potion.Quantity.Should().Be(2);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Item bonuses stored on item_found
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ApplyEvent_ItemFound_WithBonuses_StoresBonusesOnItem()
    {
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "item_found", Target = "player", Amount = 1,
            Reason = "Flaming Axe", ItemType = "Weapon", IsUnique = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 3 } }
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        var item = session.PlayerCharacter.Inventory.First(i => i.Name == "Flaming Axe");
        item.Bonuses.Should().ContainKey("Strength").WhoseValue.Should().Be(3);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CleanItemName strips AI prefixes
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Found: Iron Sword", "Iron Sword")]
    [InlineData("Знайдено Iron Sword", "Iron Sword")]
    [InlineData("Item: Health Potion", "Health Potion")]
    [InlineData("Предмет: Leather Armor", "Leather Armor")]
    [InlineData("Received Amulet of Wisdom", "Amulet of Wisdom")]
    public void ApplyEvent_ItemFound_CleansPrefixFromItemName(string dirtyName, string expectedName)
    {
        var session = CreateTestSession();
        var gameEvent = new GameEvent
        {
            Type = "item_found", Target = "player", Amount = 1,
            Reason = dirtyName
        };
        var appliedEvents = new List<string>();

        _service.ApplyEvent(session, gameEvent, appliedEvents);

        session.PlayerCharacter.Inventory.Should().Contain(i => i.Name == expectedName);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private GameSession CreateTestSession()
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            CurrentLocation = "Test Location",
            PlayerCharacter = new PlayerCharacter
            {
                Name = "Hero",
                Class = "Warrior",
                HP = 20,
                MaxHP = 20,
                Dexterity = 10,
                Gold = 0,
                Inventory = new List<Item>()
            },
            Npcs = new List<Npc>(),
            MemoryEntries = new List<MemoryEntry>(),
            Quests = new List<Quest>()
        };
    }
}
