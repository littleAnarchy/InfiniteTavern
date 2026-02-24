using FluentAssertions;
using InfiniteTavern.Application.Services;

namespace InfiniteTavern.Tests.Services;

public class DiceServiceTests
{
    private readonly DiceService _service;

    public DiceServiceTests()
    {
        _service = new DiceService();
    }

    [Theory]
    [InlineData("1d20", 1, 20)]
    [InlineData("1d6", 1, 6)]
    [InlineData("1d4", 1, 4)]
    [InlineData("1d100", 1, 100)]
    public void Roll_SingleDice_ReturnsValueInRange(string notation, int min, int max)
    {
        // Act
        var result = _service.Roll(notation);

        // Assert
        result.Should().BeInRange(min, max);
    }

    [Theory]
    [InlineData("2d6", 2, 12)]
    [InlineData("3d6", 3, 18)]
    [InlineData("4d6", 4, 24)]
    public void Roll_MultipleDice_ReturnsValueInRange(string notation, int min, int max)
    {
        // Act
        var result = _service.Roll(notation);

        // Assert
        result.Should().BeInRange(min, max);
    }

    [Fact]
    public void Roll_1d20_ReturnsConsistentResults()
    {
        // Arrange
        var results = new List<int>();

        // Act - Roll 100 times
        for (int i = 0; i < 100; i++)
        {
            results.Add(_service.Roll("1d20"));
        }

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeInRange(1, 20));
        results.Distinct().Should().HaveCountGreaterThan(1, "rolls should produce different values");
    }

    [Fact]
    public void Roll_3d6_ProducesDistribution()
    {
        // Arrange
        var results = new List<int>();

        // Act - Roll 100 times
        for (int i = 0; i < 100; i++)
        {
            results.Add(_service.Roll("3d6"));
        }

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeInRange(3, 18));
        results.Average().Should().BeInRange(8, 13, "3d6 should average around 10-11");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("d20")]
    [InlineData("1d")]
    [InlineData("")]
    public void Roll_InvalidNotation_ThrowsArgumentException(string notation)
    {
        // Act & Assert
        _service.Invoking(s => s.Roll(notation))
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid dice expression*");
    }

    [Fact]
    public void Roll_ZeroDice_ThrowsArgumentException()
    {
        // Act & Assert
        _service.Invoking(s => s.Roll("0d6"))
            .Should().Throw<ArgumentException>()
            .WithMessage("Dice count must be between 1 and 100");
    }

    [Fact]
    public void Roll_NegativeDice_ThrowsArgumentException()
    {
        // Act & Assert
        _service.Invoking(s => s.Roll("-1d6"))
            .Should().Throw<ArgumentException>()
            .WithMessage("Invalid dice expression*");
    }

    [Fact]
    public void Roll_CharacterStats_ProducesRealisticValues()
    {
        // Arrange - Test character stat generation (3d6)
        var stats = new List<int>();

        // Act - Generate 6 stats (like in character creation)
        for (int i = 0; i < 6; i++)
        {
            stats.Add(_service.Roll("3d6"));
        }

        // Assert
        stats.Should().AllSatisfy(stat => stat.Should().BeInRange(3, 18));
        stats.Average().Should().BeInRange(8, 13, "average D&D stat with 3d6 is ~10.5");
    }
}
