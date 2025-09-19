namespace WebUI.DTOs
{
    public class ComplaintStatusUpdateDto
    {
        public int Id { get; set; }             // Complaint Id
        public int StatusId { get; set; }       // New status
        public int CategoryId { get; set; }     // Must send existing category
        public int InitiatorId { get; set; }    // Must send existing initiator
        public string ComplaintCode { get; set; } // Existing complaint code
        public string Title { get; set; }       // Existing title

        public int? UpdatedById { get; set; }   // Admin ID (optional if auth used)

        // NEW FIELDS
        public string? Remarks { get; set; }     // Optional remarks
        public int? AssignedToId { get; set; }   // Worker ID (optional)
     
    }
}
