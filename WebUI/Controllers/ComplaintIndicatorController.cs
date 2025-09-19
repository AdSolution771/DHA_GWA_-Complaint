using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintIndicatorController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintIndicatorController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ComplaintIndicator
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var indicators = await _context.ComplaintIndicators
            .Include(i => i.Complaint)
            .Include(i => i.NotifiedTo)
            .ToListAsync();

        return Ok(indicators);
    }

    // GET: api/ComplaintIndicator/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var indicator = await _context.ComplaintIndicators
            .Include(i => i.Complaint)
            .Include(i => i.NotifiedTo)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (indicator == null) return NotFound();
        return Ok(indicator);
    }

    // POST: api/ComplaintIndicator
    [HttpPost]
    public async Task<IActionResult> Create(ComplaintIndicator indicator)
    {
        _context.ComplaintIndicators.Add(indicator);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = indicator.Id }, indicator);
    }

    // PUT: api/ComplaintIndicator/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintIndicator indicator)
    {
        if (id != indicator.Id) return BadRequest();

        _context.Entry(indicator).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/ComplaintIndicator/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var indicator = await _context.ComplaintIndicators.FindAsync(id);
        if (indicator == null) return NotFound();

        _context.ComplaintIndicators.Remove(indicator);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
