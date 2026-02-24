namespace InfiniteTavern.Domain.Entities;

public class MemoryEntry
{
    public string Content { get; set; } = string.Empty;
    public MemoryType Type { get; set; }
    public int ImportanceScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum MemoryType
{
    Event,
    Summary,
    NPC,
    Quest
}
