using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ExecutorComplaintUpdateDto
{
    [Required]
    public int StatusId { get; set; } // Required status ID

    public string? Remarks { get; set; } // Optional remarks

    public int? UpdatedById { get; set; } // Optional: admin/executor updating

    public int? AssignedToId { get; set; } // Optional: reassign to another user

    public List<IFormFile>? Attachments { get; set; } // Optional files (images/videos/audio)
}
