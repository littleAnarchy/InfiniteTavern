# MongoDB Migration Guide

## Overview

Infinite Tavern has migrated from **PostgreSQL + Entity Framework Core** to **MongoDB** for better alignment with the document-oriented nature of RPG game sessions.

## Why MongoDB?

### Problems with PostgreSQL
1. **Complex JOINs**: Loading a game session required 5 separate table JOINs
2. **ORM Overhead**: Entity Framework added complexity for what's essentially JSON data
3. **Rigid Schema**: Relational constraints made it harder to add new features
4. **Natural Fit**: Game sessions are inherently document-structured

### Benefits of MongoDB
1. **Single Query**: `GameSession` loads as one document with all embedded data
2. **No Migrations**: Schema-less design allows flexible evolution
3. **Native JSON**: Perfect match for AI API integration
4. **Future Ready**: Built-in vector search for memory embeddings
5. **Simpler Code**: No navigation properties, no foreign keys

## Architecture Changes

### Before (PostgreSQL + EF Core)
```
GameSession (table)
  ├─ PlayerCharacter (table, FK)
  ├─ Npcs (table, FK)
  ├─ Quests (table, FK)
  └─ MemoryEntries (table, FK)
```

**Loading a session required:**
```csharp
var session = await _context.GameSessions
    .Include(s => s.PlayerCharacter)
    .Include(s => s.Npcs)
    .Include(s => s.Quests)
    .Include(s => s.MemoryEntries)
    .FirstOrDefaultAsync(s => s.Id == id);
```

### After (MongoDB)
```
GameSession (document)
  ├─ PlayerCharacter (embedded)
  ├─ Npcs[] (embedded array)
  ├─ Quests[] (embedded array)
  └─ MemoryEntries[] (embedded array)
```

**Loading a session now:**
```csharp
var session = await _repository.GetByIdAsync(id);
// ONE query, entire session loaded!
```

## Code Changes Summary

### 1. Domain Models
**GameSession.cs**
- Added `[BsonId]` attribute
- Changed collections from `ICollection<T>` to `List<T>`
- Removed navigation properties

**Embedded Entities (PlayerCharacter, Npc, Quest, MemoryEntry)**
- Removed `Id` property
- Removed `GameSessionId` foreign key
- Removed `GameSession` navigation property
- Now exist only as embedded documents

### 2. Infrastructure Layer
**Removed:**
- `InfiniteTavernDbContext.cs` (EF Core)
- `Migrations/` folder
- All Entity Framework configuration

**Added:**
- `MongoDbContext.cs` - MongoDB connection wrapper
- `GameRepository.cs` - Repository pattern implementation
- `IGameRepository` interface

### 3. Application Layer
**GameService.cs**
- Constructor now injects `IGameRepository` instead of `DbContext`
- `CreateNewGameAsync` creates complete document with embedded entities
- `ProcessTurnAsync` updates entire document in single operation
- No more `_context.SaveChangesAsync()` - uses `_repository.UpdateAsync()`

### 4. API Layer
**Program.cs**
```csharp
// OLD: PostgreSQL + EF Core
builder.Services.AddDbContext<InfiniteTavernDbContext>(options =>
    options.UseNpgsql(connectionString));

// NEW: MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped<IGameRepository, GameRepository>();
```

**appsettings.json**
```json
// OLD
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=infinitetavern..."
}

// NEW
"ConnectionStrings": {
  "DefaultConnection": "mongodb://localhost:27017"
},
"MongoDB": {
  "DatabaseName": "InfiniteTavern"
}
```

### 5. Docker Compose
```yaml
# OLD
services:
  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]

# NEW
services:
  mongodb:
    image: mongo:7.0
    ports: ["27017:27017"]
```

## Migration Steps (If You Have Existing Data)

### Export from PostgreSQL
```powershell
# Connect to PostgreSQL
psql -U postgres -d infinitetavern

# Export as JSON
COPY (
  SELECT json_agg(row_to_json(gs)) 
  FROM (
    SELECT * FROM "GameSessions"
  ) gs
) TO 'gamesessions.json';
```

### Import to MongoDB
```javascript
// Use mongosh
use InfiniteTavern

// Load exported data (adjust format as needed)
db.GameSessions.insertMany([...])
```

**Note:** Manual data transformation required to convert relational JOINs to embedded documents.

## Breaking Changes

### API Responses
✅ **No changes** - API contracts remain the same

### Database Queries
❌ **Cannot use Entity Framework queries**
✅ **Use MongoDB LINQ provider or MongoDB.Driver queries**

### Transactions
- PostgreSQL transactions → MongoDB transactions (if needed)
- Most operations are now single-document updates (atomic by default)

## Testing the Migration

### 1. Start MongoDB
```powershell
docker-compose up -d
```

### 2. Build & Run
```powershell
dotnet build
dotnet run --project src/InfiniteTavern.API
```

### 3. Create New Game
```powershell
curl -X POST http://localhost:5000/api/game/new-game `
  -H "Content-Type: application/json" `
  -d '{
    "playerName": "TestPlayer",
    "characterName": "Gandalf",
    "race": "Human",
    "class": "Wizard"
  }'
```

### 4. Verify in MongoDB
```powershell
mongosh
use InfiniteTavern
db.GameSessions.find().pretty()
```

You should see:
```json
{
  "_id": UUID("..."),
  "PlayerName": "TestPlayer",
  "CurrentLocation": "The Infinite Tavern",
  "PlayerCharacter": {
    "Name": "Gandalf",
    "Race": "Human",
    "Class": "Wizard",
    ...
  },
  "Npcs": [
    {
      "Name": "Garrick the Tavern Keeper",
      ...
    }
  ],
  "MemoryEntries": [...],
  "Quests": []
}
```

## Performance Comparison

### PostgreSQL (5 tables, FK relationships)
- **New Game**: 5 separate INSERTs + transaction
- **Load Session**: 1 SELECT + 4 JOINs
- **Update Turn**: Multiple UPDATE statements

### MongoDB (1 collection, embedded documents)
- **New Game**: 1 INSERT (entire document)
- **Load Session**: 1 FIND (entire document)
- **Update Turn**: 1 REPLACE (entire document)

**Result:** ~50% fewer database roundtrips per turn!

## Future Enhancements Enabled

1. **Vector Search**: MongoDB Atlas Search for memory embeddings
2. **Flexible Schema**: Add new fields without migrations
3. **Aggregation Pipeline**: Rich queries for analytics
4. **Sharding**: Horizontal scaling for millions of sessions
5. **Time Series**: Track player progression over time

## Rollback (If Needed)

To revert to PostgreSQL:

1. Restore NuGet packages:
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

2. Restore `InfiniteTavernDbContext.cs` from git history
3. Update `Program.cs` to use EF Core
4. Run migrations: `dotnet ef database update`

## Support

For issues with the migration, please check:
1. MongoDB is running: `docker ps` or `mongosh`
2. Connection string is correct in `appsettings.json`
3. Project builds without errors: `dotnet build`

## References

- [MongoDB C# Driver Documentation](https://www.mongodb.com/docs/drivers/csharp/current/)
- [MongoDB Best Practices](https://www.mongodb.com/docs/manual/core/data-modeling-introduction/)
- [Embedding vs Referencing](https://www.mongodb.com/basics/embedded-mongodb)
