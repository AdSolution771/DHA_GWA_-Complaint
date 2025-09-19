using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace WebUI.DTOs
{
    public class ComplaintCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int InitiatorId { get; set; }
        public int? DefaultReceiverId { get; set; }
        public int? AssignedToId { get; set; }
        public string? Priority { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string ComplaintCode { get; set; }   // must match Flutter
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}
