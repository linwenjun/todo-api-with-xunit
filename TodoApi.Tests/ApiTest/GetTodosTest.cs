using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Net;
using Testcontainers.MongoDb;
using TodoApi.Models;
using System.Text.Json;

namespace TodoApi.Tests.ApiTest;

public class GetTodosTest: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IMongoCollection<Todo> _collection;
    private readonly string _databaseName = "IntegrationTestDb";
    private readonly MongoDbContainer _mongoContainer;
    private readonly WebApplicationFactory<Program> _factory;

    public GetTodosTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongodb/mongodb-community-server:7.0.5-ubi9")
            .Build();

        _mongoContainer.StartAsync().Wait();

        var mongoClient = new MongoClient(_mongoContainer.GetConnectionString());
        var database = mongoClient.GetDatabase(_databaseName);
        _collection = database.GetCollection<Todo>("Todos");

        // 使用新的配置创建客户端
        _client = CreateClientWithCustomConfiguration();
    }

    private HttpClient CreateClientWithCustomConfiguration()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"TodosDatabase:ConnectionString", _mongoContainer.GetConnectionString()},
                    {"TodosDatabase:DatabaseName", _databaseName},
                    {"TodosDatabase:TodoItemsCollectionName", "Todos"}
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetTodos_ReturnsSuccessStatusCode()
    {
        // Arrange
        var todos = new List<Todo> {
            new Todo { Name = "Test Todo 1", IsComplete = true },
            new Todo { Name = "Test Todo 2", IsComplete = false },
            new Todo { Name = "Test Todo 3", IsComplete = false }
        };
        await _collection.InsertManyAsync(todos);

        // Act - 发送请求到 /todos
        var response = await _client.GetAsync("/api/todos");

        // Assert - 验证状态码
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert - 验证响应体包含2个待办事项
        var content = await response.Content.ReadAsStringAsync();
        var returnedTodos = System.Text.Json.JsonSerializer.Deserialize<List<Todo>>(content);
        Assert.NotNull(returnedTodos);
        Assert.Equal(3, returnedTodos.Count);
    }

    public void Dispose()
    {
        _mongoContainer.DisposeAsync().AsTask().Wait();
    }
}
