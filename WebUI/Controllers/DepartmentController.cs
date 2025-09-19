using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Department
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _context.Departments
            .Include(d => d.Users)
            .ToListAsync();

        return Ok(departments);
    }

    // GET: api/Department/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null) return NotFound();
        return Ok(department);
    }

    // POST: api/Department
    [HttpPost]
    public async Task<IActionResult> Create(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
    }

    // PUT: api/Department/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Department department)
    {
        if (id != department.Id) return BadRequest();

        _context.Entry(department).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Department/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return NotFound();

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
