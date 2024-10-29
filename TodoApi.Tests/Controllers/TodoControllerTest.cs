using Moq;
using TodoApi.Services;
using TodoApi.Models;
using TodoApi.Controllers;
using Microsoft.AspNetCore.Mvc;

public class UserControllerTests
{
    [Fact]
    public void GetUser_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var mockTodoService = new Mock<ITodoService>();
        mockTodoService.Setup(service => service.GetAllAsync()).Returns(
            Task.FromResult(new List<Todo>
                {
                    new Todo
                    {
                        id = "1",
                        name = "Test Todo 1",
                        isComplete = true
                    },
                    new Todo
                    {
                        id = "2",
                        name = "Test Todo 2",
                        isComplete = false
                    }
                })
        );

        var controller = new TodosController(mockTodoService.Object);

        // Act
        var result = controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result.Result);
        var todos = Assert.IsType<List<Todo>>(actionResult.Value);
        Assert.NotNull(todos);
        Assert.Equal(2, todos.Count);

        // 验证第一个 Todo
        Assert.Equal("1", todos[0].id);
        Assert.Equal("Test Todo 1", todos[0].name);
        Assert.True(todos[0].isComplete);

        // 验证第二个 Todo
        Assert.Equal("2", todos[1].id);
        Assert.Equal("Test Todo 2", todos[1].name);
        Assert.False(todos[1].isComplete);
    }
}