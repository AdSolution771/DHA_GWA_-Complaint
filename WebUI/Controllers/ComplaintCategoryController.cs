using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintCategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintCategoryController(AppDbContext context)
    {
        _context = context;
    }
    //test one
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.ComplaintCategories.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _context.ComplaintCategories.FindAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ComplaintCategory category)
    {
        _context.ComplaintCategories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintCategory category)
    {
        if (id != category.Id) return BadRequest();

        _context.Entry(category).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.ComplaintCategories.FindAsync(id);
        if (category == null) return NotFound();

        _context.ComplaintCategories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
