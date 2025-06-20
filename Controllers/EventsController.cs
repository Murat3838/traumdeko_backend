using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly VeragDB _db;
    public EventsController(VeragDB db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll()
    {
        return await _db.Events
            .Include(e => e.Incomes)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event?>> Get(int id)
    {
        var ev = await _db.Events.Include(e => e.Incomes).Include(e=>e.EventType).FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null) return NotFound();
        return ev;
    }

    [HttpPost]
    public async Task<ActionResult<Event>> Create([FromBody] Event ev)
    {
        if (ev.EventTypeId.HasValue && !await _db.EventTypes.AnyAsync(t=>t.Id==ev.EventTypeId))
        {
            return BadRequest("EventType not found");
        }
        ev.CreatedAt = DateTime.UtcNow;
        ev.Outstanding = ev.TotalAmount - ev.Deposit;
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.Id }, ev);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Event ev)
    {
        if (id != ev.Id) return BadRequest();
        var existing = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e=>e.Id==id);
        if(existing==null) return NotFound();
        if(existing.IsClosed) return BadRequest("Event is closed");
        if (ev.EventTypeId.HasValue && !await _db.EventTypes.AnyAsync(t=>t.Id==ev.EventTypeId))
        {
            return BadRequest("EventType not found");
        }
        ev.CreatedAt = existing.CreatedAt; // preserve
        ev.IsClosed = existing.IsClosed;
        ev.ClosedAt = existing.ClosedAt;
        ev.IsClosed = existing.IsClosed;
        ev.ClosedAt = existing.ClosedAt;
        ev.Outstanding = ev.TotalAmount - ev.Deposit;
        _db.Entry(ev).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.Events.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if(ev.IsClosed) return BadRequest("Event is closed");
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        if (ev.IsClosed) return BadRequest("Already closed");
        // Set amounts
        ev.Deposit = ev.TotalAmount;
        ev.Outstanding = 0;
        ev.IsClosed = true;
        ev.ClosedAt = DateTime.UtcNow;

        // automatisch eine Einnahme buchen, falls noch keine existiert
        if (!await _db.Incomes.AnyAsync(i => i.EventId == ev.Id))
        {
            var inc = new Income
            {
                Date = ev.ClosedAt.Value,
                Description = $"Einnahme Event: {ev.Name}",
                Amount = ev.TotalAmount,
                EventId = ev.Id
            };
            _db.Incomes.Add(inc);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
