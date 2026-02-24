using System.Text.RegularExpressions;

namespace InfiniteTavern.Application.Services;

public interface IDiceService
{
    int Roll(string expression);
}

public class DiceService : IDiceService
{
    private readonly Random _random = new();

    public int Roll(string expression)
    {
        // Parse dice expression like "1d20", "2d6+3", "3d8-2"
        expression = expression.Trim().ToLower();

        var match = Regex.Match(expression, @"^(\d+)d(\d+)([\+\-]\d+)?$");

        if (!match.Success)
        {
            throw new ArgumentException($"Invalid dice expression: {expression}");
        }

        int count = int.Parse(match.Groups[1].Value);
        int sides = int.Parse(match.Groups[2].Value);
        int modifier = 0;

        if (match.Groups[3].Success)
        {
            modifier = int.Parse(match.Groups[3].Value);
        }

        if (count <= 0 || count > 100)
        {
            throw new ArgumentException("Dice count must be between 1 and 100");
        }

        if (sides <= 0 || sides > 1000)
        {
            throw new ArgumentException("Dice sides must be between 1 and 1000");
        }

        int total = 0;
        for (int i = 0; i < count; i++)
        {
            total += _random.Next(1, sides + 1);
        }

        return total + modifier;
    }
}
