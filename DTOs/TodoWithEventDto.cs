using VeragWebApp.Repos.Models;

namespace VeragWebApp.DTOs;

public class TodoWithEventDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string? Assignee { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }

    public static TodoWithEventDto FromTodo(Todo todo)
    {
        return new TodoWithEventDto
        {
            Id = todo.Id,
            Description = todo.Description,
            IsDone = todo.IsDone,
            Assignee = todo.Assignee,
            EventId = todo.EventId,
            EventName = todo.Event?.Name
        };
    }
}
