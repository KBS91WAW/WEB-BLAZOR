using Blazor.Models;

namespace Blazor.Services;

/// <summary>
/// Manages attendance records and event participation tracking
/// </summary>
public class AttendanceService
{
    private readonly List<AttendanceRecord> _attendanceRecords = new();
    private int _nextAttendanceId = 1;
    private readonly EventService _eventService;
    private readonly UserSessionService _userSessionService;

    public event Action? OnAttendanceChanged;

    public AttendanceService(EventService eventService, UserSessionService userSessionService)
    {
        _eventService = eventService;
        _userSessionService = userSessionService;
        InitializeSampleAttendance();
    }

    private void InitializeSampleAttendance()
    {
        // Sample attendance records
        var users = _userSessionService.GetAllUsers();
        if (users.Count >= 2)
        {
            // User 1 registered for event 1
            RegisterAttendance(users[0].Id, 1, DateTime.Now.AddDays(-5));
            CheckIn(1); // Checked in
            
            // User 1 registered for event 2
            RegisterAttendance(users[0].Id, 2, DateTime.Now.AddDays(-3));
            
            // User 2 registered for event 1
            RegisterAttendance(users[1].Id, 1, DateTime.Now.AddDays(-4));
            CheckIn(3); // Checked in
            
            // User 2 registered for event 3
            RegisterAttendance(users[1].Id, 3, DateTime.Now.AddDays(-2));
        }
    }

    public AttendanceRecord? RegisterAttendance(int userId, int eventId, DateTime? registeredAt = null)
    {
        // Check if user is already registered
        if (_attendanceRecords.Any(a => a.UserId == userId && a.EventId == eventId))
        {
            return null; // Already registered
        }

        var user = _userSessionService.GetUserById(userId);
        var eventItem = _eventService.GetEventById(eventId);

        if (user == null || eventItem == null)
        {
            return null; // Invalid user or event
        }

        var record = new AttendanceRecord
        {
            Id = _nextAttendanceId++,
            UserId = userId,
            EventId = eventId,
            RegisteredAt = registeredAt ?? DateTime.Now,
            IsCheckedIn = false,
            User = user,
            Event = eventItem
        };

        _attendanceRecords.Add(record);
        _userSessionService.AddEventToUser(userId, eventId);
        OnAttendanceChanged?.Invoke();

        return record;
    }

    public bool CheckIn(int attendanceId)
    {
        var record = _attendanceRecords.FirstOrDefault(a => a.Id == attendanceId);
        if (record == null || record.IsCheckedIn)
        {
            return false;
        }

        record.IsCheckedIn = true;
        record.CheckedInAt = DateTime.Now;
        OnAttendanceChanged?.Invoke();

        return true;
    }

    public bool CheckInByUserAndEvent(int userId, int eventId)
    {
        var record = _attendanceRecords.FirstOrDefault(a => 
            a.UserId == userId && a.EventId == eventId && !a.IsCheckedIn);
        
        if (record == null)
        {
            return false;
        }

        record.IsCheckedIn = true;
        record.CheckedInAt = DateTime.Now;
        OnAttendanceChanged?.Invoke();

        return true;
    }

    public List<AttendanceRecord> GetAttendanceByEvent(int eventId)
    {
        return _attendanceRecords
            .Where(a => a.EventId == eventId)
            .OrderByDescending(a => a.RegisteredAt)
            .ToList();
    }

    public List<AttendanceRecord> GetAttendanceByUser(int userId)
    {
        return _attendanceRecords
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.RegisteredAt)
            .ToList();
    }

    public AttendanceRecord? GetAttendanceRecord(int userId, int eventId)
    {
        return _attendanceRecords.FirstOrDefault(a => 
            a.UserId == userId && a.EventId == eventId);
    }

    public int GetRegisteredCount(int eventId)
    {
        return _attendanceRecords.Count(a => a.EventId == eventId);
    }

    public int GetCheckedInCount(int eventId)
    {
        return _attendanceRecords.Count(a => a.EventId == eventId && a.IsCheckedIn);
    }

    public double GetAttendanceRate(int eventId)
    {
        var registered = GetRegisteredCount(eventId);
        if (registered == 0) return 0;

        var checkedIn = GetCheckedInCount(eventId);
        return Math.Round((double)checkedIn / registered * 100, 1);
    }

    public List<AttendanceRecord> GetAllAttendance()
    {
        return _attendanceRecords.OrderByDescending(a => a.RegisteredAt).ToList();
    }

    public bool UpdateNotes(int attendanceId, string notes)
    {
        var record = _attendanceRecords.FirstOrDefault(a => a.Id == attendanceId);
        if (record == null) return false;

        record.Notes = notes;
        OnAttendanceChanged?.Invoke();
        return true;
    }

    public bool CancelAttendance(int attendanceId)
    {
        var record = _attendanceRecords.FirstOrDefault(a => a.Id == attendanceId);
        if (record == null) return false;

        _attendanceRecords.Remove(record);
        OnAttendanceChanged?.Invoke();
        return true;
    }

    public AttendanceStatistics GetStatistics()
    {
        var totalRegistrations = _attendanceRecords.Count;
        var totalCheckedIn = _attendanceRecords.Count(a => a.IsCheckedIn);
        var overallRate = totalRegistrations > 0 
            ? Math.Round((double)totalCheckedIn / totalRegistrations * 100, 1) 
            : 0;

        return new AttendanceStatistics
        {
            TotalRegistrations = totalRegistrations,
            TotalCheckedIn = totalCheckedIn,
            OverallAttendanceRate = overallRate,
            UniqueUsers = _attendanceRecords.Select(a => a.UserId).Distinct().Count(),
            UniqueEvents = _attendanceRecords.Select(a => a.EventId).Distinct().Count()
        };
    }
}

public class AttendanceStatistics
{
    public int TotalRegistrations { get; set; }
    public int TotalCheckedIn { get; set; }
    public double OverallAttendanceRate { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueEvents { get; set; }
}
