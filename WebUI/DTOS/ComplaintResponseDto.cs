namespace WebUI.DTOs
{
    public class ComplaintResponseDto
    {
        public int Id { get; set; }
        public string ComplaintNo { get; set; }   // Flutter expects this

        public string ComplaintCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }


        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string CategoryName { get; set; }
        public int InitiatorId { get; set; }

        public UserDto? Initiator { get; set; }
        public string Status { get; set; }        // Always "Open" initially
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // ✅ Add this
        public List<ComplaintAttachmentDto> Attachments { get; set; } = new();
    }
}
