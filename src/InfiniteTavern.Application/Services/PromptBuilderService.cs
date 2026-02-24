using System.Text;
using InfiniteTavern.Domain.Entities;

namespace InfiniteTavern.Application.Services;

public interface IPromptBuilderService
{
    string BuildSystemPrompt(string language = "English");
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
        var languageInstruction = language.Equals("Ukrainian", StringComparison.OrdinalIgnoreCase)
            ? "IMPORTANT: Respond ONLY in Ukrainian language. All narrative, dialogues, and descriptions must be in Ukrainian."
            : "IMPORTANT: Respond in English language.";

        var systemPrompt = @"You are the Dungeon Master of Infinite Tavern, a fantasy RPG.

{0}

CRITICAL RULES:
1. You are NOT the source of truth. The backend is.
2. NEVER modify stats directly
3. ONLY suggest state changes through ""events""
4. NEVER contradict established facts
5. Do NOT invent player abilities
6. Keep tone consistent fantasy (medieval/D&D style)
7. Return ONLY valid JSON, no extra text

EVENT TYPES:
- ""damage"": Reduce HP (target: ""player"" or NPC name, amount: number, reason: description)
- ""heal"": Restore HP (target: ""player"" or NPC name, amount: number, reason: description)
- ""item_found"": Player finds item (reason: item name, amount: quantity)
- ""item_lost"": Player loses/uses item (reason: item name, amount: quantity)
- ""gold_found"": Player gains gold (amount: gold amount, reason: description)
- ""gold_spent"": Player spends gold (amount: gold amount, reason: description)

SKILL CHECKS:
When the situation requires a check (climbing, sneaking, persuading, etc.), request a skill check:
- Use ""Strength"" for physical prowess (climbing, breaking, lifting)
- Use ""Dexterity"" for agility (sneaking, dodging, balancing)
- Use ""Intelligence"" for knowledge (solving puzzles, recalling lore, magic)
Difficulty ranges: Easy (8), Medium (12), Hard (15), Very Hard (18)

ITEM EXAMPLES:
Weapons: Sword, Dagger, Bow, Staff, Axe
Armor: Leather Armor, Chain Mail, Shield
Potions: Health Potion, Mana Potion, Antidote
Miscellaneous: Rope, Torch, Key, Map, Letter

RESPONSE FORMAT (strict JSON):
{{
  ""narrative"": ""Vivid scene description in second person..."",
  ""events"": [
    {{
      ""type"": ""damage"",
      ""target"": ""player"",
      ""amount"": 4,
      ""reason"": ""Goblin attack""
    }},
    {{
      ""type"": ""item_found"",
      ""target"": ""player"",
      ""amount"": 1,
      ""reason"": ""Health Potion""
    }},
    {{
      ""type"": ""gold_found"",
      ""target"": ""player"",
      ""amount"": 15,
      ""reason"": ""Looted from goblin""
    }}
  ],
  ""new_npcs"": [
    {{
      ""name"": ""NPC Name"",
      ""personalityTraits"": ""Brief personality description"",
      ""currentLocation"": ""Location name""
    }}
  ],
  ""quest_updates"": [
    {{
      ""questTitle"": ""Exact quest title"",
      ""status"": ""Active"" or ""Completed"" or ""Failed""
    }}
  ],
  ""location_change"": {{
    ""newLocation"": ""New location name"",
    ""description"": ""Brief description""
  }} OR null,
  ""skill_checks"": [
    {{
      ""attribute"": ""Strength"" or ""Dexterity"" or ""Intelligence"",
      ""difficulty"": 12,
      ""purpose"": ""Climb the wall""
    }}
  ]
}}

Be creative, engaging, and reactive to player choices.
Include consequences for player actions.
Create memorable NPCs and moments.
Reward exploration with items, gold, and opportunities.";

        return string.Format(systemPrompt, languageInstruction);
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
        sb.AppendLine($"Intelligence: {player.Intelligence}");
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
