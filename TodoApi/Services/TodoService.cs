using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoService: ITodoService
{
     private readonly IMongoCollection<Todo> _todosCollection;

     private readonly ILogger<TodoService> _Logger;
     public TodoService(IOptions<TodoStoreDatabaseSettings> todoStoreDatabaseSettings, ILogger<TodoService> logger) {
        var mongoClient = new MongoClient(todoStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(todoStoreDatabaseSettings.Value.DatabaseName);

        _todosCollection = mongoDatabase.GetCollection<Todo>(todoStoreDatabaseSettings.Value.TodoItemsCollectionName);

        _Logger = logger;
    }

    public async Task<List<Todo>> GetAllAsync()
    {
        return await _todosCollection.Find(_ => true).ToListAsync();
    }
    public int add(int number2, int number1) {
        return number1 + number2;
    }

    public async Task CompleteAsync(bool v)
    {
        await _todosCollection.UpdateManyAsync(
            new BsonDocument("isComplete", !v), 
            new BsonDocument("$set", new BsonDocument("isComplete", v))
        );
    }
}