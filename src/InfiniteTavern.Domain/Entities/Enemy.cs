namespace InfiniteTavern.Domain.Entities;

public class Enemy
{
    public string Name { get; set; } = string.Empty;
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public bool IsAlive { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
