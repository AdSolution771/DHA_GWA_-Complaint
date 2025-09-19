using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintAttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintAttachmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ComplaintAttachments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var attachments = await _context.ComplaintAttachments
            .Include(ca => ca.Complaint)
            .Include(ca => ca.History)
            .Include(ca => ca.UploadedBy)
            .ToListAsync();
        return Ok(attachments);
    }

    // GET: api/ComplaintAttachments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var attachment = await _context.ComplaintAttachments
            .Include(ca => ca.Complaint)
            .Include(ca => ca.History)
            .Include(ca => ca.UploadedBy)
            .FirstOrDefaultAsync(ca => ca.Id == id);

        if (attachment == null) return NotFound();
        return Ok(attachment);
    }
    // GET: api/ComplaintAttachments/ByComplaint/5
    [HttpGet("ByComplaint/{complaintId}")]
    public async Task<IActionResult> GetByComplaint(int complaintId)
    {
        var attachments = await _context.ComplaintAttachments
            .Where(a => a.ComplaintId == complaintId)
            .Include(a => a.UploadedBy)
            .ToListAsync();

        if (!attachments.Any())
            return NotFound(new { message = "No attachments found for this complaint." });

        return Ok(attachments);
    }


    // POST: api/ComplaintAttachments
    [HttpPost]
    public async Task<IActionResult> Create(ComplaintAttachment attachment)
    {
        _context.ComplaintAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = attachment.Id }, attachment);
    }

    // PUT: api/ComplaintAttachments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintAttachment attachment)
    {
        if (id != attachment.Id) return BadRequest();

        _context.Entry(attachment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/ComplaintAttachments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _context.ComplaintAttachments.FindAsync(id);
        if (attachment == null) return NotFound();

        _context.ComplaintAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
