namespace Blazor.Models;

/// <summary>
/// Represents an attendance record for a user at an event
/// </summary>
public class AttendanceRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public bool IsCheckedIn { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public Event? Event { get; set; }
}
