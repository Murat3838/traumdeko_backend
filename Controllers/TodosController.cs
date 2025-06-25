using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.DTOs;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Controllers;

public class TodoRequest
{
    public Todo Todo { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly VeragDB _db;
    private static readonly string[] _allowedAssignees = new[] { "Hanife", "Murad" };

    public TodosController(VeragDB db)
    {
        _db = db;
    }

    // GET: api/todos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoWithEventDto>>> GetAll([FromQuery] string? status)
    {
        var query = _db.Todos.Include(t => t.Event).AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (status.ToLower() == "open")
            {
                query = query.Where(t => !t.IsDone);
            }
            else if (status.ToLower() == "done")
            {
                query = query.Where(t => t.IsDone);
            }
        }

        var todos = await query.OrderBy(t => t.IsDone).ThenByDescending(t => t.Event.EventDate).ToListAsync();
        
        return Ok(todos.Select(TodoWithEventDto.FromTodo));
    }

    // GET: api/todos/event/5
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<Todo>>> GetByEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.Id == eventId)) return NotFound("Event not found");
        var list = await _db.Todos.Where(t => t.EventId == eventId).OrderBy(t => t.IsDone).ThenBy(t => t.Id).ToListAsync();
        return Ok(list);
    }

    // POST: api/todos
    [HttpPost]
    public async Task<ActionResult<Todo>> Create([FromBody] TodoRequest request)
    {
        var todo = request.Todo;
        if (!await _db.Events.AnyAsync(e => e.Id == todo.EventId))
            return BadRequest("Event not found");
        if (todo.Assignee != null && !_allowedAssignees.Contains(todo.Assignee, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Assignee must be Hanife or Murad");

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSingle), new { id = todo.Id }, todo);
    }

    // GET api/todos/10
    [HttpGet("{id}")]
    public async Task<ActionResult<Todo>> GetSingle(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return NotFound();
        return todo;
    }

    // PUT api/todos/10
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoRequest request)
    {
        var todo = request.Todo;
        if (id != todo.Id) return BadRequest();
        if (todo.Assignee != null && !_allowedAssignees.Contains(todo.Assignee, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Assignee must be Hanife or Murad");

        if (!await _db.Events.AnyAsync(e => e.Id == todo.EventId))
            return BadRequest("Event not found");

        _db.Entry(todo).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Todos.AnyAsync(t => t.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    // DELETE api/todos/10
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        if (todo == null) return NotFound();
        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
