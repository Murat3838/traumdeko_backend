using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeragWebApp.Repos;
using VeragWebApp.Repos.Models;

namespace VeragWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly VeragDB _db;
    public ExpensesController(VeragDB db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetAll([FromQuery] int? eventId)
    {
        var query = _db.Expenses
            .Include(e => e.Event)
            .AsQueryable();
        if (eventId.HasValue)
        {
            query = query.Where(e => e.EventId == eventId.Value);
        }
        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense?>> Get(int id)
    {
        var exp = await _db.Expenses.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == id);
        if (exp == null) return NotFound();
        return exp;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> Create(Expense expense)
    {
        if (expense.EventId.HasValue && !await _db.Events.AnyAsync(ev => ev.Id == expense.EventId))
        {
            return BadRequest("Event not found");
        }
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        if (expense.EventId.HasValue && !await _db.Events.AnyAsync(ev => ev.Id == expense.EventId))
        {
            return BadRequest("Event not found");
        }
        _db.Entry(expense).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.Expenses.Any(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var exp = await _db.Expenses.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == id);
        if (exp == null) return NotFound();
        _db.Expenses.Remove(exp);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/reimburse")]
    public async Task<IActionResult> Reimburse(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);

        if (expense == null)
        {
            return NotFound();
        }

        if (expense.OriginalPayer.HasValue)
        {
            return BadRequest("Expense has already been reimbursed.");
        }

        expense.OriginalPayer = expense.Payer;
        expense.Payer = Payer.Firma;

        _db.Entry(expense).State = EntityState.Modified;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
