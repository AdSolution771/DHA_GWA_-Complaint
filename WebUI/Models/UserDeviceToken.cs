public class UserDeviceToken
{
    public int Id { get; set; }
    public int UserId { get; set; }   // foreign key to your Users table
    public string DeviceToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

}
