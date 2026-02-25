namespace InfiniteTavern.Domain.Entities;

public class TokenUsageEntry
{
    public DateTime Timestamp { get; set; }
    public int TurnNumber { get; set; }
    public string CallType { get; set; } = string.Empty; // "Turn", "SkillCheck", "Combat", "OpeningStory", etc.
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public string ModelName { get; set; } = string.Empty;
}
