namespace InfiniteTavern.Domain.Entities;

public class Npc
{
    public string Name { get; set; } = string.Empty;
    public string PersonalityTraits { get; set; } = string.Empty;
    public string RelationshipToPlayer { get; set; } = "Neutral";
    public string CurrentLocation { get; set; } = string.Empty;
    public bool IsAlive { get; set; } = true;
}
