using InfiniteTavern.Application.Models;
using InfiniteTavern.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InfiniteTavern.Application.Services;

public interface IGameEventHandlerService
{
    void ApplyEvent(GameSession session, GameEvent gameEvent, List<string> appliedEvents);
}

public class GameEventHandlerService : IGameEventHandlerService
{
    private readonly ILogger<GameEventHandlerService> _logger;
    private readonly IDiceService _diceService;
    private readonly Dictionary<string, Func<GameSession, GameEvent, IEnumerable<string>>> _eventHandlers;

    public GameEventHandlerService(ILogger<GameEventHandlerService> logger, IDiceService diceService)
    {
        _logger = logger;
        _diceService = diceService;
        _eventHandlers = new Dictionary<string, Func<GameSession, GameEvent, IEnumerable<string>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "damage", HandleDamage },
            { "heal", HandleHeal },
            { "item_found", HandleItemFound },
            { "item_lost", HandleItemLost },
            { "gold_found", HandleGoldFound },
            { "gold_spent", HandleGoldSpent },
            { "xp_gained", HandleXpGained }
        };
    }

    public void ApplyEvent(GameSession session, GameEvent gameEvent, List<string> appliedEvents)
    {
        if (_eventHandlers.TryGetValue(gameEvent.Type, out var handler))
        {
            var messages = handler(session, gameEvent);
            appliedEvents.AddRange(messages);
        }
        else
        {
            _logger.LogWarning("Unknown event type: {EventType}", gameEvent.Type);
        }
    }

    private IEnumerable<string> HandleDamage(GameSession session, GameEvent gameEvent)
    {
        if (gameEvent.Target.Equals("player", StringComparison.OrdinalIgnoreCase) && session.PlayerCharacter != null)
        {
            // --- Guard: dead enemies cannot deal damage ---
            if (!string.IsNullOrEmpty(gameEvent.Attacker))
            {
                var namedAttacker = session.Enemies.FirstOrDefault(e =>
                    e.Name.Equals(gameEvent.Attacker, StringComparison.OrdinalIgnoreCase));
                if (namedAttacker != null && !namedAttacker.IsAlive)
                    yield break; // attacker is already dead, discard this event
            }

            // --- Dodge / block check ---
            var attacker = (!string.IsNullOrEmpty(gameEvent.Attacker)
                ? session.Enemies.FirstOrDefault(e => e.IsAlive &&
                    e.Name.Equals(gameEvent.Attacker, StringComparison.OrdinalIgnoreCase))
                : null)
                ?? session.Enemies.FirstOrDefault(e => e.IsAlive &&
                    !string.IsNullOrEmpty(e.Name) &&
                    gameEvent.Reason.Contains(e.Name, StringComparison.OrdinalIgnoreCase));

            if (attacker != null)
            {
                // D&D-style hit check: d20 + attacker.Attack >= 10 + player.Defense
                var roll = _diceService.Roll("1d20");
                var hitThreshold = 10 + session.PlayerCharacter.Defense - attacker.Attack;
                var hit = roll >= hitThreshold;

                if (!hit)
                {
                    var action = roll + attacker.Attack >= 10 + session.PlayerCharacter.Defense - 3
                        ? "blocked" : "dodged";
                    yield return $"{session.PlayerCharacter.Name} {action} the attack from {attacker.Name}! (roll {roll}, needed {Math.Max(1, hitThreshold)})";
                    yield break;
                }
            }

            session.PlayerCharacter.HP = Math.Max(0, session.PlayerCharacter.HP - gameEvent.Amount);
            yield return $"Player took {gameEvent.Amount} damage: {gameEvent.Reason}";

            if (session.PlayerCharacter.HP == 0)
            {
                yield return "Player has fallen!";
            }
        }
        else
        {
            // Check if target is an enemy
            var enemy = session.Enemies.FirstOrDefault(e =>
                e.Name.Equals(gameEvent.Target, StringComparison.OrdinalIgnoreCase) && e.IsAlive);
            
            if (enemy != null)
            {
                enemy.HP = Math.Max(0, enemy.HP - gameEvent.Amount);
                yield return $"{enemy.Name} took {gameEvent.Amount} damage: {gameEvent.Reason}";

                if (enemy.HP == 0)
                {
                    enemy.IsAlive = false;
                    yield return $"{enemy.Name} was defeated!";
                    
                    // Check if combat should end
                    if (session.Enemies.All(e => !e.IsAlive))
                    {
                        session.IsInCombat = false;
                        yield return "Victory! All enemies defeated!";

                        // Auto-award XP — do not rely on AI sending xp_gained
                        if (session.PlayerCharacter != null)
                        {
                            var xpReward = session.Enemies.Sum(e => 10 + e.Attack * 5 + (e.MaxHP / 3));
                            xpReward = Math.Max(xpReward, 20); // minimum 20 XP
                            session.PlayerCharacter.Experience += xpReward;
                            session.CombatXpAwarded = true;
                            yield return $"Gained {xpReward} XP for defeating all enemies!";

                            // Level-up check
                            while (session.PlayerCharacter.Experience >= PlayerCharacter.XpToNextLevel(session.PlayerCharacter.Level))
                            {
                                session.PlayerCharacter.Experience -= PlayerCharacter.XpToNextLevel(session.PlayerCharacter.Level);
                                session.PlayerCharacter.Level++;
                                var hpGain = session.PlayerCharacter.Class switch
                                {
                                    "Warrior" => 6, "Cleric" => 5, "Ranger" => 5, "Rogue" => 4, "Wizard" => 3, _ => 4
                                };
                                session.PlayerCharacter.MaxHP += hpGain;
                                session.PlayerCharacter.HP = Math.Min(session.PlayerCharacter.HP + hpGain, session.PlayerCharacter.MaxHP);
                                switch (session.PlayerCharacter.Class)
                                {
                                    case "Warrior": session.PlayerCharacter.Strength++;     break;
                                    case "Wizard":  session.PlayerCharacter.Intelligence++; break;
                                    case "Rogue":   session.PlayerCharacter.Dexterity++;    break;
                                    case "Cleric":  session.PlayerCharacter.Wisdom++;       break;
                                    case "Ranger":  session.PlayerCharacter.Dexterity++;    break;
                                    default:        session.PlayerCharacter.Constitution++; break;
                                }
                                yield return $"LEVEL UP! You are now level {session.PlayerCharacter.Level}! MaxHP +{hpGain}, primary stat +1.";
                            }
                        }
                    }
                }
            }
            else
            {
                // Fallback to NPC damage
                var npc = session.Npcs.FirstOrDefault(n =>
                    n.Name.Equals(gameEvent.Target, StringComparison.OrdinalIgnoreCase) && n.IsAlive);
                if (npc != null)
                {
                    npc.IsAlive = false;
                    yield return $"{npc.Name} was defeated: {gameEvent.Reason}";
                }
            }
        }
    }

    private IEnumerable<string> HandleHeal(GameSession session, GameEvent gameEvent)
    {
        if (gameEvent.Target.Equals("player", StringComparison.OrdinalIgnoreCase) && session.PlayerCharacter != null)
        {
            session.PlayerCharacter.HP = Math.Min(
                session.PlayerCharacter.MaxHP,
                session.PlayerCharacter.HP + gameEvent.Amount);
            yield return $"Player healed {gameEvent.Amount} HP: {gameEvent.Reason}";
        }
    }

    /// <summary>Strip AI prefixes like "Found: ", "Знайдено ", "Item: ", etc. from item names.</summary>
    private static string CleanItemName(string raw)
    {
        var cleaned = System.Text.RegularExpressions.Regex.Replace(
            raw.Trim(),
            @"^(Found|Знайдено|Item|Предмет|Отримано|Received|Pickup|Picked up)\s*:?\s*",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? raw.Trim() : cleaned;
    }

    private IEnumerable<string> HandleItemFound(GameSession session, GameEvent gameEvent)
    {
        if (session.PlayerCharacter == null) yield break;

        var itemName = CleanItemName(gameEvent.Reason ?? string.Empty);
        var itemType = !string.IsNullOrWhiteSpace(gameEvent.ItemType) ? gameEvent.ItemType : "Miscellaneous";

        var existingItem = session.PlayerCharacter.Inventory
            .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (existingItem != null)
        {
            // If AI marked item as unique, skip duplicate (hallucination guard)
            if (gameEvent.IsUnique)
                yield break;

            existingItem.Quantity += gameEvent.Amount > 0 ? gameEvent.Amount : 1;
        }
        else
        {
            var newItem = new Item
            {
                Name = itemName,
                Type = itemType,
                Description = "Found during adventure",
                Quantity = gameEvent.Amount > 0 ? gameEvent.Amount : 1,
                Bonuses = gameEvent.Bonuses ?? new Dictionary<string, int>()
            };
            session.PlayerCharacter.Inventory.Add(newItem);
        }
        yield return $"Found: {itemName} x{(gameEvent.Amount > 0 ? gameEvent.Amount : 1)}";
    }

    private IEnumerable<string> HandleItemLost(GameSession session, GameEvent gameEvent)
    {
        if (session.PlayerCharacter == null) yield break;

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

            yield return $"Lost: {gameEvent.Reason} x{amountToRemove}";
        }
    }

    private IEnumerable<string> HandleGoldFound(GameSession session, GameEvent gameEvent)
    {
        if (session.PlayerCharacter != null)
        {
            session.PlayerCharacter.Gold += gameEvent.Amount;
            yield return $"Found {gameEvent.Amount} gold: {gameEvent.Reason}";
        }
    }

    private IEnumerable<string> HandleGoldSpent(GameSession session, GameEvent gameEvent)
    {
        if (session.PlayerCharacter != null)
        {
            session.PlayerCharacter.Gold = Math.Max(0, session.PlayerCharacter.Gold - gameEvent.Amount);
            yield return $"Spent {gameEvent.Amount} gold: {gameEvent.Reason}";
        }
    }

    private IEnumerable<string> HandleXpGained(GameSession session, GameEvent gameEvent)
    {
        var player = session.PlayerCharacter;
        if (player == null || gameEvent.Amount <= 0) yield break;

        // Skip if combat XP was already auto-awarded this turn to avoid duplication
        if (session.CombatXpAwarded)
        {
            session.CombatXpAwarded = false; // reset for next combat
            yield break;
        }

        player.Experience += gameEvent.Amount;
        yield return $"Gained {gameEvent.Amount} XP: {gameEvent.Reason}";

        // Level-up loop (handles multiple levels at once)
        while (player.Experience >= PlayerCharacter.XpToNextLevel(player.Level))
        {
            player.Experience -= PlayerCharacter.XpToNextLevel(player.Level);
            player.Level++;

            var hpGain = player.Class switch
            {
                "Warrior" => 6,
                "Cleric"  => 5,
                "Ranger"  => 5,
                "Rogue"   => 4,
                "Wizard"  => 3,
                _          => 4
            };
            player.MaxHP += hpGain;
            player.HP = Math.Min(player.HP + hpGain, player.MaxHP);

            // Primary stat bonus
            switch (player.Class)
            {
                case "Warrior": player.Strength++;     break;
                case "Wizard":  player.Intelligence++; break;
                case "Rogue":   player.Dexterity++;    break;
                case "Cleric":  player.Wisdom++;       break;
                case "Ranger":  player.Dexterity++;    break;
                default:        player.Constitution++; break;
            }

            yield return $"LEVEL UP! You are now level {player.Level}! MaxHP +{hpGain}, primary stat +1.";
        }
    }
}
