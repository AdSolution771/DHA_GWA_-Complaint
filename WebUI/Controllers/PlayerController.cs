using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Models;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly OneSignalService _oneSignalService;

    public PlayerController(AppDbContext dbContext, OneSignalService oneSignalService)
    {
        _dbContext = dbContext;
        _oneSignalService = oneSignalService;
    }

    //abc123
    [HttpPost("savePlayerId")]
    public async Task<IActionResult> SavePlayerId([FromBody] PlayerDto dto)
    {
        if (string.IsNullOrEmpty(dto.PlayerId))
            return BadRequest("PlayerId is required");

        // Check if the UserPlayerId already exists for this user
        //var existing = await _dbContext.UserPlayerIds
        //    .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

        //if (existing != null)
        //{
        //    // Update the PlayerId if it already exists
        //    existing.PlayerId = dto.PlayerId;
        //    existing.UpdatedAt = DateTime.Now;
        //}
        //else
        //{
        //    // Add new record
        //    var userPlayer = new UserPlayerId
        //    {
        //        UserId = dto.UserId,
        //        PlayerId = dto.PlayerId,
        //        CreatedAt = DateTime.Now,
        //        UpdatedAt = DateTime.Now
        //    };
        //    await _dbContext.UserPlayerIds.AddAsync(userPlayer);
        //}

        //await _dbContext.SaveChangesAsync();

        // Optional: Send welcome notification
        await _oneSignalService.SendNotification(dto.PlayerId, "Welcome to DHA Gujranwala App!");

        return Ok(new { message = "PlayerId saved and notification sent!" });
    }
}

public class PlayerDto
{
    public int UserId { get; set; }
    public string PlayerId { get; set; }
}
