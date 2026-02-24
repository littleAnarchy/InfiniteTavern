using InfiniteTavern.Domain.Entities;
using MongoDB.Driver;

namespace InfiniteTavern.Infrastructure.Data;

public interface IGameRepository
{
    Task<GameSession?> GetByIdAsync(Guid id);
    Task<GameSession> CreateAsync(GameSession session);
    Task UpdateAsync(GameSession session);
    Task DeleteAsync(Guid id);
}

public class GameRepository : IGameRepository
{
    private readonly IMongoCollection<GameSession> _sessions;

    public GameRepository(IMongoDatabase database)
    {
        _sessions = database.GetCollection<GameSession>("GameSessions");
    }

    public async Task<GameSession?> GetByIdAsync(Guid id)
    {
        return await _sessions
            .Find(s => s.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<GameSession> CreateAsync(GameSession session)
    {
        await _sessions.InsertOneAsync(session);
        return session;
    }

    public async Task UpdateAsync(GameSession session)
    {
        await _sessions.ReplaceOneAsync(
            s => s.Id == session.Id,
            session
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        await _sessions.DeleteOneAsync(s => s.Id == id);
    }
}
