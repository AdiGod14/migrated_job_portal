using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("jobs");
    public IMongoCollection<Employer> Employers => _database.GetCollection<Employer>("employers");
    public IMongoCollection<Application> Applications => _database.GetCollection<Application>("applications");
    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
}
