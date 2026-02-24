namespace InfiniteTavern.Application.Models;

public class ClaudeResponse
{
    public string Narrative { get; set; } = string.Empty;
    public List<GameEvent> Events { get; set; } = new();
    public List<NewNpc> NewNpcs { get; set; } = new();
    public List<QuestUpdate> QuestUpdates { get; set; } = new();
    public LocationChange? LocationChange { get; set; }
    public List<SkillCheck> SkillChecks { get; set; } = new();
}

public class GameEvent
{
    public string Type { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class NewNpc
{
    public string Name { get; set; } = string.Empty;
    public string PersonalityTraits { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
}

public class QuestUpdate
{
    public string QuestTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class LocationChange
{
    public string NewLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SkillCheck
{
    public string Attribute { get; set; } = string.Empty; // "Strength", "Dexterity", "Intelligence"
    public int Difficulty { get; set; } // DC (Difficulty Class)
    public string Purpose { get; set; } = string.Empty; // What the check is for
}
