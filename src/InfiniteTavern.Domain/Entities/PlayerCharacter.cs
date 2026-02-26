namespace InfiniteTavern.Domain.Entities;

public class PlayerCharacter
{
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Constitution { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public int Experience { get; set; } = 0;
    public List<Item> Inventory { get; set; } = new();
    public int Gold { get; set; } = 0;

    /// <summary>XP required to reach the next level from the current one.</summary>
    public static int XpToNextLevel(int level) => level * 150;

    /// <summary>
    /// Defense rating used for dodge checks. Computed from Dexterity modifier
    /// plus any equipped item "Defense" bonuses.
    /// Formula: d20 + enemy.Attack >= 10 + Defense → hit (miss = dodged)
    /// </summary>
    public int Defense =>
        (Dexterity - 10) / 2 +
        Inventory
            .Where(i => i.IsEquipped && i.Bonuses.ContainsKey("Defense"))
            .Sum(i => i.Bonuses["Defense"]);
}
