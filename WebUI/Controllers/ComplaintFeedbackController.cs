using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class ComplaintFeedbackController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintFeedbackController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ComplaintFeedback
    [HttpGet]
    public async Task<IActionResult> GetFeedbacks()
    {
        var feedbacks = await _context.ComplaintFeedbacks
            .Include(f => f.Complaint)       // join Complaint
            .Include(f => f.FeedbackBy)      // join User (assuming navigation property)
            .Select(f => new ComplaintFeedbackDto
            {
                ComplaintId = f.ComplaintId,
                ComplaintNumber = f.Complaint.ComplaintCode,   // from Complaint table
                ComplaintTitle = f.Complaint.Title,              // from Complaint table
                FeedbackById = f.FeedbackById,
                UserName = f.FeedbackBy.FullName,                // from User table
                Rating = f.Rating,
                BehaviorRating = f.BehaviorRating,
                Comments = f.Comments,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(feedbacks);
    }



    // GET: api/ComplaintFeedback/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var feedback = await _context.ComplaintFeedbacks
            .Include(f => f.Complaint)
            .Include(f => f.FeedbackBy)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feedback == null) return NotFound();
        return Ok(feedback);
    }

    // POST: api/ComplaintFeedback
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ComplaintFeedbackDto dto)
    {
        var complaint = await _context.Complaints.FindAsync(dto.ComplaintId);
        var user = await _context.Users.FindAsync(dto.FeedbackById);

        if (complaint == null || user == null)
            return BadRequest("Invalid ComplaintId or FeedbackById");

        var feedback = new ComplaintFeedback
        {
            ComplaintId = dto.ComplaintId,
            FeedbackById = dto.FeedbackById,
            Rating = dto.Rating,
            BehaviorRating = dto.BehaviorRating,
            Comments = dto.Comments,
            CreatedAt = DateTime.UtcNow
        };

        _context.ComplaintFeedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = feedback.Id }, feedback);
    }



    // PUT: api/ComplaintFeedback/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ComplaintFeedback feedback)
    {
        if (id != feedback.Id) return BadRequest();

        _context.Entry(feedback).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/ComplaintFeedback/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var feedback = await _context.ComplaintFeedbacks.FindAsync(id);
        if (feedback == null) return NotFound();

        _context.ComplaintFeedbacks.Remove(feedback);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
