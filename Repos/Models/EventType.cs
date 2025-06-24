using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace VeragWebApp.Repos.Models;

[Table("event_types")]
public class EventType
{
    [Key]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    public bool IsCostCategory { get; set; }

    public ICollection<Event>? Events { get; set; }
}
