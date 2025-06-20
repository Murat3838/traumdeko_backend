using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeragWebApp.Repos.Models;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum Payer
{
    Firma = 0,
    Hanife = 1,
    Murad = 2
}

[Table("expenses")]
public class Expense
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

    [Column("event_id")]
    public int? EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    [Column("payer")]
    public Payer Payer { get; set; }
}
