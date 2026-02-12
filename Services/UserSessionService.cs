using Blazor.Models;

namespace Blazor.Services;

/// <summary>
/// Manages user session state across the application
/// </summary>
public class UserSessionService
{
    private User? _currentUser;
    private readonly List<User> _users = new();
    private int _nextUserId = 1;

    public event Action? OnUserSessionChanged;

    public User? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser != null;

    public UserSessionService()
    {
        // Initialize with sample users for testing
        InitializeSampleUsers();
    }

    private void InitializeSampleUsers()
    {
        _users.Add(new User
        {
            Id = _nextUserId++,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "555-0101",
            Organization = "Tech Corp",
            RegisteredAt = DateTime.Now.AddDays(-30),
            IsActive = true
        });

        _users.Add(new User
        {
            Id = _nextUserId++,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Phone = "555-0102",
            Organization = "Design Studio",
            RegisteredAt = DateTime.Now.AddDays(-15),
            IsActive = true
        });
    }

    public User? RegisterUser(string firstName, string lastName, string email, string phone, string? organization = null)
    {
        // Validate email uniqueness
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return null; // Email already exists
        }

        var user = new User
        {
            Id = _nextUserId++,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Organization = organization,
            RegisteredAt = DateTime.Now,
            IsActive = true
        };

        _users.Add(user);
        return user;
    }

    public bool Login(string email)
    {
        var user = _users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        
        if (user != null)
        {
            _currentUser = user;
            OnUserSessionChanged?.Invoke();
            return true;
        }
        
        return false;
    }

    public void Logout()
    {
        _currentUser = null;
        OnUserSessionChanged?.Invoke();
    }

    public User? GetUserById(int userId)
    {
        return _users.FirstOrDefault(u => u.Id == userId);
    }

    public User? GetUserByEmail(string email)
    {
        return _users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public List<User> GetAllUsers()
    {
        return _users.Where(u => u.IsActive).ToList();
    }

    public bool UpdateUser(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null) return false;

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Organization = user.Organization;

        if (_currentUser?.Id == user.Id)
        {
            _currentUser = existingUser;
            OnUserSessionChanged?.Invoke();
        }

        return true;
    }

    public void AddEventToUser(int userId, int eventId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null && !user.RegisteredEventIds.Contains(eventId))
        {
            user.RegisteredEventIds.Add(eventId);
            if (_currentUser?.Id == userId)
            {
                OnUserSessionChanged?.Invoke();
            }
        }
    }

    public List<int> GetUserEventIds(int userId)
    {
        return _users.FirstOrDefault(u => u.Id == userId)?.RegisteredEventIds ?? new List<int>();
    }
}
