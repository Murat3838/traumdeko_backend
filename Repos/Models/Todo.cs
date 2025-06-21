using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeragWebApp.Repos.Models;

[Table("todos")]
public class Todo
{
    [Key]
    public int Id { get; set; }

    [Column("description")]
    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    [Column("is_done")]
    public bool IsDone { get; set; } = false;

    [Column("assignee")]
    [MaxLength(50)]
    public string? Assignee { get; set; } // Hanife oder Murad

    [Column("event_id")]
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }
}
