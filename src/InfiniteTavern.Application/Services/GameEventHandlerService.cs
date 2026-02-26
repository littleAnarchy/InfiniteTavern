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
            // --- Dodge / block check ---
            // Find the attacking enemy: prefer explicit Attacker field, then match by name in Reason
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

    private IEnumerable<string> HandleItemFound(GameSession session, GameEvent gameEvent)
    {
        if (session.PlayerCharacter == null) yield break;

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
        yield return $"Found: {gameEvent.Reason} x{(gameEvent.Amount > 0 ? gameEvent.Amount : 1)}";
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
