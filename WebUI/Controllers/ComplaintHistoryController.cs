using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintHistoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintHistoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ComplaintHistory
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var histories = await _context.ComplaintHistories
            .Include(h => h.Complaint)
            .Include(h => h.OldStatus)
            .Include(h => h.NewStatus)
            .Include(h => h.AssignedBy)
            .Include(h => h.AssignedTo)
            .Include(h => h.Attachments)
            .ToListAsync();

        return Ok(histories);
    }

    // GET: api/ComplaintHistory/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var history = await _context.ComplaintHistories
            .Include(h => h.Complaint)
            .Include(h => h.OldStatus)
            .Include(h => h.NewStatus)
            .Include(h => h.AssignedBy)
            .Include(h => h.AssignedTo)
            .Include(h => h.Attachments)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (history == null) return NotFound();
        return Ok(history);
    }
    // GET: api/ComplaintHistory/ByComplaint/5
    [HttpGet("ByComplaint/{complaintId}")]
    public async Task<IActionResult> GetByComplaint(int complaintId)
    {
        var histories = await _context.ComplaintHistories
            .Where(h => h.ComplaintId == complaintId)
            .Include(h => h.OldStatus)
            .Include(h => h.NewStatus)
            .Include(h => h.AssignedBy)
            .Include(h => h.AssignedTo)
            .Include(h => h.Attachments)
           // .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        return Ok(histories);
    }

   
    // POST: api/ComplaintHistory/AddRemark
    [HttpPost("AddRemark")]
    public async Task<IActionResult> AddRemark([FromBody] AddRemarkDto dto)
    {
        int adminId = 15; // or any default admin ID


        // 2️⃣ Validate complaint exists
        var complaint = await _context.Complaints.FindAsync(dto.ComplaintId);
        if (complaint == null)
            return NotFound(new { error = $"Complaint with Id {dto.ComplaintId} not found." });

        // 3️⃣ Optional: validate AssignedToId exists
        if (dto.AssignedToId.HasValue)
        {
            var assignedExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedToId.Value);
            if (!assignedExists)
                return BadRequest(new { error = $"AssignedToId {dto.AssignedToId} does not exist." });
        }

        // 4️⃣ Add history
        var history = new ComplaintHistory
        {
            ComplaintId = dto.ComplaintId,
            OldStatusId = dto.OldStatusId,
            NewStatusId = dto.NewStatusId,
            AssignedById = adminId, // automatically from logged-in admin
            AssignedToId = dto.AssignedToId,
            Remarks = dto.Remarks,
            ActionDate = DateTime.UtcNow
        };

        _context.ComplaintHistories.Add(history);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Remark added successfully!", historyId = history.Id });
    }



    // PUT: api/ComplaintHistory/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintHistory history)
    {
        if (id != history.Id) return BadRequest();

        _context.Entry(history).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/ComplaintHistory/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var history = await _context.ComplaintHistories.FindAsync(id);
        if (history == null) return NotFound();

        _context.ComplaintHistories.Remove(history);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
