using FluentAssertions;
using InfiniteTavern.Application.Services;
using InfiniteTavern.Domain.Entities;

namespace InfiniteTavern.Tests.Services;

public class PromptBuilderServiceTests
{
    private readonly PromptBuilderService _service;

    public PromptBuilderServiceTests()
    {
        _service = new PromptBuilderService();
    }

    [Fact]
    public void BuildSystemPrompt_DefaultEnglish_ContainsEnglishInstruction()
    {
        // Act
        var result = _service.BuildSystemPrompt();

        // Assert
        result.Should().Contain("IMPORTANT: Respond in English language");
        result.Should().Contain("You are the Dungeon Master of Infinite Tavern");
        result.Should().Contain("CRITICAL RULES");
    }

    [Fact]
    public void BuildSystemPrompt_Ukrainian_ContainsUkrainianInstruction()
    {
        // Act
        var result = _service.BuildSystemPrompt("Ukrainian");

        // Assert
        result.Should().Contain("IMPORTANT: Respond ONLY in Ukrainian language");
        result.Should().Contain("All narrative, dialogues, and descriptions must be in Ukrainian");
    }

    [Theory]
    [InlineData("English")]
    [InlineData("english")]
    [InlineData("ENGLISH")]
    public void BuildSystemPrompt_EnglishVariants_WorksCaseInsensitive(string language)
    {
        // Act
        var result = _service.BuildSystemPrompt(language);

        // Assert
        result.Should().Contain("IMPORTANT: Respond in English language");
    }

    [Theory]
    [InlineData("Ukrainian")]
    [InlineData("ukrainian")]
    [InlineData("UKRAINIAN")]
    public void BuildSystemPrompt_UkrainianVariants_WorksCaseInsensitive(string language)
    {
        // Act
        var result = _service.BuildSystemPrompt(language);

        // Assert
        result.Should().Contain("IMPORTANT: Respond ONLY in Ukrainian language");
    }

    [Fact]
    public void BuildSystemPrompt_ContainsGameRules()
    {
        // Act
        var result = _service.BuildSystemPrompt();

        // Assert
        result.Should().Contain("EVENT TYPES");
        result.Should().Contain("SKILL CHECKS");
        result.Should().Contain("RESPONSE FORMAT");
        result.Should().Contain("damage");
        result.Should().Contain("heal");
        result.Should().Contain("item_found");
        result.Should().Contain("gold_found");
    }

    [Fact]
    public void BuildUserPrompt_IncludesPlayerCharacter()
    {
        // Arrange
        var (session, player, _, _, _, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), new(), "I look around");

        // Assert
        result.Should().Contain("=== PLAYER CHARACTER ===");
        result.Should().Contain("Name: TestHero");
        result.Should().Contain("Race: Human");
        result.Should().Contain("Class: Warrior");
        result.Should().Contain("HP: 20/20");
        result.Should().Contain("Strength: 15");
        result.Should().Contain("Gold: 10");
    }

    [Fact]
    public void BuildUserPrompt_IncludesGameState()
    {
        // Arrange
        var (session, player, _, _, _, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), new(), "I look around");

        // Assert
        result.Should().Contain("=== GAME STATE ===");
        result.Should().Contain("Turn: 1");
        result.Should().Contain("Location: The Infinite Tavern");
        result.Should().Contain("Time: Evening");
    }

    [Fact]
    public void BuildUserPrompt_IncludesInventory()
    {
        // Arrange
        var (session, player, _, _, _, _) = CreateTestData();
        player.Inventory = new List<Item>
        {
            new() { Name = "Iron Sword", Quantity = 1, IsEquipped = true },
            new() { Name = "Health Potion", Quantity = 3, IsEquipped = false }
        };

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), new(), "I look around");

        // Assert
        result.Should().Contain("Iron Sword x1 (equipped)");
        result.Should().Contain("Health Potion x3");
    }

    [Fact]
    public void BuildUserPrompt_EmptyInventory_ShowsEmpty()
    {
        // Arrange
        var (session, player, _, _, _, _) = CreateTestData();
        player.Inventory = new List<Item>();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), new(), "I look around");

        // Assert
        result.Should().Contain("Inventory: Empty");
    }

    [Fact]
    public void BuildUserPrompt_IncludesNearbyNpcs()
    {
        // Arrange
        var (session, player, npcs, _, _, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, npcs, new(), new(), new(), "I look around");

        // Assert
        result.Should().Contain("=== NPCs IN SCENE ===");
        result.Should().Contain("Garrick: Friendly tavern keeper");
        result.Should().Contain("Relationship: Welcoming");
        result.Should().Contain("Status: Alive");
    }

    [Fact]
    public void BuildUserPrompt_IncludesActiveQuests()
    {
        // Arrange
        var (session, player, _, quests, _, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), quests, new(), new(), "I look around");

        // Assert
        result.Should().Contain("=== ACTIVE QUESTS ===");
        result.Should().Contain("Find the Lost Artifact");
        result.Should().Contain("Search the ancient ruins");
    }

    [Fact]
    public void BuildUserPrompt_IncludesRecentMemories()
    {
        // Arrange
        var (session, player, _, _, recentMemories, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), recentMemories, new(), "I look around");

        // Assert
        result.Should().Contain("=== RECENT EVENTS ===");
        result.Should().Contain("Player entered the tavern");
    }

    [Fact]
    public void BuildUserPrompt_IncludesImportantMemories()
    {
        // Arrange
        var (session, player, _, _, _, importantMemories) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), importantMemories, "I look around");

        // Assert
        result.Should().Contain("=== IMPORTANT MEMORIES ===");
        result.Should().Contain("[Event] Found ancient map");
    }

    [Fact]
    public void BuildUserPrompt_IncludesPlayerAction()
    {
        // Arrange
        var (session, player, _, _, _, _) = CreateTestData();

        // Act
        var result = _service.BuildUserPrompt(session, player, new(), new(), new(), new(), "I draw my sword");

        // Assert
        result.Should().Contain("=== PLAYER ACTION ===");
        result.Should().Contain("I draw my sword");
    }

    private (GameSession, PlayerCharacter, List<Npc>, List<Quest>, List<MemoryEntry>, List<MemoryEntry>) CreateTestData()
    {
        var player = new PlayerCharacter
        {
            Name = "TestHero",
            Race = "Human",
            Class = "Warrior",
            Level = 1,
            HP = 20,
            MaxHP = 20,
            Strength = 15,
            Dexterity = 12,
            Intelligence = 10,
            Gold = 10,
            Inventory = new List<Item>()
        };

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerName = "TestPlayer",
            CurrentLocation = "The Infinite Tavern",
            WorldTime = "Evening",
            TurnNumber = 1,
            PlayerCharacter = player
        };

        var npcs = new List<Npc>
        {
            new()
            {
                Name = "Garrick",
                PersonalityTraits = "Friendly tavern keeper",
                RelationshipToPlayer = "Welcoming",
                IsAlive = true
            }
        };

        var quests = new List<Quest>
        {
            new()
            {
                Title = "Find the Lost Artifact",
                Description = "Search the ancient ruins",
                Status = QuestStatus.Active
            }
        };

        var recentMemories = new List<MemoryEntry>
        {
            new()
            {
                Content = "Player entered the tavern",
                Type = MemoryType.Event,
                ImportanceScore = 5
            }
        };

        var importantMemories = new List<MemoryEntry>
        {
            new()
            {
                Content = "Found ancient map",
                Type = MemoryType.Event,
                ImportanceScore = 10
            }
        };

        return (session, player, npcs, quests, recentMemories, importantMemories);
    }
}
