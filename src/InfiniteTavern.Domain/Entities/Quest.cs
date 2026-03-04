namespace InfiniteTavern.Domain.Entities;

public class Quest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestStatus Status { get; set; }
    public List<string> LogEntries { get; set; } = new();
}

public enum QuestStatus
{
    Active,
    Completed,
    Failed
}
