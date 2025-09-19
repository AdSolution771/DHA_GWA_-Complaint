
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class ComplaintFeedback
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public int FeedbackById { get; set; }
        public User FeedbackBy { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Range(1, 5)]
        public int BehaviorRating { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
