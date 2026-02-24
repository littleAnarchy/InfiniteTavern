using MongoDB.Driver;

namespace InfiniteTavern.Infrastructure.Data;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(IMongoDatabase database)
    {
        Database = database;
    }
}
