using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebUI.DTOs;
using WebUI.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Json;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;

namespace WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OneSignalService _oneSignalService;
        private readonly FirebaseNotificationService _firebaseService;
     

        public ComplaintsController(AppDbContext context, OneSignalService oneSignalService, FirebaseNotificationService firebaseService)
        {
            _context = context;
            _oneSignalService = oneSignalService;
            _firebaseService = firebaseService;

        }
        // Test endpoint to verify UTC DateTime serialization
        [HttpGet("now")]
        public ActionResult<string> GetNow()
        {
            // return UTC with 'Z'
            return Ok(DateTime.UtcNow.ToString("o")); // "o" = round-trip, adds 'Z'
        }

        // GET: api/Complaints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Complaint>>> GetComplaints()
        {
            return await _context.Complaints
                                 .Include(c => c.Category)
                                 .Include(c => c.Status)
                                 .Include(c => c.Initiator)
                                 .Include(c => c.AssignedTo)
                                 .Include(c => c.DefaultReceiver)
                                 .ToListAsync();
        }

        // GET: api/Complaints/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComplaint(int id)
        {
            // ✅ Eager load Status and Attachments
            var complaint = await _context.Complaints
                .Include(c => c.Status)        // Load status navigation property
                .Include(c => c.Attachments)   // Load attachments navigation property
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound();

            // ✅ Fetch user info
            var user = await _context.Users
                .Where(u => u.Id == complaint.InitiatorId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                   // Username = u.Username,
                    Email = u.Email,
                    Address = u.Address,
                    PlotNumber = u.PlotNumber
                })
                .FirstOrDefaultAsync();

            var response = new ComplaintResponseDto
            {
                Id = complaint.Id,
                ComplaintCode = complaint.ComplaintCode,
                ComplaintNo = complaint.ComplaintCode,
                Title = complaint.Title,
                Description = complaint.Description,
                CategoryId = complaint.CategoryId,
                CategoryName = await _context.ComplaintCategories
                    .Where(c => c.Id == complaint.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync() ?? "N/A",
                InitiatorId = complaint.InitiatorId,
                Initiator = new WebUI.DTOs.UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName
                },
                Status = complaint.Status?.Name ?? "Unknown", // ✅ Now shows actual status
                Latitude = complaint.Latitude,
                Longitude = complaint.Longitude,
                CreatedAt =  complaint.CreatedAt,
                Attachments = complaint.Attachments.Select(a => new ComplaintAttachmentDto
                {
                    Id = a.Id,
                    FilePath = $"{Request.Scheme}://{Request.Host}/{a.FilePath}", // ✅ Full URL for Flutter
                    FileType = a.FileType,
                    UploadedAt = a.UploadedAt,
                    IsInitialSubmission = a.IsInitialSubmission
                }).ToList()

            };

            return Ok(response); // ✅ Returns JSON object with actual status
        }

 


        [HttpGet("byrole/{role}/{userId}")]
        public async Task<IActionResult> GetComplaintsByRole(string role, int userId)
{
         IQueryable<Complaint> query = _context.Complaints
        .Include(c => c.Category)
        .Include(c => c.Status)
        .Include(c => c.AssignedTo)
        .Include(c => c.Initiator);

         switch (role.ToLower())
        {
        case "admin":
            // ✅ Admin sees everything
            break;

                //case "supervisor":
                //    // ✅ Complaints assigned to this supervisor
                //    query = query.Where(c => c.AssignedToId == userId && c.AssignedTo.UserType == "Supervisor");
                //    break;

               case "supervisor":
    // Supervisors see ALL complaints except Open (1) and Rejected (3)
    query = query.Where(c => c.StatusId != 1 && c.StatusId != 3);
    break;




                case "executor":
                    // ✅ Complaints assigned to this executor but not Rejected (StatusId != 3)
                    query = query.Where(c =>
                        c.AssignedToId == userId &&
                        c.AssignedTo.UserType == "Executor" &&
                        c.StatusId != 3
                    );
                    break;


                case "resident":
        case "user":
            // ✅ Complaints created by this user
            query = query.Where(c => c.InitiatorId == userId);
            break;

        default:
            return BadRequest("Invalid role");
    }

    var complaints = await query.ToListAsync();
    return Ok(complaints);
}





        [HttpGet("category-counts")]
        public async Task<IActionResult> GetComplaintCountsByCategory()
        {
            var result = await _context.ComplaintCategories
                .GroupJoin(
                    _context.Complaints,
                    category => category.Id,
                    complaint => complaint.CategoryId,
                    (category, complaints) => new
                    {
                        Name = category.Name,
                        Count = complaints.Count()
                    }
                )
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("GetComplaintDetails/{id}")]
        public async Task<IActionResult> GetComplaintDetails(int id)
        {
            var complaint = await _context.Complaints
                .Include(c => c.Attachments)
                .Include(c => c.Status)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
                return NotFound(new { message = "Complaint not found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = new
            {
                complaint.Id,
                complaint.ComplaintCode,
                complaint.Title,
                complaint.Description,
                complaint.CategoryId,
                CategoryName = complaint.Category?.Name,
                Status = complaint.Status?.Name,
                complaint.Priority,
                complaint.Latitude,
                complaint.Longitude,
                complaint.CreatedAt,

                // ✅ Map attachments exactly how Flutter needs them
                Attachments = complaint.Attachments.Select(a => new
                {
                    fileName = a.FileName,
                    fileType = a.FileType?.ToLower(), // Flutter checks "image"/"video"/"audio"
                    url = $"{baseUrl}/Uploads/ComplaintAttachments/{a.FilePath}"
                }).ToList()
            };

            return Ok(result);
        }


        [HttpGet("pending-execution")]
        public async Task<IActionResult> GetPendingExecutionComplaints()
        {
            var complaints = await _context.Complaints
                                           .Include(c => c.Category)
                                           .Include(c => c.Status)
                                           .Include(c => c.Initiator)
                                           .Include(c => c.AssignedTo)
                                           .Where(c => c.StatusId == 6) // Pending Execution
                                           .ToListAsync();

            return Ok(complaints);
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> GetAttachments(int id)
        {
            Console.WriteLine($"📡 GetAttachments called with ComplaintId={id}");

            var complaint = await _context.Complaints
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (complaint == null)
            {
                Console.WriteLine($"❌ Complaint {id} not found");
                return NotFound(new { message = $"Complaint {id} not found" });
            }

            var attachments = complaint.Attachments.Select(a => new
            {
                a.Id,
                FilePath = $"{Request.Scheme}://{Request.Host}/{a.FilePath}",
                FileType = string.IsNullOrEmpty(a.FileType) || a.FileType == "image"
                ? GetFileType(a.FileName)   // fallback for old data
                : a.FileType,

                a.UploadedAt,
                isInitialSubmission = a.IsInitialSubmission
            }).ToList();

            Console.WriteLine($"✅ Returning {attachments.Count} attachments for Complaint {id}");

            return Ok(attachments);
        }

        // GET: api/ComplaintAttachments/ByHistory/5
        [HttpGet("ByHistory/{historyId}")]
        public async Task<IActionResult> GetAttachmentsByHistory(int historyId)
        {
            var attachments = await _context.ComplaintAttachments
                .Where(a => a.HistoryId == historyId)
                .Select(a => new
                {
                    a.Id,
                    a.FileName,
                    a.FilePath,
                    a.FileType,
                    a.UploadedAt,
                    a.UploadedById,
                    isInitialSubmission = a.IsInitialSubmission
                })
                .ToListAsync();

            // Always return 200 with empty array if no attachments
            return Ok(attachments);
        }

        //save tokens for push notifications
        [HttpPost("save-token")]
        public async Task<IActionResult> SaveToken([FromBody] TokenDto dto)
        {
            if (string.IsNullOrEmpty(dto.Token))
                return BadRequest("Token is required");

            var entity = new UserDeviceToken
            {
                UserId = dto.UserId,
                DeviceToken = dto.Token
            };

            _context.UserDeviceTokens.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Token saved successfully" });
        }




        // POST: api/Complaints
        [HttpPost]
        public async Task<ActionResult<ComplaintResponseDto>> CreateComplaint([FromForm] ComplaintCreateDto dto)
        {
            // 1️⃣ Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Title is required." });
            if (string.IsNullOrWhiteSpace(dto.ComplaintCode))
                return BadRequest(new { error = "ComplaintCode is required." });

            // ensure it's unique
            var duplicate = await _context.Complaints
                .AnyAsync(c => c.ComplaintCode == dto.ComplaintCode);
            if (duplicate)
                return BadRequest(new { error = $"ComplaintCode {dto.ComplaintCode} already exists." });


            var categoryExists = await _context.ComplaintCategories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest(new { error = $"CategoryId {dto.CategoryId} does not exist." });

            var initiatorExists = await _context.Users.AnyAsync(u => u.Id == dto.InitiatorId);
            if (!initiatorExists)
                return BadRequest(new { error = $"User with Id {dto.InitiatorId} does not exist." });

            // Optional foreign keys
            if (dto.DefaultReceiverId.HasValue)
            {
                var receiverExists = await _context.Users.AnyAsync(u => u.Id == dto.DefaultReceiverId.Value);
                if (!receiverExists)
                    return BadRequest(new { error = $"DefaultReceiverId {dto.DefaultReceiverId} does not exist." });
            }

            if (dto.AssignedToId.HasValue)
            {
                var assignedExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedToId.Value);
                if (!assignedExists)
                    return BadRequest(new { error = $"AssignedToId {dto.AssignedToId} does not exist." });
            }

            // Fetch "Open" status
            var status = await _context.ComplaintStatuses.FirstOrDefaultAsync(s => s.Name == "Open");
            if (status == null)
                return BadRequest(new { error = "Status 'Open' not found in the database." });

            // Generate next complaint number
            //int nextComplaintNo = 100000; // Default for first complaint

            //var lastComplaint = await _context.Complaints
            //    .OrderByDescending(c => c.ComplaintCode)
            //    .FirstOrDefaultAsync();

            //if (lastComplaint != null)
            //    nextComplaintNo = lastComplaint.ComplaintCode + 1;

            var complaint = new Complaint
            {    
               // ComplaintNo = nextComplaintNo,
                ComplaintCode = dto.ComplaintCode,
                Title = dto.Title,
                Description = dto.Description,
                InitiatorId = dto.InitiatorId,
                CategoryId = dto.CategoryId,
                DefaultReceiverId = dto.DefaultReceiverId,
                AssignedToId = dto.AssignedToId,
                Priority = dto.Priority,
                Latitude = dto.Latitude.HasValue ? (decimal?)dto.Latitude.Value : null,
                Longitude = dto.Longitude.HasValue ? (decimal?)dto.Longitude.Value : null,
                StatusId = status.Id,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Save complaint
                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();

                // Add initial history
                _context.ComplaintHistories.Add(new ComplaintHistory
                {
                    ComplaintId = complaint.Id,
                    OldStatusId = null,
                    NewStatusId = complaint.StatusId,
                    AssignedById = null,
                    AssignedToId = complaint.AssignedToId,
                    Remarks = "Complaint submitted",
                    ActionDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // Handle attachments
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    foreach (var file in dto.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            // Save inside wwwroot so it is publicly accessible
                            var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                            var folder = Path.Combine(wwwRoot, "Uploads", "ComplaintAttachments");
                            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                            var physicalPath = Path.Combine(folder, uniqueFileName);

                            using (var stream = new FileStream(physicalPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                           
                            // ✅ Instead of full E:\ path, save relative
                            var relativePath = Path.Combine("Uploads", "ComplaintAttachments", uniqueFileName)
                                                .Replace("\\", "/");

                            var attachment = new ComplaintAttachment
                            {
                                ComplaintId = complaint.Id,
                                FileName = file.FileName,
                                FilePath = relativePath,   // ✅ save relative path only
                                FileType = GetFileType(file.FileName), // ✅ Detect type by extension
                                IsInitialSubmission = true,
                                UploadedById = complaint.InitiatorId,
                           
                            };


                            _context.ComplaintAttachments.Add(attachment);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                // 🔔 Send notifications here (using Firebase instead of OneSignal)

                // Notify initiator
                var initiatorToken = await _context.UserDeviceTokens
                    .Where(u => u.UserId == complaint.InitiatorId)
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => u.DeviceToken)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(initiatorToken))
                {
                    await _firebaseService.SendNotificationAsync(
     initiatorToken,
     "Complaint Submitted",
     $"Your complaint '{complaint.Title}' has been submitted successfully."
 );
                    ;
                }

                // Notify assigned user (if complaint assigned)
                if (complaint.AssignedToId.HasValue)
                {
                    var assignedToken = await _context.UserDeviceTokens
                        .Where(u => u.UserId == complaint.AssignedToId.Value)
                        .OrderByDescending(u => u.CreatedAt)
                        .Select(u => u.DeviceToken)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(assignedToken))
                    {
                         await _firebaseService.SendNotificationAsync(
                            assignedToken,
                            "New Complaint Assigned",
                            $"A new complaint '{complaint.Title}' has been assigned to you."
                        );
                    }
                }

                // Notify admins
                var adminTokens = await (from u in _context.Users
                                         join t in _context.UserDeviceTokens on u.Id equals t.UserId
                                         where u.UserType == "admin"
                                         select t.DeviceToken)
                                        .ToListAsync();

                foreach (var adminToken in adminTokens)
                {
                    await _firebaseService.SendNotificationAsync(
                        adminToken,
                        "New Complaint Submitted",
                        $"New complaint submitted by user {complaint.InitiatorId}: {complaint.Title}"
                    );
                }




                // ✅ Return clean DTO
                var response = new ComplaintResponseDto
                {
                    Id = complaint.Id,
                   // ComplaintNo = complaint.ComplaintNo.ToString(),
                    ComplaintNo = complaint.ComplaintCode,
                    ComplaintCode = complaint.ComplaintCode,
                    Title = complaint.Title,
                    Description = complaint.Description,
                    CategoryId = complaint.CategoryId,
                    InitiatorId = complaint.InitiatorId,
                    Status = status.Name,
                    Latitude = complaint.Latitude,
                    Longitude = complaint.Longitude
                };

                return CreatedAtAction(nameof(GetComplaint), new { id = complaint.Id }, response);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;

               
                Console.WriteLine(" Complaint Create Error: " + innerMessage);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new
                {
                    error = "Unexpected server error.",
                    details = innerMessage,   // ⚠️ shows real DB error
                    stackTrace = ex.StackTrace
                });
            }


        }
     
        // PUT: api/Complaints/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComplaint(int id, Complaint complaint)
        {
            if (id != complaint.Id)
                return BadRequest();

            _context.Entry(complaint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComplaintExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }
        // PUT: api/Complaints/UpdateStatus/5
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateComplaintStatus(int id, [FromBody] ComplaintStatusUpdateDto dto)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
                return NotFound(new { error = "Complaint not found." });

            var oldStatusId = complaint.StatusId;

            var statusExists = await _context.ComplaintStatuses.AnyAsync(s => s.Id == dto.StatusId);
            if (!statusExists)
                return BadRequest(new { error = "Invalid status ID." });

            complaint.StatusId = dto.StatusId;
            complaint.UpdatedAt = DateTime.UtcNow;

            if (dto.AssignedToId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(dto.AssignedToId.Value);
                if (assignedUser == null)
                    return BadRequest(new { error = "Assigned worker not found." });

                complaint.AssignedToId = dto.AssignedToId.Value;
            }

            if (dto.UpdatedById.HasValue)
            {
                var updatedByUser = await _context.Users.FindAsync(dto.UpdatedById.Value);
                if (updatedByUser == null)
                    return BadRequest(new { error = "Admin/Supervisor not found." });

                complaint.UpdatedById = dto.UpdatedById.Value;
            }

            try
            {
                _context.ComplaintHistories.Add(new ComplaintHistory
                {
                    ComplaintId = complaint.Id,
                    OldStatusId = oldStatusId,
                    NewStatusId = dto.StatusId,
                    AssignedById = dto.UpdatedById,
                    AssignedToId = dto.AssignedToId,
                    Remarks = dto.Remarks ?? "Nil",
                    ActionDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                var updatedComplaint = await _context.Complaints
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.ComplaintCode,
                        c.Title,
                        c.Description,
                        c.CategoryId,
                        CategoryName = c.Category.Name,
                        c.InitiatorId,
                        c.StatusId,
                        Status = _context.ComplaintStatuses
                                     .Where(s => s.Id == c.StatusId)
                                     .Select(s => s.Name).FirstOrDefault(),
                        c.AssignedToId,
                        c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return Ok(updatedComplaint);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Failed to update status.", details = ex.Message });
            }
        }
        // PUT: api/Complaints/ExecutorUpdate/5
        [HttpPut("ExecutorUpdate/{id}")]
        public async Task<IActionResult> ExecutorUpdateComplaint(int id, [FromForm] ExecutorComplaintUpdateDto dto)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
                return NotFound(new { error = "Complaint not found." });

            var oldStatusId = complaint.StatusId;

            // Validate status exists
            var statusExists = await _context.ComplaintStatuses.AnyAsync(s => s.Id == dto.StatusId);
            if (!statusExists)
                return BadRequest(new { error = "Invalid status ID." });

            // Update status
            complaint.StatusId = dto.StatusId;
            complaint.UpdatedAt = DateTime.UtcNow;

            // ✅ Safe assignments using .HasValue
            if (dto.UpdatedById.HasValue)
                complaint.UpdatedById = dto.UpdatedById.Value;

            if (dto.AssignedToId.HasValue)
                complaint.AssignedToId = dto.AssignedToId.Value;

            try
            {

                // Create history row first
                var history = new ComplaintHistory
                {
                    ComplaintId = complaint.Id,
                    OldStatusId = oldStatusId,
                    NewStatusId = complaint.StatusId, // even if status didn't change
                    AssignedById = dto.UpdatedById,
                    AssignedToId = dto.AssignedToId,
                    Remarks = string.IsNullOrWhiteSpace(dto.Remarks) && dto.Attachments?.Count > 0
                              ? "Attachments added"
                              : dto.Remarks ?? "Nil",
                    ActionDate = DateTime.UtcNow,
                };

                _context.ComplaintHistories.Add(history);
                await _context.SaveChangesAsync(); // get history.Id

                // Handle attachments - create rows even if complaint had none before
                if (dto.Attachments != null && dto.Attachments.Count > 0)
                {
                    var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var folder = Path.Combine(wwwRoot, "Uploads", "ComplaintAttachments");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    foreach (var file in dto.Attachments)
                    {
                        if (file.Length <= 0) continue;

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var physicalPath = Path.Combine(folder, uniqueFileName);

                        using (var stream = new FileStream(physicalPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var relativePath = Path.Combine("Uploads", "ComplaintAttachments", uniqueFileName)
                                                .Replace("\\", "/");

                        // ✅ Create attachment row and link to history
                        _context.ComplaintAttachments.Add(new ComplaintAttachment
                        {
                            ComplaintId = complaint.Id,
                            FileName = file.FileName,
                            FilePath = relativePath,
                            FileType = Path.GetExtension(file.FileName).ToLower(),
                            UploadedAt = DateTime.UtcNow,
                            UploadedById = dto.UpdatedById ?? complaint.InitiatorId,
                            HistoryId = history.Id
                        });
                    }
                }

                await _context.SaveChangesAsync();



                // Return updated complaint with attachments
                var updatedComplaint = await _context.Complaints
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.ComplaintCode,
                        c.Title,
                        c.Description,
                        c.CategoryId,
                        CategoryName = c.Category.Name,
                        c.InitiatorId,
                        c.StatusId,
                        Status = _context.ComplaintStatuses
                                     .Where(s => s.Id == c.StatusId)
                                     .Select(s => s.Name).FirstOrDefault(),
                        c.AssignedToId,
                        c.UpdatedAt,
                        Attachments = c.Attachments
                                        .Select(a => new { a.FileName, a.FilePath, a.FileType })
                                        .ToList()
                    })
                    .FirstOrDefaultAsync();

                return Ok(updatedComplaint);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { error = "Failed to update complaint.", details = ex.Message });
            }
        }



        // DELETE: api/Complaints/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComplaint(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null)
                return NotFound();

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComplaintExists(int id)
        {
            return _context.Complaints.Any(e => e.Id == id);
        }
       

        // 👇 Add this here
        private string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    return "image";

                case ".mp4":
                case ".mov":
                case ".avi":
                case ".mkv":
                    return "video";

                case ".mp3":
                case ".aac":
                case ".wav":
                case ".ogg":
                    return "audio";

                default:
                    return "unknown";
            }
        }

    }



}

