using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeragWebApp.Repos.Models;

[Table("incomes")]
public class Income
{
    [Key]
    public int Id { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("description")]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; set; }

    [ForeignKey("Event")]
    public int? EventId { get; set; }
    public Event? Event { get; set; }
}
