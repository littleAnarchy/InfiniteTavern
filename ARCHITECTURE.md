# System Architecture & Data Flow

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         CLIENT                               │
│                    (Any HTTP Client)                         │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTP Requests
                       │ (JSON)
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                   API LAYER                                  │
│  ┌────────────────────────────────────────────────────┐     │
│  │          GameController                            │     │
│  │  - POST /new-game                                  │     │
│  │  - POST /turn                                      │     │
│  │  - GET /health                                     │     │
│  └────────────────────────────────────────────────────┘     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              APPLICATION LAYER                               │
│                                                              │
│  ┌──────────────────────────────────────────────────┐       │
│  │           GameService (Core Logic)               │       │
│  └──────────────────────────────────────────────────┘       │
│           │          │           │           │              │
│           ▼          ▼           ▼           ▼              │
│  ┌──────────┐ ┌───────────┐ ┌─────────┐ ┌────────┐        │
│  │  Prompt  │ │    AI     │ │  Dice   │ │MongoDB │        │
│  │ Builder  │ │  Service  │ │ Service │ │ Repo   │        │
│  └──────────┘ └───────────┘ └─────────┘ └────────┘        │
│                     │                         │             │
└─────────────────────┼─────────────────────────┼─────────────┘
                      │                         │
                      ▼                         ▼
         ┌────────────────────┐    ┌──────────────────────┐
         │   AI Provider:     │    │   MongoDB            │
         │   - OpenAI         │    │   (Game State)       │
         │   - Claude         │    │   Documents          │
         └────────────────────┘    └──────────────────────┘
```

## Turn Processing Flow

```
CLIENT REQUEST
    │
    │ POST /turn { gameSessionId, playerAction }
    │
    ▼
┌─────────────────────────────────────────────────────────┐
│ 1. LOAD GAME STATE                                      │
│    - GameSession                                        │
│    - PlayerCharacter                                    │
│    - NPCs in current location                           │
│    - Active Quests                                      │
│    - Recent Memories (last 5 turns)                     │
│    - Important Memories (top 3 by importance)           │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 2. BUILD PROMPT                                          │
│    PromptBuilderService                                  │
│    ┌──────────────────────────────────────────────┐     │
│    │ System Prompt:                               │     │
│    │  - Role definition (Dungeon Master)          │     │
│    │  - Rules (no direct stat changes)            │     │
│    │  - Event types                               │     │
│    │  - JSON format requirements                  │     │
│    └──────────────────────────────────────────────┘     │
│    ┌──────────────────────────────────────────────┐     │
│    │ User Prompt:                                 │     │
│    │  - Current game state                        │     │
│    │  - Player stats                              │     │
│    │  - NPCs in scene                             │     │
│    │  - Active quests                             │     │
│    │  - Recent events                             │     │
│    │  - Important memories                        │     │
│    │  - Player action                             │     │
│    └──────────────────────────────────────────────┘     │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 3. CALL CLAUDE API                                       │
│    ClaudeService                                         │
│    - HTTP POST to api.anthropic.com                      │
│    - Model: claude-3-5-sonnet-20241022                   │
│    - Max tokens: 2048                                    │
│    - Parse JSON response                                 │
│    - Extract from markdown if needed                     │
│    - Handle errors gracefully                            │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 4. PARSE & VALIDATE RESPONSE                             │
│    {                                                     │
│      "narrative": "...",                                 │
│      "events": [...],                                    │
│      "new_npcs": [...],                                  │
│      "quest_updates": [...],                             │
│      "location_change": {...}                            │
│    }                                                     │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 5. APPLY EVENTS                                          │
│    Backend is SOURCE OF TRUTH                            │
│                                                          │
│    For each event:                                       │
│    ┌─────────────────────────────────────┐              │
│    │ damage → Reduce HP                  │              │
│    │ heal → Restore HP                   │              │
│    │ item_found → Log event              │              │
│    │ item_lost → Log event               │              │
│    └─────────────────────────────────────┘              │
│                                                          │
│    Validate all changes:                                 │
│    - HP cannot go below 0                                │
│    - HP cannot exceed MaxHP                              │
│    - NPCs track alive/dead status                        │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 6. UPDATE GAME STATE                                     │
│    - Apply location changes                              │
│    - Create new NPCs                                     │
│    - Update quest status                                 │
│    - Increment turn number                               │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 7. SAVE MEMORY                                           │
│    Create MemoryEntry:                                   │
│    - Type: Event                                         │
│    - Content: Turn summary                               │
│    - ImportanceScore: 5 (default)                        │
│    - CreatedAt: Now                                      │
│                                                          │
│    Every 10 turns:                                       │
│    - Generate summary via Claude                         │
│    - Store as high-importance memory (10)                │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 8. PERSIST TO DATABASE                                   │
│    SaveChangesAsync()                                    │
│    - All entities updated atomically                     │
│    - Transaction ensures consistency                     │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│ 9. RETURN RESPONSE                                       │
│    {                                                     │
│      "narrative": "...",                                 │
│      "playerHP": 16,                                     │
│      "maxPlayerHP": 20,                                  │
│      "currentLocation": "...",                           │
│      "appliedEvents": [...]                              │
│    }                                                     │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
                   CLIENT RECEIVES
                    RESPONSE JSON
