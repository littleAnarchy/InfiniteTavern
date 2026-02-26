using InfiniteTavern.Application.Models;
using InfiniteTavern.Domain.Entities;
using InfiniteTavern.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace InfiniteTavern.Application.Services;

public interface IGameService
{
    Task<NewGameResponse> CreateNewGameAsync(NewGameRequest request);
    Task<TurnResponse> ProcessTurnAsync(TurnRequest request);
    Task<TokenUsageStats> GetTokenUsageStatsAsync(Guid gameSessionId);
}

public class GameService : IGameService
{
    private readonly IGameRepository _repository;
    private readonly IAIService _aiService;
    private readonly IPromptBuilderService _promptBuilder;
    private readonly IDiceService _diceService;
    private readonly IGameEventHandlerService _eventHandler;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IGameRepository repository,
        IAIService aiService,
        IPromptBuilderService promptBuilder,
        IDiceService diceService,
        IGameEventHandlerService eventHandler,
        ILogger<GameService> logger)
    {
        _repository = repository;
        _aiService = aiService;
        _promptBuilder = promptBuilder;
        _diceService = diceService;
        _eventHandler = eventHandler;
        _logger = logger;
    }

    private void RecordTokenUsage(GameSession session, AIResponse aiResponse, string callType)
    {
        if (aiResponse.Usage != null)
        {
            session.TokenUsageHistory.Add(new TokenUsageEntry
            {
                Timestamp = DateTime.UtcNow,
                TurnNumber = session.TurnNumber,
                CallType = callType,
                InputTokens = aiResponse.Usage.InputTokens,
                OutputTokens = aiResponse.Usage.OutputTokens,
                TotalTokens = aiResponse.Usage.TotalTokens,
                ModelName = aiResponse.Usage.ModelName
            });
        }
    }

    private (int str, int dex, int con, int intel, int wis, int cha, int maxHp) RollStatsForClass(string characterClass)
    {
        return characterClass switch
        {
            "Warrior" => (
                str:   _diceService.Roll("2d6") + 3,
                dex:   _diceService.Roll("2d6") + 1,
                con:   _diceService.Roll("2d6") + 3,
                intel: _diceService.Roll("2d6"),
                wis:   _diceService.Roll("2d6") + 1,
                cha:   _diceService.Roll("2d6"),
                maxHp: 12
            ),
            "Wizard" => (
                str:   _diceService.Roll("2d6"),
                dex:   _diceService.Roll("2d6") + 1,
                con:   _diceService.Roll("2d6"),
                intel: _diceService.Roll("2d6") + 3,
                wis:   _diceService.Roll("2d6") + 2,
                cha:   _diceService.Roll("2d6") + 1,
                maxHp: 6
            ),
            "Rogue" => (
                str:   _diceService.Roll("2d6") + 1,
                dex:   _diceService.Roll("2d6") + 3,
                con:   _diceService.Roll("2d6") + 1,
                intel: _diceService.Roll("2d6") + 2,
                wis:   _diceService.Roll("2d6"),
                cha:   _diceService.Roll("2d6") + 2,
                maxHp: 8
            ),
            "Cleric" => (
                str:   _diceService.Roll("2d6") + 1,
                dex:   _diceService.Roll("2d6"),
                con:   _diceService.Roll("2d6") + 2,
                intel: _diceService.Roll("2d6") + 1,
                wis:   _diceService.Roll("2d6") + 3,
                cha:   _diceService.Roll("2d6") + 2,
                maxHp: 10
            ),
            "Ranger" => (
                str:   _diceService.Roll("2d6") + 2,
                dex:   _diceService.Roll("2d6") + 3,
                con:   _diceService.Roll("2d6") + 2,
                intel: _diceService.Roll("2d6") + 1,
                wis:   _diceService.Roll("2d6") + 2,
                cha:   _diceService.Roll("2d6"),
                maxHp: 10
            ),
            _ => (
                str:   _diceService.Roll("2d6") + 1,
                dex:   _diceService.Roll("2d6") + 1,
                con:   _diceService.Roll("2d6") + 1,
                intel: _diceService.Roll("2d6") + 1,
                wis:   _diceService.Roll("2d6") + 1,
                cha:   _diceService.Roll("2d6") + 1,
                maxHp: 8
            ),
        };
    }

    public async Task<NewGameResponse> CreateNewGameAsync(NewGameRequest request)
    {
        var (str, dex, con, intel, wis, cha, maxHp) = RollStatsForClass(request.Class);
        var playerCharacter = new PlayerCharacter
        {
            Name = request.CharacterName,
            Race = request.Race,
            Class = request.Class,
            Level = 1,
            MaxHP = maxHp,
            HP = maxHp,
            Strength = str,
            Dexterity = dex,
            Constitution = con,
            Intelligence = intel,
            Wisdom = wis,
            Charisma = cha,
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
        var (openingStory, suggestedActions, openingResponse) = await GenerateOpeningStoryAsync(playerCharacter, request.Language, request.UseDefaultCampaign);

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

        // Record token usage if AI was used
        if (openingResponse != null)
        {
            RecordTokenUsage(gameSession, openingResponse, "OpeningStory");
        }

        await _repository.CreateAsync(gameSession);

        _logger.LogInformation("Created new game session {SessionId} for character {CharacterName}",
            gameSession.Id, request.CharacterName);

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
                Constitution = playerCharacter.Constitution,
                Wisdom = playerCharacter.Wisdom,
                Charisma = playerCharacter.Charisma,
                Defense = playerCharacter.Defense,
                Experience = playerCharacter.Experience,
                ExperienceToNextLevel = PlayerCharacter.XpToNextLevel(playerCharacter.Level),
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
            },
            SuggestedActions = suggestedActions
        };
    }

    private async Task<(string narrative, List<string> suggestedActions, AIResponse? response)> GenerateOpeningStoryAsync(PlayerCharacter player, string language, bool useDefault)
    {
        // If using default campaign, return fallback immediately
        if (useDefault)
        {
            var narrative = PromptTemplates.GetDefaultOpeningNarrative(player.Name, player.Race, player.Class, language);
            var actions = PromptTemplates.GetDefaultSuggestedActions(language);
            return (narrative, actions, null);
        }

        var languageInstruction = PromptTemplates.GetLanguageInstruction(language);
        var systemPrompt = string.Format(PromptTemplates.OpeningStorySystemPrompt, languageInstruction);
        var userPrompt = PromptTemplates.BuildOpeningStoryUserPrompt(player);

        try
        {
            var response = await _aiService.GenerateResponseAsync(systemPrompt, userPrompt, useJsonFormat: true);
            return (response.Narrative, response.SuggestedActions, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate opening story, using fallback");
            var narrative = PromptTemplates.GetDefaultOpeningNarrative(player.Name, player.Race, player.Class, language);
            var actions = PromptTemplates.GetDefaultSuggestedActions(language);
            return (narrative, actions, null);
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

        if (session.IsGameOver)
        {
            throw new InvalidOperationException("Game is over. Please start a new game.");
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
        var systemPrompt = session.IsInCombat 
            ? _promptBuilder.BuildSystemPromptForCombat(session.Language)
            : _promptBuilder.BuildSystemPrompt(session.Language);
        var userPrompt = _promptBuilder.BuildUserPrompt(
            session,
            session.PlayerCharacter,
            nearbyNpcs,
            activeQuests,
            recentMemories,
            importantMemories,
            request.PlayerAction);

        // Call AI Service
        var aiResponse = await _aiService.GenerateResponseAsync(systemPrompt, userPrompt);

        // Record token usage
        var callType = session.IsInCombat ? "Combat" : "Turn";
        RecordTokenUsage(session, aiResponse, callType);

        // Apply events
        var appliedEvents = new List<string>();
        var levelBefore = session.PlayerCharacter.Level;
        foreach (var gameEvent in aiResponse.Events)
        {
            _eventHandler.ApplyEvent(session, gameEvent, appliedEvents);
        }

        // Handle location change
        if (aiResponse.LocationChange != null)
        {
            session.CurrentLocation = aiResponse.LocationChange.NewLocation;
            if (Enum.TryParse<LocationType>(aiResponse.LocationChange.LocationType, true, out var locType))
            {
                session.CurrentLocationType = locType;
            }
            appliedEvents.Add($"Moved to {aiResponse.LocationChange.NewLocation}");
        }

        // Add new NPCs
        foreach (var newNpc in aiResponse.NewNpcs)
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

        // Handle enemies (combat)
        if (aiResponse.Enemies.Any())
        {
            // Update or create enemies
            foreach (var enemyResponse in aiResponse.Enemies)
            {
                var existingEnemy = session.Enemies.FirstOrDefault(e => 
                    e.Name.Equals(enemyResponse.Name, StringComparison.OrdinalIgnoreCase));
                
                if (existingEnemy != null)
                {
                    // Update existing enemy
                    existingEnemy.HP = enemyResponse.HP;
                    existingEnemy.MaxHP = enemyResponse.MaxHP;
                    existingEnemy.Description = enemyResponse.Description;
                    existingEnemy.IsAlive = enemyResponse.HP > 0;
                    // Preserve attack unless AI explicitly re-specifies it
                    if (enemyResponse.Attack > 0) existingEnemy.Attack = enemyResponse.Attack;
                }
                else
                {
                    // Add new enemy
                    session.Enemies.Add(new Enemy
                    {
                        Name = enemyResponse.Name,
                        HP = enemyResponse.HP,
                        MaxHP = enemyResponse.MaxHP,
                        Description = enemyResponse.Description,
                        IsAlive = enemyResponse.HP > 0,
                        Attack = enemyResponse.Attack > 0 ? enemyResponse.Attack : 3
                    });
                }
            }

            // Enter combat if not already in combat and there are alive enemies
            if (!session.IsInCombat && session.Enemies.Any(e => e.IsAlive))
            {
                session.IsInCombat = true;
                appliedEvents.Add("Combat started!");
            }
        }

        // Update quests
        foreach (var questUpdate in aiResponse.QuestUpdates)
        {
            var quest = session.Quests.FirstOrDefault(q => q.Title == questUpdate.QuestTitle);
            if (quest != null && Enum.TryParse<QuestStatus>(questUpdate.Status, out var status))
            {
                quest.Status = status;
                appliedEvents.Add($"Quest '{quest.Title}' is now {status}");
            }
        }

        // Process skill checks and generate outcomes
        var diceRolls = new List<DiceRollResult>();
        var skillCheckNarratives = new List<string>();
        
        foreach (var skillCheck in aiResponse.SkillChecks)
        {
            var rollResult = ProcessSkillCheck(session.PlayerCharacter, skillCheck);
            diceRolls.Add(rollResult);

            // Add skill check result first
            var successText = rollResult.Success ? "succeeded" : "failed";
            appliedEvents.Add($"Skill check ({skillCheck.Attribute}): {successText}!");

            // Generate narrative outcome based on roll result
            var outcome = await GenerateSkillCheckOutcomeAsync(session, rollResult, request.PlayerAction);
            skillCheckNarratives.Add(outcome.Narrative);

            // Then apply outcome events (e.g., damage from falling)
            foreach (var outcomeEvent in outcome.Events)
            {
                _eventHandler.ApplyEvent(session, outcomeEvent, appliedEvents);
            }
        }

        // Check if player died from AI events or skill checks
        if (session.PlayerCharacter.HP == 0)
        {
            session.IsGameOver = true;
            session.IsInCombat = false;
        }

        // Increment turn number
        session.TurnNumber++;

        // Save memory entry
        var memoryEntry = new MemoryEntry
        {
            Content = $"Turn {session.TurnNumber}: Player action: {request.PlayerAction}. {aiResponse.Narrative}",
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

        // Combine main narrative with skill check outcomes
        var fullNarrative = aiResponse.Narrative;
        if (skillCheckNarratives.Any())
        {
            fullNarrative += "\n\n" + string.Join("\n\n", skillCheckNarratives);
        }

        return new TurnResponse
        {
            Narrative = fullNarrative,
            PlayerHP = session.PlayerCharacter.HP,
            MaxPlayerHP = session.PlayerCharacter.MaxHP,
            PlayerLevel = session.PlayerCharacter.Level,
            PlayerExperience = session.PlayerCharacter.Experience,
            PlayerExperienceToNextLevel = PlayerCharacter.XpToNextLevel(session.PlayerCharacter.Level),
            LeveledUp = session.PlayerCharacter.Level > levelBefore,
            CurrentLocation = session.CurrentLocation,
            LocationType = session.CurrentLocationType.ToString(),
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
            DiceRolls = diceRolls,
            SuggestedActions = aiResponse.SuggestedActions,
            IsInCombat = session.IsInCombat,
            IsGameOver = session.IsGameOver,
            Enemies = session.Enemies.Select(e => new EnemyDto
            {
                Name = e.Name,
                HP = e.HP,
                MaxHP = e.MaxHP,
                IsAlive = e.IsAlive,
                Description = e.Description,
                Attack = e.Attack
            }).ToList()
        };
    }

    private DiceRollResult ProcessSkillCheck(PlayerCharacter player, SkillCheck skillCheck)
    {
        // Get attribute value
        var attributeValue = skillCheck.Attribute.ToLower() switch
        {
            "strength" => player.Strength,
            "dexterity" => player.Dexterity,
            "constitution" => player.Constitution,
            "intelligence" => player.Intelligence,
            "wisdom" => player.Wisdom,
            "charisma" => player.Charisma,
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

    private async Task<(string Narrative, List<GameEvent> Events)> GenerateSkillCheckOutcomeAsync(
        GameSession session,
        DiceRollResult rollResult,
        string playerAction)
    {
        var languageInstruction = PromptTemplates.GetLanguageInstruction(session.Language);
        var systemPrompt = string.Format(PromptTemplates.SkillCheckOutcomeSystemPrompt, languageInstruction);
        
        var modifier = (rollResult.AttributeValue - 10) / 2;
        var userPrompt = PromptTemplates.BuildSkillCheckOutcomeUserPrompt(
            playerAction,
            rollResult.Attribute,
            rollResult.Difficulty,
            rollResult.DiceRoll,
            modifier,
            rollResult.Total,
            rollResult.Success);

        try
        {
            var response = await _aiService.GenerateResponseAsync(systemPrompt, userPrompt, useJsonFormat: true);
            
            // Record token usage
            RecordTokenUsage(session, response, "SkillCheck");
            
            return (response.Narrative, response.Events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate skill check outcome");
            var fallbackNarrative = rollResult.Success
                ? $"You manage to {rollResult.Purpose.ToLower()} successfully!"
                : $"You fail to {rollResult.Purpose.ToLower()}.";
            return (fallbackNarrative, new List<GameEvent>());
        }
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

        var languageInstruction = PromptTemplates.GetLanguageInstruction(session.Language);
        var systemPrompt = string.Format(PromptTemplates.SummarySystemPrompt, languageInstruction);
        var summaryPrompt = "Summarize the following events into a brief paragraph:\n\n" +
            string.Join("\n", recentEvents.Select(e => e.Content));

        try
        {
            var aiResponse = await _aiService.GenerateResponseAsync(systemPrompt, summaryPrompt, useJsonFormat: false);

            // Record token usage
            RecordTokenUsage(session, aiResponse, "Summary");

            var summary = new MemoryEntry
            {
                Content = aiResponse.Narrative,
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

    public async Task<TokenUsageStats> GetTokenUsageStatsAsync(Guid gameSessionId)
    {
        var session = await _repository.GetByIdAsync(gameSessionId);
        
        if (session == null)
        {
            throw new InvalidOperationException($"Game session {gameSessionId} not found");
        }

        var tokenHistory = session.TokenUsageHistory;

        // Calculate totals
        var totalTokens = tokenHistory.Sum(t => t.TotalTokens);
        var totalInputTokens = tokenHistory.Sum(t => t.InputTokens);
        var totalOutputTokens = tokenHistory.Sum(t => t.OutputTokens);

        // Group by call type
        var byType = tokenHistory
            .GroupBy(t => t.CallType)
            .Select(g => new TokenUsageByType
            {
                CallType = g.Key,
                Count = g.Count(),
                TotalTokens = g.Sum(t => t.TotalTokens),
                InputTokens = g.Sum(t => t.InputTokens),
                OutputTokens = g.Sum(t => t.OutputTokens)
            })
            .ToList();

        // Group by turn
        var byTurn = tokenHistory
            .GroupBy(t => t.TurnNumber)
            .Select(g => new TokenUsageByTurn
            {
                TurnNumber = g.Key,
                TotalTokens = g.Sum(t => t.TotalTokens),
                InputTokens = g.Sum(t => t.InputTokens),
                OutputTokens = g.Sum(t => t.OutputTokens),
                CallTypes = g.Select(t => t.CallType).ToList()
            })
            .OrderBy(t => t.TurnNumber)
            .ToList();

        // Estimate cost (rough calculation, assumes GPT-4o-mini pricing)
        // Input: $0.15 / 1M tokens, Output: $0.60 / 1M tokens
        var estimatedCost = (totalInputTokens * 0.15m + totalOutputTokens * 0.60m) / 1_000_000m;

        return new TokenUsageStats
        {
            GameSessionId = gameSessionId,
            TotalTokens = totalTokens,
            TotalInputTokens = totalInputTokens,
            TotalOutputTokens = totalOutputTokens,
            ByType = byType,
            ByTurn = byTurn,
            EstimatedCost = estimatedCost
        };
    }
}
