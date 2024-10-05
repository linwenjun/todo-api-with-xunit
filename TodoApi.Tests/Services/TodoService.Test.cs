using TodoApi.Services;

namespace TodoApi.Tests.Services;


public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        TodoServices todoServices = new TodoServices();
        
        Assert.Equal(4, todoServices.add(2, 2));
    }
}