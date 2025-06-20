using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTypesController : ControllerBase
{
    private readonly VeragDB _db;
    public EventTypesController(VeragDB db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventType>>> GetAll()
    {
        return await _db.EventTypes.OrderBy(t => t.Name).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventType?>> Get(int id)
    {
        var type = await _db.EventTypes.FindAsync(id);
        if (type == null) return NotFound();
        return type;
    }

    [HttpPost]
    public async Task<ActionResult<EventType>> Create(EventType type)
    {
        _db.EventTypes.Add(type);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = type.Id }, type);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, EventType type)
    {
        if (id != type.Id) return BadRequest();
        _db.Entry(type).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.EventTypes.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _db.EventTypes.FindAsync(id);
        if (type == null) return NotFound();
        if (await _db.Events.AnyAsync(e => e.EventTypeId == id))
            return BadRequest("EventType is in use.");
        _db.EventTypes.Remove(type);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
