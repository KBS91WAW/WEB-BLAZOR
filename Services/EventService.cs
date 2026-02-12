using Blazor.Models;

namespace Blazor.Services;

public class EventService
{
    private readonly List<Event> _events;

    public EventService()
    {
        // Initialize with sample data
        _events = new List<Event>
        {
            new Event
            {
                Id = 1,
                Name = "Tech Conference 2026",
                Date = new DateTime(2026, 3, 15, 9, 0, 0),
                Location = "Convention Center, Downtown",
                Description = "Join industry leaders for a day of networking and learning about the latest in technology.",
                Capacity = 500,
                RegisteredAttendees = 234,
                Category = "Technology",
                ImageUrl = "/images/tech-conference.jpg"
            },
            new Event
            {
                Id = 2,
                Name = "Annual Gala Dinner",
                Date = new DateTime(2026, 4, 20, 18, 30, 0),
                Location = "Grand Hotel Ballroom",
                Description = "An elegant evening of fine dining, entertainment, and charity fundraising.",
                Capacity = 300,
                RegisteredAttendees = 287,
                Category = "Social",
                ImageUrl = "/images/gala-dinner.jpg"
            },
            new Event
            {
                Id = 3,
                Name = "Product Launch Event",
                Date = new DateTime(2026, 5, 10, 14, 0, 0),
                Location = "Innovation Hub",
                Description = "Be the first to experience our groundbreaking new product line.",
                Capacity = 150,
                RegisteredAttendees = 98,
                Category = "Corporate",
                ImageUrl = "/images/product-launch.jpg"
            },
            new Event
            {
                Id = 4,
                Name = "Summer Music Festival",
                Date = new DateTime(2026, 6, 25, 12, 0, 0),
                Location = "City Park",
                Description = "A full day of live music featuring local and international artists.",
                Capacity = 1000,
                RegisteredAttendees = 756,
                Category = "Entertainment",
                ImageUrl = "/images/music-festival.jpg"
            },
            new Event
            {
                Id = 5,
                Name = "Business Networking Breakfast",
                Date = new DateTime(2026, 3, 5, 7, 30, 0),
                Location = "Downtown Business Center",
                Description = "Connect with local business leaders over breakfast and discussion.",
                Capacity = 80,
                RegisteredAttendees = 62,
                Category = "Corporate",
                ImageUrl = "/images/networking-breakfast.jpg"
            },
            new Event
            {
                Id = 6,
                Name = "Charity Fun Run",
                Date = new DateTime(2026, 7, 15, 8, 0, 0),
                Location = "Riverside Trail",
                Description = "5K and 10K runs to support local charities. All fitness levels welcome.",
                Capacity = 400,
                RegisteredAttendees = 312,
                Category = "Social",
                ImageUrl = "/images/fun-run.jpg"
            }
        };
    }

    public List<Event> GetAllEvents()
    {
        return _events.OrderBy(e => e.Date).ToList();
    }

    public Event? GetEventById(int id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public List<Event> GetEventsByCategory(string category)
    {
        return _events
            .Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Date)
            .ToList();
    }

    public bool RegisterForEvent(int eventId)
    {
        var eventItem = GetEventById(eventId);
        if (eventItem != null && eventItem.RegisteredAttendees < eventItem.Capacity)
        {
            eventItem.RegisteredAttendees++;
            return true;
        }
        return false;
    }

    public List<string> GetCategories()
    {
        return _events
            .Select(e => e.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}
