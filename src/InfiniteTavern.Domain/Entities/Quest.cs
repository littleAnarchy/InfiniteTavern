namespace InfiniteTavern.Domain.Entities;

public class Quest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestStatus Status { get; set; }
    public List<string> LogEntries { get; set; } = new();
    public List<QuestObjective> Objectives { get; set; } = new();
}

public class QuestObjective
{
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public enum QuestStatus
{
    Active,
    Completed,
    Failed
}
