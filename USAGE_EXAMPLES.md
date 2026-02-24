# Infinite Tavern - Usage Examples

## Starting the Application

### 1. Start MongoDB
```powershell
docker-compose up -d
```

### 2. Update API Key
Edit `src/InfiniteTavern.API/appsettings.json`:
```json
{
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR-KEY-HERE"
  }
}
```

### 3. Run the API
```powershell
dotnet run --project src/InfiniteTavern.API
```

API URL: http://localhost:5000

**Note:** No migrations needed with MongoDB!

## Example Game Sessions

### Session 1: Classic Fantasy Adventure

#### 1. Create Character
```powershell
$response = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/new-game" `
  -ContentType "application/json" `
  -Body '{"playerName":"Alice","characterName":"Aria Swiftwind","race":"Elf","class":"Ranger"}'

$gameId = $response.gameSessionId
Write-Host "Game ID: $gameId"
Write-Host "HP: $($response.playerStats.hp)/$($response.playerStats.maxHP)"
```

#### 2. Greet the Tavern Keeper
```powershell
$turn1 = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$gameId`",`"playerAction`":`"I greet Garrick warmly and ask him how business has been`"}"

Write-Host "Narrative: $($turn1.narrative)"
Write-Host "HP: $($turn1.playerHP)/$($turn1.maxPlayerHP)"
```

#### 3. Ask About Rumors
```powershell
$turn2 = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$gameId`",`"playerAction`":`"I lean in and ask if he's heard any interesting rumors or if anyone needs help`"}"

Write-Host "Narrative: $($turn2.narrative)"
```

#### 4. Accept Quest
```powershell
$turn3 = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$gameId`",`"playerAction`":`"I accept the quest and prepare to set out immediately`"}"

Write-Host "Narrative: $($turn3.narrative)"
Write-Host "Events: $($turn3.appliedEvents -join ', ')"
```

### Session 2: Combat Encounter

#### 1. Create Warrior
```powershell
$warrior = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/new-game" `
  -ContentType "application/json" `
  -Body '{"playerName":"Bob","characterName":"Thorin Ironforge","race":"Dwarf","class":"Warrior"}'

$warriorId = $warrior.gameSessionId
```

#### 2. Leave the Tavern
```powershell
$leave = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$warriorId`",`"playerAction`":`"I thank Garrick and head outside to explore the surrounding forest`"}"

Write-Host $leave.narrative
Write-Host "Location: $($leave.currentLocation)"
```

#### 3. Encounter Combat
```powershell
$encounter = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$warriorId`",`"playerAction`":`"I continue walking down the forest path, keeping my hand on my weapon`"}"

Write-Host $encounter.narrative
```

#### 4. Fight
```powershell
$fight = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$warriorId`",`"playerAction`":`"I draw my axe and charge at the enemy with a battle cry!`"}"

Write-Host $fight.narrative
Write-Host "HP: $($fight.playerHP)/$($fight.maxPlayerHP)"
Write-Host "Events:"
$fight.appliedEvents | ForEach-Object { Write-Host "  - $_" }
```

### Session 3: Social/Investigation

#### 1. Create Mage
```powershell
$mage = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/new-game" `
  -ContentType "application/json" `
  -Body '{"playerName":"Carol","characterName":"Elara Moonwhisper","race":"Elf","class":"Mage"}'

$mageId = $mage.gameSessionId
```

#### 2. Observe the Room
```powershell
$observe = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$mageId`",`"playerAction`":`"I sit quietly in the corner, observing the other patrons and listening for interesting conversations`"}"

Write-Host $observe.narrative
```

#### 3. Investigate Mysterious Stranger
```powershell
$investigate = Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/game/turn" `
  -ContentType "application/json" `
  -Body "{`"gameSessionId`":`"$mageId`",`"playerAction`":`"I approach the mysterious hooded figure and politely introduce myself, asking if they're a traveler like me`"}"

Write-Host $investigate.narrative
Write-Host "Events:"
$investigate.appliedEvents | ForEach-Object { Write-Host "  - $_" }
```

## Using cURL (Cross-Platform)

### Create New Game
```bash
curl -X POST http://localhost:5000/api/game/new-game \
  -H "Content-Type: application/json" \
  -d '{
    "playerName": "Alice",
    "characterName": "Aria",
    "race": "Elf",
    "class": "Ranger"
  }'
