using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebUI.DTOs
{
    public class Report
    {
        public int Id { get; set; }

        
        [JsonPropertyName("complaintNo")]   // Flutter expects complaintNo
        public string ComplaintCode { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // ✅ User who initiated the complaint
        public UserDto Initiator { get; set; } = new UserDto();

        // ✅ Assigned user (nullable)
        public UserDto? AssignedTo { get; set; }

        [MaxLength(500)]
        public string Status { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        // ✅ Complaint history (only required fields)
        public ICollection<ReportHistoryDto> Histories { get; set; } = new List<ReportHistoryDto>();

       

        // ✅ User feedback
        public ICollection<ReportFeedbackDto> Feedbacks { get; set; } = new List<ReportFeedbackDto>();
    }

    // 🔹 Lightweight related DTOs
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class ReportHistoryDto
    {
        public int? AssignedById { get; set; }
        public int? AssignedToId { get; set; }
        public string? Remarks { get; set; }
    }

    //public class ReportAttachmentDto
    //{
    //    public int Id { get; set; }
    //    public string FileUrl { get; set; } = string.Empty;
    //    public string FileType { get; set; } = string.Empty;  // image, video, audio
    //}

    public class ReportFeedbackDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }  // e.g., 1–5 stars
        public string Comments { get; set; } = string.Empty;
    }
}
