# Infinite Tavern - AI Dungeon Master Prototype

A persistent AI-powered RPG backend where Claude acts as Dungeon Master and the backend is the authoritative game engine.

## 🎯 Core Principle

**LLM is NOT source of truth. Backend is source of truth.**

- Claude generates narrative and suggests events
- Backend validates, applies state changes, and persists data
- Game state is always deterministic and traceable

## 🏗 Architecture

### Tech Stack
- **ASP.NET Core 8** Web API
- **MongoDB 7.0** database (document-oriented)
- **AI Providers:**
  - OpenAI GPT-4o mini (default)
  - Anthropic Claude Sonnet 3.5

### Project Structure
```
InfiniteTavern/
├── src/
│   ├── InfiniteTavern.Domain/         # Domain entities (MongoDB documents)
│   ├── InfiniteTavern.Application/    # Business logic & services
│   ├── InfiniteTavern.Infrastructure/ # MongoDB repository & data access
│   └── InfiniteTavern.API/            # Web API controllers
├── frontend/                           # React frontend (Vite + TypeScript)
│   ├── src/components/                # UI components
│   ├── src/services/                  # API client
│   └── src/types/                     # TypeScript types
└── docker-compose.yml                  # MongoDB container
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- MongoDB 7.0+ (or Docker)
- AI Provider API key:
  - **OpenAI** (GPT-4o mini) - Recommended for MVP
  - **Anthropic** (Claude Sonnet 3.5) - Premium quality

### 1. Setup MongoDB

**Option A: Using Docker** (Recommended)
```powershell
docker-compose up -d
```

**Option B: Local MongoDB**
- Install MongoDB 7.0+
- Start MongoDB service
- Database `InfiniteTavern` will be created automatically

### 2. Configure API Key

Edit `src/InfiniteTavern.API/appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  },
  "Anthropic": {
    "ApiKey": "your-anthropic-api-key-here"
  }
}
```

### 3. Run the Backend API

```powershell
dotnet run --project src/InfiniteTavern.API
```

API will be available at: `http://localhost:5000`

Swagger UI: `http://localhost:5000`

### 4. Run the Frontend (Optional)

```powershell
cd frontend
npm install
npm run dev
```

Frontend will be available at: `http://localhost:3000`

**Features:**
- 🎮 Character creation UI
- 💬 Chat-like game interface
- 📊 Real-time stats display
- 📜 Turn history
- 🎨 Dark theme with smooth animations

See [frontend/README.md](frontend/README.md) for details.

## 📡 API Endpoints

### Create New Game
```http
POST /api/game/new-game
Content-Type: application/json

{
  "playerName": "John",
  "characterName": "Thorin",
  "race": "Dwarf",
  "class": "Warrior"
}
```

**Response:**
```json
{
  "gameSessionId": "guid",
  "message": "Welcome to the Infinite Tavern!",
  "playerStats": {
    "name": "Thorin",
    "race": "Dwarf",
    "class": "Warrior",
    "level": 1,
    "hp": 20,
    "maxHP": 20,
    "strength": 12,
    "dexterity": 10,
    "intelligence": 8
  }
}
```

### Process Turn
```http
POST /api/game/turn
Content-Type: application/json

{
  "gameSessionId": "guid-from-new-game",
  "playerAction": "I approach the tavern keeper and ask about local rumors"
}
```

**Response:**
```json
{
  "narrative": "Garrick leans in with a knowing smile...",
  "playerHP": 20,
  "maxPlayerHP": 20,
  "currentLocation": "The Infinite Tavern",
  "appliedEvents": [
    "Met new NPC: Mysterious Stranger"
  ]
}
```

## 🧩 Core Features

### Turn Flow
1. Load game session with all context
2. Gather recent memories (last 5 turns)
3. Gather important memories (top 3 by importance)
4. Build AI prompt with full game state
5. Call Claude API
6. Validate JSON response
7. Apply events (HP changes, location changes, etc.)
8. Persist state
9. Return narrative to player

### Memory System (MVP)
- Stores last 10 turns as raw events
- Every 10 turns, generates a summary
- Summaries have high importance scores
- Structured for future embedding support

### Event System
Claude can only suggest events - backend applies them:
- `damage`: Reduce HP
- `heal`: Restore HP
- `item_found`: Log item acquisition
- `item_lost`: Log item loss

### Dice Rolling
Backend rolls all dice using standard notation:
- `1d20` - Roll one 20-sided die
- `2d6+3` - Roll two 6-sided dice and add 3
- `3d8-2` - Roll three 8-sided dice and subtract 2

## 🗃️ Database Schema (MongoDB)

### GameSession (Root Document)
- `_id`: Guid (BsonId)
- `PlayerName`, `CurrentLocation`, `WorldTime`, `TurnNumber`, `CreatedAt`
- **Embedded Documents:**
  - `PlayerCharacter`: Name, Race, Class, Level, HP, Stats
  - `Npcs[]`: Array of NPCs with personality and location
  - `Quests[]`: Array of quests with status
  - `MemoryEntries[]`: Array of memories with importance scores

**Why MongoDB?**
- Game sessions are naturally document-structured
- No complex JOINs needed - single query loads entire session
- Flexible schema for future features
- Native JSON support for AI integration
- Future: Vector search for memory embeddings

## 🤖 Claude Integration

### System Prompt Rules
- Never modify stats directly
- Only suggest state changes through events
- Never contradict established facts
- Return ONLY JSON, no extra text
- Keep fantasy tone consistent

### Response Format
```json
{
  "narrative": "Scene description...",
  "events": [
    {
      "type": "damage",
      "target": "player",
      "amount": 4,
      "reason": "Goblin attack"
    }
  ],
  "new_npcs": [],
  "quest_updates": [],
  "location_change": null
}
```

## 🛠 Development

### Build Solution
```powershell
dotnet build
```

### Run Tests
```powershell
dotnet test
```

### Clean Build
```powershell
dotnet clean
dotnet restore
dotnet build
```

### View MongoDB Data
```powershell
# Using mongosh
mongosh
use InfiniteTavern
db.GameSessions.find().pretty()
```

## 📝 Notes

### This is an MVP
- No authentication
- No user management
- Single-player only
- No inventory system yet
- No combat system yet
- No frontend yet

### Future Enhancements
- Add vector embeddings for memory
- Implement inventory system
- Add structured combat
- Multi-session support
- Real-time updates via SignalR
- Character progression system

## 🔒 Security Notes

- **API Key**: Never commit real API keys
- **Database**: Use strong passwords in production
- **CORS**: Configure appropriately for production
- **HTTPS**: Always use HTTPS in production

## 📄 License

MIT - This is a prototype for learning and experimentation.
