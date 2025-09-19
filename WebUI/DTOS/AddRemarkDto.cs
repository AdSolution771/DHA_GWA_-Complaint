public class AddRemarkDto
{
    public int ComplaintId { get; set; }
    public int? OldStatusId { get; set; }
    public int NewStatusId { get; set; }
    public int? AssignedToId { get; set; }
    public string? Remarks { get; set; }
}
