namespace InfiniteTavern.Application.Models;

public class TurnRequest
{
    public Guid GameSessionId { get; set; }
    public string PlayerAction { get; set; } = string.Empty;
}

public class TurnResponse
{
    public string Narrative { get; set; } = string.Empty;
    public int PlayerHP { get; set; }
    public int MaxPlayerHP { get; set; }
    public int PlayerLevel { get; set; }
    public int PlayerExperience { get; set; }
    public int PlayerExperienceToNextLevel { get; set; }
    public bool LeveledUp { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public string LocationType { get; set; } = "Tavern";
    public List<string> AppliedEvents { get; set; } = new();
    public List<ItemDto> Inventory { get; set; } = new();
    public int Gold { get; set; }
    public List<DiceRollResult> DiceRolls { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public bool IsInCombat { get; set; }
    public bool IsGameOver { get; set; }
    public List<EnemyDto> Enemies { get; set; } = new();
}

public class DiceRollResult
{
    public string Attribute { get; set; } = string.Empty;
    public int AttributeValue { get; set; }
    public int DiceRoll { get; set; }
    public int Total { get; set; }
    public int Difficulty { get; set; }
    public bool Success { get; set; }
    public string Purpose { get; set; } = string.Empty;
}

public class NewGameRequest
{
    public string CharacterName { get; set; } = string.Empty;
    public string Race { get; set; } = "Human";
    public string Class { get; set; } = "Adventurer";
    public string Language { get; set; } = "English";
    public bool UseDefaultCampaign { get; set; } = true;
}

public class NewGameResponse
{
    public Guid GameSessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public PlayerStats PlayerStats { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public string LocationType { get; set; } = "Tavern";
}

public class PlayerStats
{
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int Level { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Constitution { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public int Defense { get; set; }
    public int Experience { get; set; }
    public int ExperienceToNextLevel { get; set; }
    public List<ItemDto> Inventory { get; set; } = new();
    public int Gold { get; set; }
}

public class ItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsEquipped { get; set; }
    public Dictionary<string, int> Bonuses { get; set; } = new();
}

public class EnemyDto
{
    public string Name { get; set; } = string.Empty;
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public bool IsAlive { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Attack { get; set; } = 3;
}

public class EquipItemRequest
{
    public Guid GameSessionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
}

public class EquipItemResponse
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
    public int Defense { get; set; }
    public List<ItemDto> Inventory { get; set; } = new();
}
