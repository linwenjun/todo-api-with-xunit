
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Testcontainers.MongoDb;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;


public class TodoServiceTest: IAsyncLifetime
{
    private readonly IMongoCollection<Todo> _collection;
    private readonly TodoService _todoService;

    private readonly string _databaseName = "IntegrationTestDb";

     private readonly MongoDbContainer _mongoContainer;

    public TodoServiceTest()
    {
        
        _mongoContainer= new MongoDbBuilder()
            .WithImage("mongodb/mongodb-community-server:7.0.5-ubi9")
            .Build();

        _mongoContainer.StartAsync().Wait();

        var _mongoClient = new MongoClient(_mongoContainer.GetConnectionString());

        var _database = _mongoClient.GetDatabase(_databaseName);

        _collection = _database.GetCollection<Todo>("Todos");
        
        var mockSettings = new Mock<IOptions<TodoStoreDatabaseSettings>>();
        
        mockSettings.Setup(s => s.Value).Returns(new TodoStoreDatabaseSettings
        {
            ConnectionString = _mongoContainer.GetConnectionString(),
            DatabaseName = _databaseName,
            TodoItemsCollectionName = "Todos"
        });

        // 初始化 TodoService
        _todoService = new TodoService(mockSettings.Object);
    }
    
    [Fact]
    public async void Test1()
    {

        var todos = new List<Todo> {
            new Todo { name = "Test Todo 1", isComplete = true },
            new Todo { name = "Test Todo 2", isComplete = false }
        };

        await _collection.InsertManyAsync(todos);

        List<Todo> todos1 = await _todoService.GetAllAsync();
        
        Assert.Equal(2, todos1.Count);
    }

    // IAsyncLifetime 的实现，用于在测试开始前后启动和销毁容器
    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
    }
}