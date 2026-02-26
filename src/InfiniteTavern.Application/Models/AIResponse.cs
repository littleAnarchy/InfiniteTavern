using System.Text.Json.Serialization;

namespace InfiniteTavern.Application.Models;

public class AIResponse
{
    [JsonPropertyName("narrative")]
    public string Narrative { get; set; } = string.Empty;
    
    [JsonPropertyName("events")]
    public List<GameEvent> Events { get; set; } = new();
    
    [JsonPropertyName("new_npcs")]
    public List<NewNpc> NewNpcs { get; set; } = new();
    
    [JsonPropertyName("quest_updates")]
    public List<QuestUpdate> QuestUpdates { get; set; } = new();
    
    [JsonPropertyName("location_change")]
    public LocationChange? LocationChange { get; set; }
    
    [JsonPropertyName("skill_checks")]
    public List<SkillCheck> SkillChecks { get; set; } = new();
    
    [JsonPropertyName("suggested_actions")]
    public List<string> SuggestedActions { get; set; } = new();
    
    [JsonPropertyName("enemies")]
    public List<EnemyResponse> Enemies { get; set; } = new();
    
    // Token usage info (not from AI response, populated by service)
    public TokenUsage? Usage { get; set; }
}

public class GameEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

public class NewNpc
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("personalityTraits")]
    public string PersonalityTraits { get; set; } = string.Empty;
    
    [JsonPropertyName("currentLocation")]
    public string CurrentLocation { get; set; } = string.Empty;
}

public class QuestUpdate
{
    [JsonPropertyName("questTitle")]
    public string QuestTitle { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class LocationChange
{
    [JsonPropertyName("newLocation")]
    public string NewLocation { get; set; } = string.Empty;

    [JsonPropertyName("locationType")]
    public string LocationType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class SkillCheck
{
    [JsonPropertyName("attribute")]
    public string Attribute { get; set; } = string.Empty; // "Strength", "Dexterity", "Intelligence"
    
    [JsonPropertyName("difficulty")]
    public int Difficulty { get; set; } // DC (Difficulty Class)
    
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty; // What the check is for
}

public class EnemyResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("hp")]
    public int HP { get; set; }
    
    [JsonPropertyName("maxHP")]
    public int MaxHP { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class TokenUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
