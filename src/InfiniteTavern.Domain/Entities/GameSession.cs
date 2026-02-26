using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InfiniteTavern.Domain.Entities;

public class GameSession
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string CurrentLocation { get; set; } = "The Infinite Tavern";
    public string WorldTime { get; set; } = "Evening";
    public string Language { get; set; } = "English";
    public int TurnNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    // Combat state
    public bool IsInCombat { get; set; } = false;
    public bool IsGameOver { get; set; } = false;
    public List<Enemy> Enemies { get; set; } = new();

    // Embedded documents (not separate collections!)
    public PlayerCharacter? PlayerCharacter { get; set; }
    public List<Npc> Npcs { get; set; } = new();
    public List<Quest> Quests { get; set; } = new();
    public List<MemoryEntry> MemoryEntries { get; set; } = new();
    public List<TokenUsageEntry> TokenUsageHistory { get; set; } = new();
}
