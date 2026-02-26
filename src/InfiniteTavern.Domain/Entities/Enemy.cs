namespace InfiniteTavern.Domain.Entities;

public class Enemy
{
    public string Name { get; set; } = string.Empty;
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public bool IsAlive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Attack rating used in hit/dodge calculation.
    /// Tiers: Weak 2-3, Normal 4-6, Strong 7-9, Boss 10-12.
    /// </summary>
    public int Attack { get; set; } = 3;
}
