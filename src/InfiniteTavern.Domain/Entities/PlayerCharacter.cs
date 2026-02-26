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
}
