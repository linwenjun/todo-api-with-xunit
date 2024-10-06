using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Net;
using Testcontainers.MongoDb;
using TodoApi.Models;
using System.Text.Json;
using TodoApi.Controllers;
using Newtonsoft.Json;
using System.Text;

namespace TodoApi.Tests.ApiTest;

public class GetTodosTest : IClassFixture<WebApplicationFactory<Program>>
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
                config.AddInMemoryCollection(new Dictionary<string, string?>
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
            new Todo { name = "Test Todo 1", isComplete = true },
            new Todo { name = "Test Todo 2", isComplete = false },
            new Todo { name = "Test Todo 3", isComplete = false }
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PutTodos_All_ITems_ShouldBeComplete(bool tobeComplete)
    {

        var todos = new List<Todo> {
            new Todo { name = "Test Todo 1", isComplete = true },
            new Todo { name = "Test Todo 2", isComplete = false },
            new Todo { name = "Test Todo 3", isComplete = false }
        };
        await _collection.InsertManyAsync(todos);

        // when
        // Arrange
        var request = new CompleteStatusRequest { isComplete = tobeComplete };
        var jsonContent = JsonConvert.SerializeObject(request);
        var putContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var putResponse = await _client.PutAsync("/api/todos/complete", putContent);

        // assert nocontent status code
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // then
        var response = await _client.GetAsync("/api/todos");
        var content = await response.Content.ReadAsStringAsync();
        var returnedTodos = System.Text.Json.JsonSerializer.Deserialize<List<Todo>>(content);
        // Assert.NotNull(returnedTodos);
        Assert.Equal(tobeComplete, returnedTodos[0].isComplete);
        Assert.Equal(tobeComplete, returnedTodos[1].isComplete);
        Assert.Equal(tobeComplete, returnedTodos[2].isComplete);
    }

    public void Dispose()
    {
        _mongoContainer.DisposeAsync().AsTask().Wait();
    }
}
