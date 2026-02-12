namespace Blazor.Models;

/// <summary>
/// Represents a registered user in the system
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public DateTime RegisteredAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public List<int> RegisteredEventIds { get; set; } = new();
    
    public string FullName => $"{FirstName} {LastName}";
}
