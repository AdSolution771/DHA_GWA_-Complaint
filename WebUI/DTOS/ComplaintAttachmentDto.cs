namespace WebUI.DTOs
{
    public class ComplaintAttachmentDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; }   // or URL if stored remotely
        public string FileType { get; set; }   // e.g. "image", "video", "audio"
        public DateTime UploadedAt { get; set; }
        public bool IsInitialSubmission { get; set; } = false;
    }
}
