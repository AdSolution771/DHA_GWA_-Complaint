using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class ComplaintAttachment
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }
        public virtual Complaint Complaint { get; set; }

        public int? HistoryId { get; set; }
        public virtual ComplaintHistory? History { get; set; }

        [MaxLength(2000)]
        public string? FileType { get; set; }

        [MaxLength(5000)]
        public string FilePath { get; set; }

        [MaxLength(2555)]
        public string FileName { get; set; }

        public int UploadedById { get; set; }
        public virtual User UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // NEW FLAG
        public bool IsInitialSubmission { get; set; } = false;
    }
}
