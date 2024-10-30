using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoService {
    Task<List<Todo>> GetAllAsync();
    Task CompleteAsync(bool v);
}