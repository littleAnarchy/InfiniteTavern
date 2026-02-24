# Quick Start Guide

## 1. Setup Environment

### Install Prerequisites
- Install .NET 8 SDK: https://dotnet.microsoft.com/download
- Install Docker Desktop (for MongoDB)
- Get AI Provider API key:
  - **OpenAI**: https://platform.openai.com/ (Recommended for MVP)
  - **Anthropic**: https://console.anthropic.com/

### Run Setup Script
```powershell
.\setup.ps1
```

## 2. Configure API Key

Edit `src/InfiniteTavern.API/appsettings.json`:
```json
{
  "AI": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "sk-proj-..."
  },
  "Anthropic": {
    "ApiKey": "sk-ant-api03-..."
  }
}
```

## 3. Start MongoDB

```powershell
docker-compose up -d
```

Verify MongoDB is running:
```powershell
docker ps
# Should show: infinitetavern_mongodb
```

## 4. Run the API

```powershell
dotnet run --project src/InfiniteTavern.API
```

**Note:** No migrations needed! MongoDB creates collections automatically.

The API will start at: http://localhost:5000

## 6. Test the API

### Option A: Use Swagger UI
Open browser: http://localhost:5000

### Option B: Use cURL

**Create new game:**
```powershell
curl -X POST http://localhost:5000/api/game/new-game `
  -H "Content-Type: application/json" `
  -d '{\"playerName\":\"Alice\",\"characterName\":\"Thorin\",\"race\":\"Dwarf\",\"class\":\"Warrior\"}'
```

**Process turn:**
```powershell
# Replace {SESSION_ID} with the gameSessionId from the previous response
curl -X POST http://localhost:5000/api/game/turn `
  -H "Content-Type: application/json" `
  -d '{\"gameSessionId\":\"{SESSION_ID}\",\"playerAction\":\"I greet the tavern keeper\"}'
```

### Option C: Use REST Client (VS Code)
1. Install "REST Client" extension
2. Open `api-examples.http`
3. Click "Send Request" above any endpoint

## Troubleshooting

### Database connection fails
- Ensure MongoDB is running: `docker ps`
- Check connection string in appsettings.json
- Test connection: `mongosh mongodb://localhost:27017`

### Build errors
```powershell
dotnet clean
dotnet restore
dotnet build
```

### AI API errors (OpenAI/Claude)
- Verify API key is correct in appsettings.json
- Check API key has credits
- Check internet connection
- Verify `AI:Provider` is set to correct value

### View MongoDB Data
```powershell
mongosh
use InfiniteTavern
db.GameSessions.find().pretty()
```

## Next Steps

1. ✅ API is running
2. ✅ Database is connected
3. ✅ Claude is integrated
4. 🎮 Start playing!

Try different scenarios:
- Combat encounters
- Social interactions
- Exploration
- Puzzle solving
- Quest acceptance

The AI will adapt to your choices!
