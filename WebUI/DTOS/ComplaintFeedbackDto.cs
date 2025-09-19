public class ComplaintFeedbackDto
{
    public int ComplaintId { get; set; }
    public int FeedbackById { get; set; }
    public int Rating { get; set; }
    public int BehaviorRating { get; set; }
    public string Comments { get; set; }
    public string ComplaintNumber { get; set; }   // <- new
    public string ComplaintTitle { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
