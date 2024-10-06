using TodoApi.Models;
using TodoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly TodoService _todoService;

        public TodosController(TodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Todo>>> GetAll()
        {
            return Ok(await _todoService.GetAllAsync());
        }
        
        [HttpPut("complete")]
        public async Task<ActionResult> SetCompleteStatusForAll(CompleteStatusRequest request)
        {
            // await _todoService.CompleteAsync(id);
            await _todoService.CompleteAsync(request.isComplete);
            return NoContent();
        }
    }

    public class CompleteStatusRequest
    {
        public bool isComplete { get; set;}
    }
}
