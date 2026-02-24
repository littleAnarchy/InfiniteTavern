namespace InfiniteTavern.Domain.Entities;

public class Item
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Miscellaneous"; // Weapon, Armor, Potion, Miscellaneous
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsEquipped { get; set; } = false;
    public Dictionary<string, int> Bonuses { get; set; } = new(); // e.g., {"Strength": 2, "HP": 10}
}
