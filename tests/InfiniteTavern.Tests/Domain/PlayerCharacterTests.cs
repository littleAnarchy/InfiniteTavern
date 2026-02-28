using FluentAssertions;
using InfiniteTavern.Domain.Entities;

namespace InfiniteTavern.Tests.Domain;

public class PlayerCharacterTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // GetEquippedBonus
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetEquippedBonus_NoItems_ReturnsZero()
    {
        var player = new PlayerCharacter { Inventory = new List<Item>() };

        player.GetEquippedBonus("Strength").Should().Be(0);
    }

    [Fact]
    public void GetEquippedBonus_UnequippedItem_ReturnsZero()
    {
        var player = CreatePlayer();
        player.Inventory.Add(new Item
        {
            Name = "Iron Sword", IsEquipped = false,
            Bonuses = new Dictionary<string, int> { { "Strength", 2 } }
        });

        player.GetEquippedBonus("Strength").Should().Be(0);
    }

    [Fact]
    public void GetEquippedBonus_EquippedItem_ReturnsBonusValue()
    {
        var player = CreatePlayer();
        player.Inventory.Add(new Item
        {
            Name = "Iron Sword", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 2 } }
        });

        player.GetEquippedBonus("Strength").Should().Be(2);
    }

    [Fact]
    public void GetEquippedBonus_MultipleEquippedItems_SumsAllBonuses()
    {
        var player = CreatePlayer();
        player.Inventory.Add(new Item
        {
            Name = "Iron Sword", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 2 } }
        });
        player.Inventory.Add(new Item
        {
            Name = "Gauntlets of Power", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 1 } }
        });
        player.Inventory.Add(new Item
        {
            Name = "Leather Armor", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Defense", 2 } }
        });

        player.GetEquippedBonus("Strength").Should().Be(3);
        player.GetEquippedBonus("Defense").Should().Be(2);
    }

    [Fact]
    public void GetEquippedBonus_NoBonusForStat_ReturnsZero()
    {
        var player = CreatePlayer();
        player.Inventory.Add(new Item
        {
            Name = "Iron Sword", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 2 } }
        });

        player.GetEquippedBonus("Dexterity").Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Defense
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Defense_BaseOnly_Dex10_ReturnsZero()
    {
        var player = CreatePlayer();
        player.Dexterity = 10;

        player.Defense.Should().Be(0); // (10-10)/2 = 0
    }

    [Fact]
    public void Defense_Dex14_ReturnsPositiveModifier()
    {
        var player = CreatePlayer();
        player.Dexterity = 14;

        player.Defense.Should().Be(2); // (14-10)/2 = 2
    }

    [Fact]
    public void Defense_Dex8_ReturnsNegativeModifier()
    {
        var player = CreatePlayer();
        player.Dexterity = 8;

        player.Defense.Should().Be(-1); // (8-10)/2 = -1
    }

    [Fact]
    public void Defense_WithEquippedArmor_AddsArmorBonus()
    {
        var player = CreatePlayer();
        player.Dexterity = 10; // modifier = 0
        player.Inventory.Add(new Item
        {
            Name = "Chain Mail", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Defense", 3 } }
        });

        player.Defense.Should().Be(3); // 0 + 3 = 3
    }

    [Fact]
    public void Defense_UnequippedArmor_NotIncluded()
    {
        var player = CreatePlayer();
        player.Dexterity = 10;
        player.Inventory.Add(new Item
        {
            Name = "Chain Mail", IsEquipped = false,
            Bonuses = new Dictionary<string, int> { { "Defense", 3 } }
        });

        player.Defense.Should().Be(0); // unequipped, not counted
    }

    [Fact]
    public void Defense_MultipleArmorPieces_SumsAllDefenseBonuses()
    {
        var player = CreatePlayer();
        player.Dexterity = 12; // modifier = 1
        player.Inventory.Add(new Item
        {
            Name = "Iron Shield", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Defense", 2 } }
        });
        player.Inventory.Add(new Item
        {
            Name = "Iron Helmet", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Defense", 1 } }
        });

        player.Defense.Should().Be(4); // 1 (dex) + 2 + 1 = 4
    }

    [Fact]
    public void Defense_EquippedWeapon_NotCountedInDefense()
    {
        var player = CreatePlayer();
        player.Dexterity = 10;
        player.Inventory.Add(new Item
        {
            Name = "Iron Sword", IsEquipped = true,
            Bonuses = new Dictionary<string, int> { { "Strength", 2 } } // no Defense key
        });

        player.Defense.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // XpToNextLevel
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 150)]
    [InlineData(2, 300)]
    [InlineData(5, 750)]
    [InlineData(10, 1500)]
    public void XpToNextLevel_ReturnsLevelTimes150(int level, int expected)
    {
        PlayerCharacter.XpToNextLevel(level).Should().Be(expected);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static PlayerCharacter CreatePlayer() => new()
    {
        Name = "Hero",
        Dexterity = 10,
        Inventory = new List<Item>()
    };
}