```

### Process Turn
```bash
# Replace YOUR-GAME-SESSION-ID with actual ID
curl -X POST http://localhost:5000/api/game/turn \
  -H "Content-Type: application/json" \
  -d '{
    "gameSessionId": "YOUR-GAME-SESSION-ID",
    "playerAction": "I look around the tavern"
  }'
```

## Using Swagger UI

1. Start the API
2. Navigate to: http://localhost:5000
3. Expand `/api/game/new-game` endpoint
4. Click "Try it out"
5. Edit the request body
6. Click "Execute"
7. Copy the `gameSessionId` from response
8. Use it in the `/api/game/turn` endpoint

## Interesting Actions to Try

### Exploration
- "I search the room for hidden doors"
- "I examine the strange artifact on the shelf"
- "I ask about the history of this place"
- "I look out the window at the town"

### Social
- "I try to befriend the guard"
- "I attempt to persuade the merchant to lower his prices"
- "I share a drink with the other adventurers"
- "I ask the bard to play a song"

### Combat
- "I draw my weapon and attack"
- "I cast a fireball at the enemies"
- "I dodge and counterattack"
- "I try to find cover"

### Creative
- "I start juggling to entertain the crowd"
- "I try to pick the lock on the chest"
- "I attempt to climb the tower"
- "I disguise myself as a guard"

### Consequences Test
- "I insult the tavern keeper"
- "I try to steal from the merchant"
- "I challenge the strongest warrior to a duel"
- "I drink way too much ale"

## Expected Behaviors

### Health System
- Taking damage reduces HP
- HP cannot go below 0
- HP cannot exceed MaxHP
- Healing restores HP up to maximum

### Location System
- Actions can trigger location changes
- NPCs are tied to locations
- Location descriptions are contextual

### Quest System
- Quests can be given by NPCs
- Quest status updates based on actions
- Multiple quests can be active

### Memory System
- Recent actions are remembered
- Important events are prioritized
- Every 10 turns, a summary is generated

### NPC System
- New NPCs can appear
- NPCs have personality traits
- NPCs can be defeated (IsAlive = false)
- Relationship can change

## Troubleshooting

### "Game session not found"
- The gameSessionId is incorrect
- The session was deleted
- Check the ID carefully

### "Failed to parse Claude response"
- API key may be invalid
- Network connectivity issues
- Claude API may be down
- Check logs for details

### No narrative returned / Generic response
- Claude API quota exceeded
- Malformed prompt
- API key issues

### Database errors
- MongoDB not running (`docker ps` to check)
- Connection string incorrect
- MongoDB service not started

## Tips for Best Experience

1. **Be Specific**: Detailed actions get better responses
   - ❌ "I talk to him"
   - ✅ "I approach the merchant and ask about magical items for sale"

2. **Stay In Character**: The AI responds to your tone
   - ❌ "Go to the forest"
   - ✅ "I gather my courage and venture into the dark forest"

3. **Accept Consequences**: The AI will create interesting complications
   - Taking damage is part of the story
   - Failed actions can lead to interesting outcomes

4. **Explore**: Try unexpected actions
   - The AI can handle creative solutions
   - Don't just follow the obvious path

5. **Use the Memory System**: Reference past events
   - "I remember what Garrick told me about the forest"
   - "I return to the merchant from earlier"

## Monitoring Your Game

### Check Player Status
Query the database:
```sql
SELECT 
    gs.TurnNumber,
    pc.Name,
    pc.HP,
    pc.MaxHP,
    gs.CurrentLocation
FROM GameSessions gs
JOIN PlayerCharacters pc ON pc.GameSessionId = gs.Id
WHERE gs.Id = 'your-game-session-id';
```

### View Recent Memory
```sql
SELECT Content, Type, ImportanceScore, CreatedAt
FROM MemoryEntries
WHERE GameSessionId = 'your-game-session-id'
ORDER BY CreatedAt DESC
LIMIT 10;
```

### See All NPCs
```sql
SELECT Name, PersonalityTraits, RelationshipToPlayer, IsAlive
FROM Npcs
WHERE GameSessionId = 'your-game-session-id';
```

---

**Have fun exploring the Infinite Tavern!** 🎲🗡️🧙‍♂️
