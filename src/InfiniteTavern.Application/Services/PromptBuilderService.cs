using System.Text;
using InfiniteTavern.Domain.Entities;

namespace InfiniteTavern.Application.Services;

public interface IPromptBuilderService
{
    string BuildSystemPrompt(string language = "English");
    string BuildSystemPromptForCombat(string language = "English");
    string BuildUserPrompt(
        GameSession session,
        PlayerCharacter player,
        List<Npc> nearbyNpcs,
        List<Quest> activeQuests,
        List<MemoryEntry> recentMemories,
        List<MemoryEntry> importantMemories,
        string playerAction);
}

public class PromptBuilderService : IPromptBuilderService
{
    public string BuildSystemPrompt(string language = "English")
    {
        var languageInstruction = PromptTemplates.GetLanguageInstruction(language);
        return string.Format(PromptTemplates.DungeonMasterSystemPrompt, languageInstruction);
    }

    public string BuildSystemPromptForCombat(string language = "English")
    {
        var languageInstruction = PromptTemplates.GetLanguageInstruction(language);
        return string.Format(PromptTemplates.CombatSystemPrompt, languageInstruction);
    }

    public string BuildUserPrompt(
        GameSession session,
        PlayerCharacter player,
        List<Npc> nearbyNpcs,
        List<Quest> activeQuests,
        List<MemoryEntry> recentMemories,
        List<MemoryEntry> importantMemories,
        string playerAction)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== GAME STATE ===");
        sb.AppendLine($"Turn: {session.TurnNumber}");
        sb.AppendLine($"Location: {session.CurrentLocation}");
        sb.AppendLine($"Time: {session.WorldTime}");
        sb.AppendLine();

        sb.AppendLine("=== PLAYER CHARACTER ===");
        sb.AppendLine($"Name: {player.Name}");
        sb.AppendLine($"Race: {player.Race}");
        sb.AppendLine($"Class: {player.Class}");
        sb.AppendLine($"Level: {player.Level}");
        sb.AppendLine($"HP: {player.HP}/{player.MaxHP}");
        sb.AppendLine($"Strength: {player.Strength}");
        sb.AppendLine($"Dexterity: {player.Dexterity}");
        sb.AppendLine($"Constitution: {player.Constitution}");
        sb.AppendLine($"Intelligence: {player.Intelligence}");
        sb.AppendLine($"Wisdom: {player.Wisdom}");
        sb.AppendLine($"Charisma: {player.Charisma}");
        sb.AppendLine($"Defense: {player.Defense} (dodge/block rating; higher = harder to hit)");
        sb.AppendLine($"Experience: {player.Experience}/{PlayerCharacter.XpToNextLevel(player.Level)} (next level)");
        sb.AppendLine($"Gold: {player.Gold}");

        if (player.Inventory.Any())
        {
            sb.AppendLine("Inventory:");
            foreach (var item in player.Inventory)
            {
                sb.AppendLine($"  - {item.Name} x{item.Quantity}{(item.IsEquipped ? " (equipped)" : "")}");
            }
        }
        else
        {
            sb.AppendLine("Inventory: Empty");
        }
        sb.AppendLine();

        if (session.IsInCombat && session.Enemies.Any(e => e.IsAlive))
        {
            sb.AppendLine("=== COMBAT - ENEMIES ===");
            foreach (var enemy in session.Enemies.Where(e => e.IsAlive))
            {
                sb.AppendLine($"- {enemy.Name}: {enemy.HP}/{enemy.MaxHP} HP");
                if (!string.IsNullOrEmpty(enemy.Description))
                {
                    sb.AppendLine($"  {enemy.Description}");
                }
            }
            sb.AppendLine();
        }

        if (nearbyNpcs.Any())
        {
            sb.AppendLine("=== NPCs IN SCENE ===");
            foreach (var npc in nearbyNpcs)
            {
                sb.AppendLine($"- {npc.Name}: {npc.PersonalityTraits}");
                sb.AppendLine($"  Relationship: {npc.RelationshipToPlayer}");
                sb.AppendLine($"  Status: {(npc.IsAlive ? "Alive" : "Dead")}");
            }
            sb.AppendLine();
        }

        if (activeQuests.Any())
        {
            sb.AppendLine("=== ACTIVE QUESTS ===");
            foreach (var quest in activeQuests)
            {
                sb.AppendLine($"- {quest.Title}");
                sb.AppendLine($"  {quest.Description}");
            }
            sb.AppendLine();
        }

        if (importantMemories.Any())
        {
            sb.AppendLine("=== IMPORTANT MEMORIES ===");
            foreach (var memory in importantMemories)
            {
                sb.AppendLine($"- [{memory.Type}] {memory.Content}");
            }
            sb.AppendLine();
        }

        if (recentMemories.Any())
        {
            sb.AppendLine("=== RECENT EVENTS ===");
            foreach (var memory in recentMemories)
            {
                sb.AppendLine($"- {memory.Content}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== PLAYER ACTION ===");
        sb.AppendLine(playerAction);
        sb.AppendLine();

        sb.AppendLine("Generate narrative and events in strict JSON format.");

        return sb.ToString();
    }
}
