using InfiniteTavern.Application.Models;
using InfiniteTavern.Domain.Entities;
using InfiniteTavern.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace InfiniteTavern.Application.Services;

public interface IGameService
{
    Task<NewGameResponse> CreateNewGameAsync(NewGameRequest request);
    Task<TurnResponse> ProcessTurnAsync(TurnRequest request);
}

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IAIService _aiService;
    private readonly IPromptBuilderService _promptBuilder;
    private readonly IDiceService _diceService;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameRepository repository,
        IAIService aiService,
        IPromptBuilderService promptBuilder,
        IDiceService diceService,
        ILogger<GameService> logger)
    {
        _repository = repository;
        _aiService = aiService;
        _promptBuilder = promptBuilder;
        _diceService = diceService;
        _logger = logger;
    }

    public async Task<NewGameResponse> CreateNewGameAsync(NewGameRequest request)
    {
        var playerCharacter = new PlayerCharacter
        {
            Name = request.CharacterName,
            Race = request.Race,
            Class = request.Class,
            Level = 1,
            MaxHP = 20,
            HP = 20,
            Strength = _diceService.Roll("3d6"),
            Dexterity = _diceService.Roll("3d6"),
            Intelligence = _diceService.Roll("3d6"),
            Gold = 10,
            Inventory = new List<Item>
            {
                new Item
                {
                    Name = request.Class == "Warrior" ? "Iron Sword" : request.Class == "Wizard" ? "Wooden Staff" : "Iron Dagger",
                    Type = "Weapon",
                    Description = "Your starting weapon",
                    Quantity = 1,
                    IsEquipped = true,
                    Bonuses = new Dictionary<string, int> { { "Strength", 2 } }
                },
                new Item
                {
                    Name = "Health Potion",
                    Type = "Potion",
                    Description = "Restores 10 HP",
                    Quantity = 2,
                    IsEquipped = false
                }
            }
        };

        // Generate opening story with AI
        var openingStory = await GenerateOpeningStoryAsync(playerCharacter, request.Language);

        var tavernKeeper = new Npc
        {
            Name = "Garrick the Tavern Keeper",
            PersonalityTraits = "Friendly, wise, knows many secrets",
            RelationshipToPlayer = "Welcoming",
            CurrentLocation = "The Infinite Tavern",
            IsAlive = true
        };

        var initialMemory = new MemoryEntry
        {
            Content = $"{playerCharacter.Name}, a {playerCharacter.Race} {playerCharacter.Class}, enters the mystical Infinite Tavern for the first time.",
            Type = MemoryType.Event,
            ImportanceScore = 10,
            CreatedAt = DateTime.UtcNow
        };

        var gameSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerName = request.PlayerName,
            CurrentLocation = "The Infinite Tavern",
            WorldTime = "Evening",
            Language = request.Language,
            TurnNumber = 0,
            CreatedAt = DateTime.UtcNow,
            PlayerCharacter = playerCharacter,
            Npcs = new List<Npc> { tavernKeeper },
            MemoryEntries = new List<MemoryEntry> { initialMemory },
            Quests = new List<Quest>()
        };

        await _repository.CreateAsync(gameSession);

        _logger.LogInformation("Created new game session {SessionId} for player {PlayerName}",
            gameSession.Id, request.PlayerName);

        return new NewGameResponse
        {
            GameSessionId = gameSession.Id,
            Message = openingStory,
            PlayerStats = new PlayerStats
            {
                Name = playerCharacter.Name,
                Race = playerCharacter.Race,
                Class = playerCharacter.Class,
                Level = playerCharacter.Level,
                HP = playerCharacter.HP,
                MaxHP = playerCharacter.MaxHP,
                Strength = playerCharacter.Strength,
                Dexterity = playerCharacter.Dexterity,
                Intelligence = playerCharacter.Intelligence,
                Inventory = playerCharacter.Inventory.Select(i => new ItemDto
                {
                    Name = i.Name,
                    Type = i.Type,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    IsEquipped = i.IsEquipped,
                    Bonuses = i.Bonuses
                }).ToList(),
                Gold = playerCharacter.Gold
            }
        };
    }

    private async Task<string> GenerateOpeningStoryAsync(PlayerCharacter player, string language)
    {
        var isUkrainian = language.Equals("Ukrainian", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isUkrainian
            ? @"Ти - креативний Майстер Підземель, що розпочинає нову фентезійну RPG пригоду.
Створи УНІКАЛЬНУ, захоплюючу вступну сцену (2-3 параграфи) для гравця, що входить до легендарної Нескінченної Таверни.

ВАЖЛИВО: Придумай ОРИГІНАЛЬНИЙ початковий сценарій (як гравець опинився тут):
- Можливо, вони шукали щось конкретне
- Або тікали від небезпеки/ворогів
- Або отримали таємничий лист/знак
- Або випадково знайшли таверну під час подорожі
- Або почули легенди і прийшли навмисно
- Можна комбінувати різні мотиви

Пиши від другої особи (ти/твій). Створи атмосферу, настрій, унікальні деталі.
Дай інтригуючу зачіпку, яка змусить гравця бажати досліджувати.
Закінчи запитанням або ситуацією, що спонукає гравця до дії."
            : @"You are a creative Dungeon Master starting a new fantasy RPG adventure.
Create a UNIQUE, engaging opening scene (2-3 paragraphs) for a player entering the legendary Infinite Tavern.

IMPORTANT: Invent an ORIGINAL starting scenario (how the player arrived here):
- Perhaps they were searching for something specific
- Or fleeing from danger/enemies
- Or received a mysterious letter/sign
- Or stumbled upon the tavern while traveling
- Or heard legends and came deliberately
- Feel free to combine different motivations

Write in second person (you/your). Create atmosphere, mood, unique details.
Give an intriguing hook that makes the player want to explore.
End with a question or situation that prompts the player to take action.";

        var userPrompt = $@"Character: {player.Name}, a level {player.Level} {player.Race} {player.Class}

Stats:
- HP: {player.HP}/{player.MaxHP}
- Strength: {player.Strength}
- Dexterity: {player.Dexterity}
- Intelligence: {player.Intelligence}

Starting equipment:
{string.Join("\n", player.Inventory.Select(i => $"- {i.Name}"))}

Create an immersive, ORIGINAL opening scene that incorporates the character's race, class, and personality.
Make it unique and memorable - different from other adventures!";

        try
        {
            var response = await _aiService.GenerateResponseAsync(systemPrompt, userPrompt);
            return response.Narrative;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate opening story, using fallback");
            return isUkrainian
                ? $"Вітаємо, {player.Name}! Ви - {player.Race} {player.Class}, який щойно прибув до легендарної Нескінченної Таверни. Тепле сяйво ліхтарів освітлює дерев'яні столи, де пригодники з далеких земель діляться розповідями про славу. Гаррік, таверняр, знавіще кивує вам. Що ви робите?"
                : $"Welcome, {player.Name}! You are a {player.Race} {player.Class} who has just arrived at the legendary Infinite Tavern. The warm glow of lanterns illuminates wooden tables where adventurers from distant lands share tales of glory. Garrick, the tavern keeper, nods at you knowingly. What do you do?";
        }
    }

    public async Task<TurnResponse> ProcessTurnAsync(TurnRequest request)
    {
        // Load game session - single query gets entire document!
        var session = await _repository.GetByIdAsync(request.GameSessionId);

        if (session == null)
        {
            throw new InvalidOperationException($"Game session {request.GameSessionId} not found");
        }

        if (session.PlayerCharacter == null)
        {
            throw new InvalidOperationException($"Player character not found for session {request.GameSessionId}");
        }

        // Get nearby NPCs
        var nearbyNpcs = session.Npcs
            .Where(n => n.CurrentLocation == session.CurrentLocation && n.IsAlive)
            .ToList();

        // Get active quests
        var activeQuests = session.Quests
            .Where(q => q.Status == QuestStatus.Active)
            .ToList();

        // Get last 5 turns (recent memories)
        var recentMemories = session.MemoryEntries
            .Where(m => m.Type == MemoryType.Event)
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        // Get top 3 important memories
        var importantMemories = session.MemoryEntries
            .Where(m => m.Type != MemoryType.Event)
            .OrderByDescending(m => m.ImportanceScore)
            .Take(3)
            .ToList();

        // Build prompts
        var systemPrompt = _promptBuilder.BuildSystemPrompt(session.Language);
        var userPrompt = _promptBuilder.BuildUserPrompt(
            session,
            session.PlayerCharacter,
            nearbyNpcs,
            activeQuests,
            recentMemories,
            importantMemories,
            request.PlayerAction);

        // Call AI Service
        var claudeResponse = await _aiService.GenerateResponseAsync(systemPrompt, userPrompt);

        // Apply events
        var appliedEvents = new List<string>();
        foreach (var gameEvent in claudeResponse.Events)
        {
            ApplyEvent(session, gameEvent, appliedEvents);
        }

        // Handle location change
        if (claudeResponse.LocationChange != null)
        {
            session.CurrentLocation = claudeResponse.LocationChange.NewLocation;
            appliedEvents.Add($"Moved to {claudeResponse.LocationChange.NewLocation}");
        }

        // Add new NPCs
        foreach (var newNpc in claudeResponse.NewNpcs)
        {
            var npc = new Npc
            {
                Name = newNpc.Name,
                PersonalityTraits = newNpc.PersonalityTraits,
                CurrentLocation = newNpc.CurrentLocation,
                RelationshipToPlayer = "Neutral",
                IsAlive = true
            };
            session.Npcs.Add(npc);
            appliedEvents.Add($"Met new NPC: {newNpc.Name}");
        }

        // Update quests
        foreach (var questUpdate in claudeResponse.QuestUpdates)
        {
            var quest = session.Quests.FirstOrDefault(q => q.Title == questUpdate.QuestTitle);
            if (quest != null && Enum.TryParse<QuestStatus>(questUpdate.Status, out var status))
            {
                quest.Status = status;
                appliedEvents.Add($"Quest '{quest.Title}' is now {status}");
            }
        }

        // Process skill checks
        var diceRolls = new List<DiceRollResult>();
        foreach (var skillCheck in claudeResponse.SkillChecks)
        {
            var rollResult = ProcessSkillCheck(session.PlayerCharacter, skillCheck);
            diceRolls.Add(rollResult);
            
            var successText = rollResult.Success ? "succeeded" : "failed";
            appliedEvents.Add($"Skill check ({skillCheck.Attribute}): {successText}!");
        }

        // Increment turn number
        session.TurnNumber++;

        // Save memory entry
        var memoryEntry = new MemoryEntry
        {
            Content = $"Turn {session.TurnNumber}: Player action: {request.PlayerAction}. {claudeResponse.Narrative.Substring(0, Math.Min(200, claudeResponse.Narrative.Length))}",
            Type = MemoryType.Event,
            ImportanceScore = 5,
            CreatedAt = DateTime.UtcNow
        };
        session.MemoryEntries.Add(memoryEntry);

        // Check if we need to generate a summary (every 10 turns)
        if (session.TurnNumber % 10 == 0)
        {
            await GenerateSummaryAsync(session);
        }

        // Update entire session - single write operation!
        await _repository.UpdateAsync(session);

        _logger.LogInformation("Processed turn {TurnNumber} for session {SessionId}",
            session.TurnNumber, session.Id);

        return new TurnResponse
        {
            Narrative = claudeResponse.Narrative,
            PlayerHP = session.PlayerCharacter.HP,
            MaxPlayerHP = session.PlayerCharacter.MaxHP,
            CurrentLocation = session.CurrentLocation,
            AppliedEvents = appliedEvents,
            Inventory = session.PlayerCharacter.Inventory.Select(i => new ItemDto
            {
                Name = i.Name,
                Type = i.Type,
                Description = i.Description,
                Quantity = i.Quantity,
                IsEquipped = i.IsEquipped,
                Bonuses = i.Bonuses
            }).ToList(),
            Gold = session.PlayerCharacter.Gold,
            DiceRolls = diceRolls
        };
    }

    private void ApplyEvent(GameSession session, GameEvent gameEvent, List<string> appliedEvents)
    {
        switch (gameEvent.Type.ToLower())
        {
            case "damage":
                if (gameEvent.Target.ToLower() == "player" && session.PlayerCharacter != null)
                {
                    session.PlayerCharacter.HP = Math.Max(0, session.PlayerCharacter.HP - gameEvent.Amount);
                    appliedEvents.Add($"Player took {gameEvent.Amount} damage: {gameEvent.Reason}");

                    if (session.PlayerCharacter.HP == 0)
                    {
                        appliedEvents.Add("Player has fallen!");
                    }
                }
                else
                {
                    var npc = session.Npcs.FirstOrDefault(n =>
                        n.Name.Equals(gameEvent.Target, StringComparison.OrdinalIgnoreCase) && n.IsAlive);
                    if (npc != null)
                    {
                        npc.IsAlive = false;
                        appliedEvents.Add($"{npc.Name} was defeated: {gameEvent.Reason}");
                    }
                }
                break;

            case "heal":
                if (gameEvent.Target.ToLower() == "player" && session.PlayerCharacter != null)
                {
                    session.PlayerCharacter.HP = Math.Min(
                        session.PlayerCharacter.MaxHP,
                        session.PlayerCharacter.HP + gameEvent.Amount);
                    appliedEvents.Add($"Player healed {gameEvent.Amount} HP: {gameEvent.Reason}");
                }
                break;

            case "item_found":
                if (session.PlayerCharacter != null)
                {
                    var existingItem = session.PlayerCharacter.Inventory
                        .FirstOrDefault(i => i.Name.Equals(gameEvent.Reason, StringComparison.OrdinalIgnoreCase));

                    if (existingItem != null)
                    {
                        existingItem.Quantity += gameEvent.Amount > 0 ? gameEvent.Amount : 1;
                    }
                    else
                    {
                        session.PlayerCharacter.Inventory.Add(new Item
                        {
                            Name = gameEvent.Reason,
                            Type = "Miscellaneous",
                            Description = "Found during adventure",
                            Quantity = gameEvent.Amount > 0 ? gameEvent.Amount : 1
                        });
                    }
                    appliedEvents.Add($"Found: {gameEvent.Reason} x{(gameEvent.Amount > 0 ? gameEvent.Amount : 1)}");
                }
                break;

            case "item_lost":
                if (session.PlayerCharacter != null)
                {
                    var itemToRemove = session.PlayerCharacter.Inventory
                        .FirstOrDefault(i => i.Name.Equals(gameEvent.Reason, StringComparison.OrdinalIgnoreCase));

                    if (itemToRemove != null)
                    {
                        var amountToRemove = gameEvent.Amount > 0 ? gameEvent.Amount : itemToRemove.Quantity;
                        itemToRemove.Quantity -= amountToRemove;

                        if (itemToRemove.Quantity <= 0)
                        {
                            session.PlayerCharacter.Inventory.Remove(itemToRemove);
                        }

                        appliedEvents.Add($"Lost: {gameEvent.Reason} x{amountToRemove}");
                    }
                }
                break;

            case "gold_found":
                if (session.PlayerCharacter != null)
                {
                    session.PlayerCharacter.Gold += gameEvent.Amount;
                    appliedEvents.Add($"Found {gameEvent.Amount} gold: {gameEvent.Reason}");
                }
                break;

            case "gold_spent":
                if (session.PlayerCharacter != null)
                {
                    session.PlayerCharacter.Gold = Math.Max(0, session.PlayerCharacter.Gold - gameEvent.Amount);
                    appliedEvents.Add($"Spent {gameEvent.Amount} gold: {gameEvent.Reason}");
                }
                break;

            default:
                _logger.LogWarning("Unknown event type: {EventType}", gameEvent.Type);
                break;
        }
    }

    private DiceRollResult ProcessSkillCheck(PlayerCharacter player, SkillCheck skillCheck)
    {
        // Get attribute value
        var attributeValue = skillCheck.Attribute.ToLower() switch
        {
            "strength" => player.Strength,
            "dexterity" => player.Dexterity,
            "intelligence" => player.Intelligence,
            _ => 0
        };

        // Roll 1d20
        var diceRoll = _diceService.Roll("1d20");
        
        // Calculate total (d20 + attribute modifier)
        // Modifier = (attribute - 10) / 2
        var modifier = (attributeValue - 10) / 2;
        var total = diceRoll + modifier;
        
        // Check success
        var success = total >= skillCheck.Difficulty;

        return new DiceRollResult
        {
            Attribute = skillCheck.Attribute,
            AttributeValue = attributeValue,
            DiceRoll = diceRoll,
            Total = total,
            Difficulty = skillCheck.Difficulty,
            Success = success,
            Purpose = skillCheck.Purpose
        };
    }

    private async Task GenerateSummaryAsync(GameSession session)
    {
        var recentEvents = session.MemoryEntries
            .Where(m => m.Type == MemoryType.Event)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToList();

        if (!recentEvents.Any())
        {
            return;
        }

        var summaryPrompt = "Summarize the following events into a brief paragraph:\n\n" +
            string.Join("\n", recentEvents.Select(e => e.Content));

        var systemPrompt = "You are a helpful assistant. Summarize the events concisely in 2-3 sentences.";

        try
        {
            var claudeResponse = await _aiService.GenerateResponseAsync(systemPrompt, summaryPrompt);

            var summary = new MemoryEntry
            {
                Content = claudeResponse.Narrative,
                Type = MemoryType.Summary,
                ImportanceScore = 10,
                CreatedAt = DateTime.UtcNow
            };

            session.MemoryEntries.Add(summary);

            _logger.LogInformation("Generated summary for session {SessionId} at turn {TurnNumber}",
                session.Id, session.TurnNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate summary for session {SessionId}", session.Id);
        }
    }
}
