using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncomesController : ControllerBase
{
    private readonly VeragDB _db;
    public IncomesController(VeragDB db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Income>>> GetAll([FromQuery] int? eventId)
    {
        var query = _db.Incomes
            .Include(i => i.Event)
            .AsQueryable();
        if (eventId.HasValue)
        {
            query = query.Where(i => i.EventId == eventId.Value);
        }
        return await query.OrderByDescending(i => i.Date).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Income?>> Get(int id)
    {
        var inc = await _db.Incomes.Include(i => i.Event).FirstOrDefaultAsync(i => i.Id == id);
        if (inc == null) return NotFound();
        return inc;
    }

    [HttpPost]
    public async Task<ActionResult<Income>> Create(Income income)
    {
        if (income.EventId.HasValue && !await _db.Events.AnyAsync(e => e.Id == income.EventId))
            return BadRequest("Event not found");
        _db.Incomes.Add(income);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = income.Id }, income);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Income income)
    {
        if (id != income.Id) return BadRequest();
        _db.Entry(income).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.Incomes.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inc = await _db.Incomes.FindAsync(id);
        if (inc == null) return NotFound();
        _db.Incomes.Remove(inc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
