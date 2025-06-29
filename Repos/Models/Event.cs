using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace VeragWebApp.Repos.Models;

[Table("events")]
public class Event
{
    [Key]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [NotMapped]
    public string Ev
    {
        get => Name;
        set => Name = value;
    }

    [Column("customer_name")]
    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [Column("event_date")]
    public DateTime EventDate { get; set; }

    [Column("event_start")]
    public DateTime? EventStart { get; set; }

    [Column("event_end")]
    public DateTime? EventEnd { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_closed")]
    public bool IsClosed { get; set; } = false;

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [Column("location")]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Column("deposit")]
    public decimal Deposit { get; set; }

    [Column("outstanding")]
    public decimal Outstanding { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    public bool Backdrop { get; set; }

    public bool GuestTables { get; set; }

    public int? GuestCount { get; set; }

    public bool Catering { get; set; }

    public string? Dish { get; set; }

    public int? CateringCount { get; set; }

    [Column("event_type_id")]
    public int? EventTypeId { get; set; }

    [ForeignKey("EventTypeId")]
    public EventType? EventType { get; set; }

    [Column("street")]
    [MaxLength(150)]
    public string? Street { get; set; }

    [Column("zip")]
    [MaxLength(20)]
    public string? Zip { get; set; }

    [Column("tip")]
    public decimal? Tip { get; set; }

    public ICollection<Income>? Incomes { get; set; }

    public ICollection<Todo>? Todos { get; set; }
}
