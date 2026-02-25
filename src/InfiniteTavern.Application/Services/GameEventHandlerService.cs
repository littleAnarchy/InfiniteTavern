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
    private readonly Dictionary<string, Func<GameSession, GameEvent, IEnumerable<string>>> _eventHandlers;

    public GameEventHandlerService(ILogger<GameEventHandlerService> logger)
    {
        _logger = logger;
        _eventHandlers = new Dictionary<string, Func<GameSession, GameEvent, IEnumerable<string>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "damage", HandleDamage },
            { "heal", HandleHeal },
            { "item_found", HandleItemFound },
            { "item_lost", HandleItemLost },
            { "gold_found", HandleGoldFound },
            { "gold_spent", HandleGoldSpent }
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
}
