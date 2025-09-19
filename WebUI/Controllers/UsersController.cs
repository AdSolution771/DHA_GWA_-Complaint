using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebUI.Models;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public UsersController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // PUT: api/Complaints/AssignWorker/{workerId}/{complaintId}
    [HttpPut("AssignWorker/{workerId}/{complaintId}")]
    public async Task<IActionResult> AssignWorker(int workerId, int complaintId)
    {
        // Find the complaint
        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint == null)
        {
            return NotFound(new { message = "Complaint not found" });
        }

        // Check if worker exists and is of type 'worker'
        var worker = await _context.Users.FirstOrDefaultAsync(u => u.Id == workerId && u.UserType.ToLower() == "worker");
        if (worker == null)
        {
            return BadRequest(new { message = "Worker not found or invalid user type" });
        }

        // Assign worker
        complaint.AssignedToId = workerId;

        // Optional: You can add a log/history entry here if needed
        // e.g., ComplaintHistory table: assignedByAdminId, assignedToId, date

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Complaint assigned to {worker.FullName}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to assign worker", error = ex.Message });
        }
    }

    // GET: api/Users/Workers
    [HttpGet("Workers")]
    public async Task<ActionResult<IEnumerable<User>>> GetWorkers()
    {
        try
        {
            var workers = await _context.Users
                .Where(u => u.UserType.ToLower() == "worker")
                .ToListAsync();

            return Ok(workers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{userId}/complaints")]
    public async Task<IActionResult> GetUserComplaints(int userId)
    {
        var complaints = await _context.Complaints
            .Include(c => c.Status)
            .Include(c => c.Category)
            .Where(c => c.InitiatorId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(complaints);
    }
    // GET: api/Users/Supervisors
    [HttpGet("Supervisors")]
    public async Task<ActionResult<IEnumerable<User>>> GetSupervisors()
    {
        try
        {
            var supervisors = await _context.Users
                .Where(u => u.UserType.ToLower() == "supervisor") // or the exact type in your DB
                .ToListAsync();

            return Ok(supervisors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/Users/Executors
    [HttpGet("Executors")]
    public async Task<ActionResult<IEnumerable<User>>> GetExecutors()
    {
        try
        {
            var executors = await _context.Users
                .Where(u => u.UserType.ToLower() == "executor") // filter for Executors
                .ToListAsync();

            return Ok(executors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }



    // GET: api/Users/approved-users
    [HttpGet("approved-users")]
    public async Task<ActionResult<IEnumerable<User>>> GetApprovedUsers()
    {
        var approvedUsers = await _context.Users
            .Where(u => u.IsApproved && u.UserType == "Resident")  // Only approved users
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Address,
                u.Phone,
                u.CNIC
            })
            .ToListAsync();

        if (approvedUsers == null || !approvedUsers.Any())
        {
            return NotFound(new { message = "No approved users found" });
        }

        return Ok(approvedUsers);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, User user)
    {
        if (id != user.Id) return BadRequest();
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("User not found");

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            return BadRequest(new { message = "Old password is incorrect" });

        // Hash and update new password
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password updated successfully" });
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { message = "Validation failed", errors });
        }

        // Normalize email
        if (!string.IsNullOrEmpty(user.Email))
        {
            user.Email = user.Email.Trim().ToLower();
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "Email already registered." });
        }

        if (!string.IsNullOrEmpty(user.Phone))
        {
            if (await _context.Users.AnyAsync(u => u.Phone == user.Phone))
                return BadRequest(new { message = "Phone number already registered." });
        }

        // Hash password
        if (string.IsNullOrEmpty(user.Password))
            return BadRequest(new { message = "Password is required." });

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        // Force system defaults
        user.Status = "pending";
        user.IsApproved = false;
        user.CreatedAt = DateTime.UtcNow;
        user.UserType ??= "Resident";
        user.Type = 0;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully!", userId = user.Id });
    }


    [HttpGet("pending-requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var pendingUsers = await _context.Users
            .Where(u => u.Status == "pending" && u.IsApproved == false)
            .Select(u => new {
                u.Id,
                u.FullName,
                u.Email,
                u.Address,
                u.Phone,
                u.CNIC,
                u.UserType,
                u.Status,
                u.IsApproved,
                u.PlotNumber
            })
            .ToListAsync();

        return Ok(pendingUsers);
    }

    [HttpPost("update-approval")]
    public async Task<IActionResult> UpdateApproval([FromBody] ApprovalRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound("User not found");

        user.IsApproved = request.IsApproved;
        user.Status = request.IsApproved ? "approved" : "disapproved";

        await _context.SaveChangesAsync();

        return Ok(new { message = $"User {user.FullName} has been {(request.IsApproved ? "approved" : "disapproved")}" });
    }

    public class ApprovalRequest
    {
        public int UserId { get; set; }
        public bool IsApproved { get; set; }
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find user by email or phone
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email || u.Phone == request.Email);

        if (user == null)
            return Unauthorized(new { message = "User not found." });

        // Verify password
        if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return Unauthorized(new { message = "Invalid password." });

        // Login successful
        return Ok(new
        {
            message = "Login successful!",
            userId = user.Id,
            fullName = user.FullName,
            email = user.Email,
            phone = user.Phone,
            address = user.Address,
            cnic = user.CNIC,
            userType = user.UserType,
            status = user.Status,
            isApproved = user.IsApproved,
            type = user.Type,
            plotNumber = user.PlotNumber,
            createdAt = user.CreatedAt
        });
    }

    // Login request class
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; } = null!;  // can be email or phone
        [Required]
        public string Password { get; set; } = null!;
    }
}

