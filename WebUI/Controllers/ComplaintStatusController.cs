using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintStatusController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintStatusController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ComplaintStatus
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.ComplaintStatuses.ToListAsync());

    // GET: api/ComplaintStatus/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var status = await _context.ComplaintStatuses.FindAsync(id);
        if (status == null) return NotFound();
        return Ok(status);
    }

    // POST: api/ComplaintStatus
    [HttpPost]
    public async Task<IActionResult> Create(ComplaintStatus status)
    {
        _context.ComplaintStatuses.Add(status);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = status.Id }, status);
    }


    // PUT: api/ComplaintStatus/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintStatus status)
    {
        if (id != status.Id) return BadRequest();

        _context.Entry(status).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("UpdateStatus/{complaintId}")]
    public async Task<IActionResult> UpdateStatus(int complaintId, [FromBody] int statusId)
    {
        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint == null) return NotFound();

        complaint.StatusId = statusId;
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/ComplaintStatus/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var status = await _context.ComplaintStatuses.FindAsync(id);
        if (status == null) return NotFound();

        _context.ComplaintStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
