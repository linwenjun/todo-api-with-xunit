using TodoApi.Models;

public interface ITodoService {
    Task<List<Todo>> GetAllAsync();
    Task CompleteAsync(bool v);
}