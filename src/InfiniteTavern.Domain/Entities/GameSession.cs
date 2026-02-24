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

    // Embedded documents (not separate collections!)
    public PlayerCharacter? PlayerCharacter { get; set; }
    public List<Npc> Npcs { get; set; } = new();
    public List<Quest> Quests { get; set; } = new();
    public List<MemoryEntry> MemoryEntries { get; set; } = new();
}
