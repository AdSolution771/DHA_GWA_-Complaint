using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;  

namespace WebUI.Models
{
    public class ComplaintHistory
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }
        public Complaint Complaint { get; set; }

        public int? OldStatusId { get; set; }
        public ComplaintStatus? OldStatus { get; set; }

        public int NewStatusId { get; set; }
        public ComplaintStatus NewStatus { get; set; }

        public int? AssignedById { get; set; }
        public User? AssignedBy { get; set; }

        public int? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        public ICollection<ComplaintAttachment> Attachments { get; set; } = new List<ComplaintAttachment>();
    }
}
