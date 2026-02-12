using Microsoft.AspNetCore.Components;

namespace Blazor.Services;

/// <summary>
/// Navigation helper service providing utilities for routing and navigation
/// </summary>
public class NavigationHelper
{
    private readonly NavigationManager _navigationManager;
    private readonly EventService _eventService;

    public NavigationHelper(NavigationManager navigationManager, EventService eventService)
    {
        _navigationManager = navigationManager;
        _eventService = eventService;
    }

    // Route Constants
    public const string HomeRoute = "/";
    public const string EventsRoute = "/events";
    public const string EventDetailsRoute = "/events/{0}";
    public const string EventRegistrationRoute = "/events/{0}/register";

    // Navigation Methods
    public void NavigateToHome() => _navigationManager.NavigateTo(HomeRoute);
    
    public void NavigateToEvents() => _navigationManager.NavigateTo(EventsRoute);
    
    public void NavigateToEventDetails(int eventId) 
        => _navigationManager.NavigateTo(string.Format(EventDetailsRoute, eventId));
    
    public void NavigateToEventRegistration(int eventId) 
        => _navigationManager.NavigateTo(string.Format(EventRegistrationRoute, eventId));

    // Query Parameter Methods
    public void NavigateToEventsWithCategory(string category)
    {
        var uri = _navigationManager.GetUriWithQueryParameter("category", category);
        _navigationManager.NavigateTo(uri);
    }

    public void NavigateToEventsWithSearch(string searchTerm)
    {
        var uri = _navigationManager.GetUriWithQueryParameter("search", searchTerm);
        _navigationManager.NavigateTo(uri);
    }

    // Navigation with History
    public void NavigateBack()
    {
        _navigationManager.NavigateTo("javascript:history.back()", forceLoad: false);
    }

    public void NavigateToWithReturn(string url, string returnUrl)
    {
        var uri = _navigationManager.GetUriWithQueryParameter("returnUrl", returnUrl);
        _navigationManager.NavigateTo(uri);
    }

    // URL Helpers
    public string GetCurrentUrl() => _navigationManager.Uri;
    
    public string GetBaseUrl() => _navigationManager.BaseUri;
    
    public bool IsCurrentRoute(string route)
    {
        var currentPath = new Uri(_navigationManager.Uri).AbsolutePath;
        return currentPath.Equals(route, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsRouteActive(string route)
    {
        var currentPath = new Uri(_navigationManager.Uri).AbsolutePath;
        return currentPath.StartsWith(route, StringComparison.OrdinalIgnoreCase);
    }

    // Route Validation
    public bool IsValidEventId(string? id)
    {
        return int.TryParse(id, out var eventId) && eventId > 0;
    }

    public bool IsValidEventId(int id)
    {
        if (id <= 0) return false;
        return _eventService.GetEventById(id) != null;
    }

    // Breadcrumb Generation
    public List<BreadcrumbItem> GetBreadcrumbs(string currentPath)
    {
        var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Text = "Home", Url = HomeRoute, IsActive = false }
        };

        if (currentPath.StartsWith("/events"))
        {
            breadcrumbs.Add(new BreadcrumbItem { Text = "Events", Url = EventsRoute, IsActive = currentPath == EventsRoute });

            if (currentPath.Contains("/register"))
            {
                var eventId = ExtractEventIdFromPath(currentPath);
                if (eventId.HasValue)
                {
                    breadcrumbs.Add(new BreadcrumbItem 
                    { 
                        Text = "Event Details", 
                        Url = string.Format(EventDetailsRoute, eventId.Value), 
                        IsActive = false 
                    });
                    breadcrumbs.Add(new BreadcrumbItem { Text = "Register", Url = currentPath, IsActive = true });
                }
            }
            else if (currentPath != EventsRoute)
            {
                breadcrumbs.Add(new BreadcrumbItem { Text = "Event Details", Url = currentPath, IsActive = true });
            }
        }

        return breadcrumbs;
    }

    private int? ExtractEventIdFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2 && int.TryParse(segments[1], out var id))
        {
            return id;
        }
        return null;
    }
}

/// <summary>
/// Represents a breadcrumb navigation item
/// </summary>
public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