```

## Memory System Flow

```
┌─────────────────────────────────────────────────────────┐
│               MEMORY ENTRY TYPES                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Event (ImportanceScore: 5)                              │
│  ├─ Created every turn                                   │
│  ├─ Contains player action + narrative summary           │
│  └─ Last 5 used for recent context                       │
│                                                          │
│  Summary (ImportanceScore: 10)                           │
│  ├─ Generated every 10 turns                             │
│  ├─ Summarizes previous 10 events                        │
│  └─ Top 3 used for important context                     │
│                                                          │
│  NPC (ImportanceScore: varies)                           │
│  └─ Key NPC interactions                                 │
│                                                          │
│  Quest (ImportanceScore: varies)                         │
│  └─ Quest milestones                                     │
│                                                          │
└─────────────────────────────────────────────────────────┘

Retrieval Strategy
──────────────────
1. Recent Context (Temporal)
   SELECT TOP 5 WHERE Type = Event
   ORDER BY CreatedAt DESC

2. Important Context (Significance)
   SELECT TOP 3 WHERE Type != Event
   ORDER BY ImportanceScore DESC
```

## Database Schema (Relationships)

```
┌──────────────────────┐
│    GameSession       │
│──────────────────────│
│ PK Id                │
│    PlayerName        │
│    CurrentLocation   │
│    WorldTime         │
│    TurnNumber        │
│    CreatedAt         │
└──────────┬───────────┘
           │
           │ 1:1
           │
           ├─────────────────────────┐
           │                         │
           ▼                         │
┌──────────────────────┐             │
│  PlayerCharacter     │             │
│──────────────────────│             │
│ PK Id                │             │
│ FK GameSessionId ────┘             │
│    Name                            │
│    Race                            │
│    Class                           │
│    Level                           │
│    HP                              │
│    MaxHP                           │
│    Strength                        │
│    Dexterity                       │
│    Intelligence                    │
└──────────────────────┘             │
                                     │
           ┌─────────────────────────┤
           │ 1:N                     │
           ▼                         │
┌──────────────────────┐             │
│       Npc            │             │
│──────────────────────│             │
│ PK Id                │             │
│ FK GameSessionId ────┤             │
│    Name              │             │
│    PersonalityTraits │             │
│    Relationship...   │             │
│    CurrentLocation   │             │
│    IsAlive           │             │
└──────────────────────┘             │
                                     │
           ┌─────────────────────────┤
           │ 1:N                     │
           ▼                         │
┌──────────────────────┐             │
│      Quest           │             │
│──────────────────────│             │
│ PK Id                │             │
│ FK GameSessionId ────┤             │
│    Title             │             │
│    Description       │             │
│    Status            │             │
└──────────────────────┘             │
                                     │
           ┌─────────────────────────┘
           │ 1:N
           ▼
┌──────────────────────┐
│    MemoryEntry       │
│──────────────────────│
│ PK Id                │
│ FK GameSessionId     │
│    Content           │
│    Type              │
│    ImportanceScore   │
│    CreatedAt         │
│ IX (SessionId, Imp.) │
└──────────────────────┘
```

## Service Dependencies

```
┌─────────────────────────────────────────────────────────┐
│                    GameService                           │
│                 (Application Core)                       │
└───────┬───────────┬────────────┬────────────┬───────────┘
        │           │            │            │
        │           │            │            │
        ▼           ▼            ▼            ▼
┌──────────┐ ┌───────────┐ ┌─────────┐ ┌──────────┐
│ Prompt   │ │  Claude   │ │  Dice   │ │ DbContext│
│ Builder  │ │  Service  │ │ Service │ │          │
└──────────┘ └─────┬─────┘ └─────────┘ └──────────┘
                   │
                   │ HttpClient
                   │
                   ▼
            ┌──────────────┐
            │   Anthropic  │
            │   Claude API │
            └──────────────┘

