# Combat System Documentation

## Overview
The combat system is a turn-based combat phase that activates when enemies are present. During combat, the AI uses specialized combat prompts and enemies automatically counterattack after each player action.

## How Combat Works

### Starting Combat
The AI can start combat by including enemies in the response with a special event or narrative. The system will automatically set `IsInCombat = true` and populate the `Enemies` list.

Example AI response structure:
```json
{
  "narrative": "A goblin jumps out from the shadows!",
  "events": [],
  "new_npcs": [],
  "enemies": [
    {
      "name": "Goblin Scout",
      "hp": 15,
      "maxHP": 15,
      "isAlive": true,
      "description": "A small but vicious goblin"
    }
  ]
}
```

### During Combat

1. **Player's Turn**: Player takes an action (attack, use item, flee, etc.)
2. **AI Response**: AI processes the action and generates damage events
3. **Enemy Counterattacks**: All alive enemies automatically attack the player (1d6+2 damage each)
4. **HP Updates**: Damage is applied to targets
5. **Victory Check**: If all enemies HP = 0, combat ends automatically

### Combat Prompts

The AI uses [PromptTemplates.CombatSystemPrompt](src/InfiniteTavern.Application/Services/PromptTemplates.cs#L163) during combat, which includes:

- Turn-based combat rules
- Enemy counterattack instructions
- Damage event format: `{ "type": "damage", "target": "Goblin Scout", "amount": 5 }`
- Flee attempt mechanics (Dexterity check)
- Combat end conditions

### Ending Combat

Combat ends automatically when:
- All enemies are defeated (HP <= 0)
- Player dies (HP = 0)  
- Player successfully flees (Dexterity check)

### UI Components

**New Components:**
- [EnemyList.tsx](frontend/src/components/EnemyList.tsx) - Displays enemies with HP bars
- Integrated into [GameView.tsx](frontend/src/components/GameView.tsx) sidebar

**Enemy Display:**
- Name and current/max HP
- Visual HP bar (red gradient)
- Description

## Technical Implementation

### Backend Changes

**Entity Model:**
- [Enemy.cs](src/InfiniteTavern.Domain/Entities/Enemy.cs) - Enemy entity with HP tracking
- [GameSession.cs](src/InfiniteTavern.Domain/Entities/GameSession.cs) - Added `IsInCombat` and `Enemies` list

**Services:**
- [PromptBuilderService.cs](src/InfiniteTavern.Application/Services/PromptBuilderService.cs) - New `BuildSystemPromptForCombat()` method
- [GameEventHandlerService.cs](src/InfiniteTavern.Application/Services/GameEventHandlerService.cs) - Extended `HandleDamage()` for enemy targeting
- [GameService.cs](src/InfiniteTavern.Application/Services/GameService.cs) - Combat phase detection and enemy counterattacks

**API Models:**
- [GameModels.cs](src/InfiniteTavern.Application/Models/GameModels.cs) - Added `EnemyDto`, updated `TurnResponse`

### Frontend Changes

**Type Definitions:**
- [game.ts](frontend/src/types/game.ts) - Added `Enemy` interface, updated `TurnResponse` and `GameState`

**State Management:**
- [App.tsx](frontend/src/App.tsx) - Track `isInCombat` and `enemies` in game state

**Localization:**
- [en.ts](frontend/src/locales/en.ts) - Added `enemies: 'Enemies'`
- [uk.ts](frontend/src/locales/uk.ts) - Added `enemies: 'Вороги'`

## Example Combat Flow

1. Player: "I attack the goblin with my sword"
2. AI generates: `{ "type": "damage", "target": "Goblin Scout", "amount": 8 }`
3. Goblin HP: 15 → 7
4. Automatic counterattack: Goblin deals 5 damage (1d6+2)
5. Player HP: 20 → 15
6. Combat continues until victory/defeat

## Testing the Combat System

To test the combat system:

1. Start a new game
2. Ask the AI to start a combat encounter: "A goblin attacks me!"
3. The combat UI should appear showing the enemy with HP bar
4. Attack the enemy and observe:
   - Enemy HP decreases
   - Enemy counterattacks automatically
   - Combat ends when enemy HP reaches 0

## Future Enhancements

Potential improvements:
- Multiple enemy types with different stats
- Special abilities for enemies
- Loot drops on enemy defeat
- Experience/leveling system
- Status effects (poison, stun, etc.)
- Targeted attacks (select specific enemy)
