namespace InfiniteTavern.Application.Models;

public class TokenUsageStats
{
    public Guid GameSessionId { get; set; }
    public int TotalTokens { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public List<TokenUsageByType> ByType { get; set; } = new();
    public List<TokenUsageByTurn> ByTurn { get; set; } = new();
    public decimal EstimatedCost { get; set; }
}

public class TokenUsageByType
{
    public string CallType { get; set; } = string.Empty;
    public int Count { get; set; }
    public int TotalTokens { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}

public class TokenUsageByTurn
{
    public int TurnNumber { get; set; }
    public int TotalTokens { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public List<string> CallTypes { get; set; } = new();
}