Dependency Injection Configuration
──────────────────────────────────
Singleton  → DiceService (stateless, reusable)
Scoped     → GameService, PromptBuilder (per request)
Scoped     → DbContext (per request)
Transient  → ClaudeService (via HttpClient factory)
```

## Event Application Logic

```
┌─────────────────────────────────────────────────────────┐
│              EVENT: "damage"                             │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  IF target == "player":                                  │
│    player.HP = Max(0, player.HP - amount)                │
│    IF player.HP == 0:                                    │
│      Log "Player has fallen!"                            │
│                                                          │
│  ELSE:                                                   │
│    npc = Find NPC by name                                │
│    IF npc exists AND npc.IsAlive:                        │
│      npc.IsAlive = false                                 │
│      Log "{npc.Name} was defeated"                       │
│                                                          │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│              EVENT: "heal"                               │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  IF target == "player":                                  │
│    player.HP = Min(player.MaxHP, player.HP + amount)     │
│    Log "Player healed {amount} HP"                       │
│                                                          │
└─────────────────────────────────────────────────────────┘

Backend Always Validates:
─────────────────────────
✓ HP never negative
✓ HP never exceeds MaxHP
✓ NPCs properly tracked
✓ Events always logged
✓ State always persisted
```

## Claude Prompt Structure

```
┌──────────────────────────────────────────────────────────┐
│                   SYSTEM PROMPT                           │
│  (Defines DM role and strict rules)                      │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  You are the Dungeon Master of Infinite Tavern...        │
│                                                           │
│  CRITICAL RULES:                                          │
│  1. Backend is source of truth                           │
│  2. Never modify stats directly                          │
│  3. Only suggest events                                  │
│  4. Return ONLY JSON                                     │
│  5. Never contradict established facts                   │
│                                                           │
│  EVENT TYPES: damage, heal, item_found, item_lost        │
│                                                           │
│  RESPONSE FORMAT: {strict JSON schema}                   │
│                                                           │
└──────────────────────────────────────────────────────────┘
                          +
┌──────────────────────────────────────────────────────────┐
│                    USER PROMPT                            │
│  (Current game state and player action)                  │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  === GAME STATE ===                                       │
│  Turn: 5                                                  │
│  Location: The Infinite Tavern                            │
│  Time: Evening                                            │
│                                                           │
│  === PLAYER CHARACTER ===                                 │
│  Name: Thorin                                             │
│  Race: Dwarf, Class: Warrior, Level: 1                    │
│  HP: 16/20                                                │
│  STR: 14, DEX: 10, INT: 8                                 │
│                                                           │
│  === NPCs IN SCENE ===                                    │
│  - Garrick: Friendly tavern keeper, Welcoming             │
│                                                           │
│  === ACTIVE QUESTS ===                                    │
│  (none)                                                   │
│                                                           │
│  === IMPORTANT MEMORIES ===                               │
│  (top 3 summaries)                                        │
│                                                           │
│  === RECENT EVENTS ===                                    │
│  (last 5 turns)                                           │
│                                                           │
│  === PLAYER ACTION ===                                    │
│  I ask Garrick about local rumors                         │
│                                                           │
└──────────────────────────────────────────────────────────┘
                          ↓
                  Sent to Claude API
                          ↓
                   JSON Response
```

## API Request/Response Flow

```
CREATE NEW GAME
───────────────
POST /api/game/new-game
{
  "playerName": "Alice",
  "characterName": "Thorin",
  "race": "Dwarf",
  "class": "Warrior"
}
        ↓
    Generate:
    - Random stats (3d6 each)
    - Starting HP: 20
    - Initial NPC (Garrick)
    - First memory entry
        ↓
{
  "gameSessionId": "guid",
  "message": "Welcome...",
  "playerStats": {
    "name": "Thorin",
    "race": "Dwarf",
    "class": "Warrior",
    "level": 1,
    "hp": 20,
    "maxHP": 20,
    "strength": 14,
    "dexterity": 10,
    "intelligence": 8
  }
}

PROCESS TURN
────────────
POST /api/game/turn
{
  "gameSessionId": "guid",
  "playerAction": "I attack the goblin"
}
        ↓
    Execute turn flow
    (see Turn Processing Flow above)
        ↓
{
  "narrative": "You swing your axe...",
  "playerHP": 16,
  "maxPlayerHP": 20,
  "currentLocation": "Forest Path",
  "appliedEvents": [
    "Player took 4 damage: Goblin counterattack",
    "Goblin was defeated"
  ]
}
```

---

This architecture ensures:
- ✅ Clean separation of concerns
- ✅ Backend as authoritative source
- ✅ AI integration without control loss
- ✅ Scalable and maintainable design
- ✅ Clear data flow and state management
