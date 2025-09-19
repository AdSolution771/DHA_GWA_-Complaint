using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context; // <-- inject DbContext

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            var reports = await _context.Complaints
                .Include(c => c.Initiator)
                .Include(c => c.AssignedTo)
                .Include(c => c.Histories)
                .Include(c => c.Attachments)
                .Include(c => c.Status)
                .Include(c => c.Feedbacks)
                .Select(c => new Report
                {
                    Id = c.Id,
                    ComplaintCode = c.ComplaintCode,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status != null ? c.Status.Name : "N/A",

                    Initiator = new WebUI.DTOs.UserDto
                    {
                        Id = c.Initiator.Id,
                        FullName = c.Initiator.FullName
                    },

                    AssignedTo = c.AssignedTo == null ? null : new WebUI.DTOs.UserDto
                    {
                        Id = c.AssignedTo.Id,
                        FullName = c.AssignedTo.FullName
                    },

                    Histories = c.Histories.Select(h => new ReportHistoryDto
                    {
                        AssignedById = h.AssignedById,
                        AssignedToId = h.AssignedToId,
                        Remarks = h.Remarks
                    }).ToList(),

                    Feedbacks = c.Feedbacks.Select(f => new ReportFeedbackDto
                    {
                        Id = f.Id,
                        Rating = f.Rating,
                        Comments = f.Comments
                    }).ToList()
                })
                .ToListAsync();

            return Ok(reports);
        }

        // ✅ GET: api/reports/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _context.Complaints
                .Include(c => c.Initiator)
                .Include(c => c.AssignedTo)
                .Include(c => c.Histories)
                .Include(c => c.Attachments)
                .Include(c => c.Status)
                .Include(c => c.Feedbacks)
                .Where(c => c.Id == id)
                .Select(c => new Report
                {
                    Id = c.Id,
                    ComplaintCode = c.ComplaintCode,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    Status = c.Status != null ? c.Status.Name : "N/A",

                    Initiator = new WebUI.DTOs.UserDto
                    {
                        Id = c.Initiator.Id,
                        FullName = c.Initiator.FullName
                    },

                    AssignedTo = c.AssignedTo == null ? null : new WebUI.DTOs.UserDto
                    {
                        Id = c.AssignedTo.Id,
                        FullName = c.AssignedTo.FullName
                    },

                    Histories = c.Histories.Select(h => new ReportHistoryDto
                    {
                        AssignedById = h.AssignedById,
                        AssignedToId = h.AssignedToId,
                        Remarks = h.Remarks
                    }).ToList(),

                    Feedbacks = c.Feedbacks.Select(f => new ReportFeedbackDto
                    {
                        Id = f.Id,
                        Rating = f.Rating,
                        Comments = f.Comments
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (report == null)
                return NotFound();

            return Ok(report);
        }
    }
}
