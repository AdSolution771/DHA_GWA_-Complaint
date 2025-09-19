using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebUI.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [JsonPropertyName("complaintNo")]   // match Flutter key
        public string ComplaintCode { get; set; }  // will hold the Flutter-generated ID

        [Required, MaxLength(2000)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public String? CategoryName { get; set; }
        public ComplaintCategory Category { get; set; }

        public int InitiatorId { get; set; }
        public User Initiator { get; set; }

        public int? DefaultReceiverId { get; set; }
        public User? DefaultReceiver { get; set; }

        public int? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        [MaxLength(500)]
        public string? Priority { get; set; }

        public int StatusId { get; set; }
        public ComplaintStatus Status { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? WorkFlow { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }

        public ICollection<ComplaintHistory> Histories { get; set; } = new List<ComplaintHistory>();

        // ✅ Attachments collection ready for EF Core
        public virtual ICollection<ComplaintAttachment> Attachments { get; set; } = new List<ComplaintAttachment>();

        public ICollection<ComplaintFeedback> Feedbacks { get; set; } = new List<ComplaintFeedback>();
    }
}
