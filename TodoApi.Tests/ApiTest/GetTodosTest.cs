using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace TodoApi.Tests.ApiTest;

public class GetTodosTest: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetTodosTest(WebApplicationFactory<Program> factory)
    {
        // 创建HttpClient用于发送请求
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTodos_ReturnsSuccessStatusCode()
    {
        // Act - 发送请求到 /todos
        var response = await _client.GetAsync("/api/todos");

        // Assert - 验证状态码
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
