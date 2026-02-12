# Routing and Navigation Guide - EventEase

## Table of Contents
1. [Route Definitions](#route-definitions)
2. [Navigation Patterns](#navigation-patterns)
3. [Best Practices](#best-practices)
4. [Code Examples](#code-examples)
5. [Troubleshooting](#troubleshooting)

---

## Route Definitions

### Application Routes

| Route | Component | Purpose | Parameters |
|-------|-----------|---------|------------|
| `/` | Home.razor | Landing page with event previews | None |
| `/events` | Events.razor | Browse all events with search/filter | Optional: `?category=`, `?search=` |
| `/events/{id:int}` | EventDetails.razor | View detailed event information | `id`: Event ID (integer) |
| `/events/{id:int}/register` | EventRegistration.razor | Register for an event | `id`: Event ID (integer) |
| `/not-found` | NotFound.razor | 404 error page | None |

### Route Constraints

The application uses ASP.NET Core route constraints to ensure type safety:

```razor
@page "/events/{id:int}"
```

**Supported Constraints:**
- `{id:int}` - Integer parameter
- `{id:long}` - Long integer
- `{id:guid}` - GUID format
- `{slug:alpha}` - Alphabetic characters only

---

## Navigation Patterns

### 1. Using NavigationHelper Service

The application provides a `NavigationHelper` service for consistent navigation:

#### Inject the Service
```razor
@inject NavigationHelper NavHelper
```

#### Navigate to Routes
```csharp
// Navigate to home
NavHelper.NavigateToHome();

// Navigate to events list
NavHelper.NavigateToEvents();

// Navigate to specific event
NavHelper.NavigateToEventDetails(eventId: 5);

// Navigate to registration
NavHelper.NavigateToEventRegistration(eventId: 5);
```

#### Navigate with Query Parameters
```csharp
// Navigate with category filter
NavHelper.NavigateToEventsWithCategory("Technology");

// Navigate with search term
NavHelper.NavigateToEventsWithSearch("conference");
```

#### Check Current Route
```csharp
// Check if on specific route
bool isOnHome = NavHelper.IsCurrentRoute("/");

// Check if route is active (matches prefix)
bool isOnEvents = NavHelper.IsRouteActive("/events");
```

### 2. Using NavLink Component

For navigation menu items with automatic active state:

```razor
<NavLink class="nav-link" href="events" Match="NavLinkMatch.Prefix">
    <span class="bi bi-calendar-event-nav-menu"></span> Events
</NavLink>
```

**Match Options:**
- `NavLinkMatch.All` - Exact match required
- `NavLinkMatch.Prefix` - Match if URL starts with href

### 3. Using Standard Links

For simple navigation:

```razor
<a href="/events">Browse Events</a>
```

With click handler:

```razor
<a href="/events" @onclick="HandleClick" @onclick:preventDefault="true">
    Browse Events
</a>

@code {
    private void HandleClick()
    {
        // Custom logic before navigation
        NavHelper.NavigateToEvents();
    }
}
```

### 4. Programmatic Navigation

Using NavigationManager directly (when NavigationHelper doesn't suffice):

```csharp
@inject NavigationManager Navigation

private void NavigateWithForceLoad()
{
    Navigation.NavigateTo("/events", forceLoad: true);
}

private void NavigateWithReplace()
{
    Navigation.NavigateTo("/events", replace: true);
}
```

---

## Best Practices

### ✅ DO

1. **Use NavigationHelper for consistency**
   ```csharp
   NavHelper.NavigateToEventDetails(id);
   ```

2. **Use route constraints for type safety**
   ```razor
   @page "/events/{id:int}"
   ```

3. **Validate route parameters**
   ```csharp
   protected override void OnInitialized()
   {
       if (!NavHelper.IsValidEventId(Id.ToString()))
       {
           NavHelper.NavigateToEvents();
           return;
       }
       LoadEvent();
   }
   ```

4. **Provide breadcrumbs for deep navigation**
   ```razor
   <Breadcrumb Items="@breadcrumbItems" />
   ```

5. **Use NavLink for menu items**
   ```razor
   <NavLink href="events" Match="NavLinkMatch.Prefix">Events</NavLink>
   ```

6. **Handle navigation events**
   ```csharp
   protected override void OnInitialized()
   {
       NavigationManager.LocationChanged += OnLocationChanged;
   }
   
   public void Dispose()
   {
       NavigationManager.LocationChanged -= OnLocationChanged;
   }
   ```

### ❌ DON'T

1. **Hardcode URLs throughout the application**
   ```csharp
   // Bad
   NavigationManager.NavigateTo($"/events/{id}");
   
   // Good
   NavHelper.NavigateToEventDetails(id);
   ```

2. **Forget to validate route parameters**
   ```csharp
   // Bad - no validation
   var eventItem = EventService.GetEventById(Id);
   
   // Good - with null check
   var eventItem = EventService.GetEventById(Id);
   if (eventItem == null)
   {
       NavHelper.NavigateToEvents();
       return;
   }
   ```

3. **Use relative paths inconsistently**
   ```razor
   <!-- Bad -->
   <a href="events">Events</a>
   <a href="/events">Events</a>
   
   <!-- Good - be consistent -->
   <a href="/events">Events</a>
   ```

4. **Forget to unsubscribe from events**
   ```csharp
   // Always implement IDisposable when subscribing
   @implements IDisposable
   ```

---

## Code Examples

### Complete Navigation Implementation

#### Component with Full Navigation

```razor
@page "/events/{id:int}"
@using Blazor.Services
@inject NavigationHelper NavHelper
@inject EventService EventService
@rendermode InteractiveServer

<div class="container">
    <Breadcrumb Items="@breadcrumbItems" />
    
    @if (eventItem == null)
    {
        <div class="alert alert-warning">
            <p>Event not found</p>
            <button @onclick="GoToEvents">Back to Events</button>
        </div>
    }
    else
    {
        <h1>@eventItem.Name</h1>
        <button @onclick="Register">Register Now</button>
        <button @onclick="GoBack">Back</button>
    }
</div>

@code {
    [Parameter]
    public int Id { get; set; }
    
    private Event? eventItem;
    private List<BreadcrumbItem> breadcrumbItems = new();
    
    protected override void OnInitialized()
    {
        LoadEvent();
        SetupBreadcrumbs();
    }
    
    private void LoadEvent()
    {
        eventItem = EventService.GetEventById(Id);
    }
    
    private void SetupBreadcrumbs()
    {
        breadcrumbItems = new List<BreadcrumbItem>
        {
            new() { Text = "Home", Url = "/", IsActive = false },
            new() { Text = "Events", Url = "/events", IsActive = false },
            new() { Text = eventItem?.Name ?? "Event", Url = $"/events/{Id}", IsActive = true }
        };
    }
    
    private void Register() => NavHelper.NavigateToEventRegistration(Id);
    private void GoToEvents() => NavHelper.NavigateToEvents();
    private void GoBack() => NavHelper.NavigateToEvents(); // Or use browser back
}
```

### Custom Navigation Guard

```csharp
// Services/NavigationGuard.cs
public class NavigationGuard
{
    private readonly EventService _eventService;
    private readonly NavigationHelper _navHelper;
    
    public NavigationGuard(EventService eventService, NavigationHelper navHelper)
    {
        _eventService = eventService;
        _navHelper = navHelper;
    }
    
    public bool CanNavigateToEvent(int eventId)
    {
        var eventItem = _eventService.GetEventById(eventId);
        return eventItem != null;
    }
    
    public bool CanRegisterForEvent(int eventId)
    {
        var eventItem = _eventService.GetEventById(eventId);
        return eventItem != null && eventItem.RegisteredAttendees < eventItem.Capacity;
    }
    
    public void ValidateAndNavigateToEvent(int eventId)
    {
        if (CanNavigateToEvent(eventId))
        {
            _navHelper.NavigateToEventDetails(eventId);
        }
        else
        {
            _navHelper.NavigateToEvents();
        }
    }
}
```

### Breadcrumb Component Usage

```razor
<!-- Automatic breadcrumb generation -->
<Breadcrumb AutoGenerate="true" />

<!-- Manual breadcrumb definition -->
<Breadcrumb Items="@breadcrumbItems" />

@code {
    private List<BreadcrumbItem> breadcrumbItems = new()
    {
        new() { Text = "Home", Url = "/", IsActive = false },
        new() { Text = "Events", Url = "/events", IsActive = false },
        new() { Text = "Current Page", Url = "/current", IsActive = true }
    };
}
```

### Navigation with State Preservation

```razor
@code {
    private void NavigateWithState()
    {
        // Save state before navigation
        var state = new Dictionary<string, object>
        {
            ["searchTerm"] = searchTerm,
            ["selectedCategory"] = selectedCategory
        };
        
        // Navigate (state would be retrieved on return)
        NavHelper.NavigateToEventDetails(eventId);
    }
}
```

---

## Route Testing and Verification

### Manual Testing Checklist

- [ ] Home page loads correctly at `/`
- [ ] Events list loads at `/events`
- [ ] Event details loads with valid ID at `/events/1`
- [ ] Invalid event ID redirects to events list
- [ ] Registration page loads at `/events/1/register`
- [ ] Breadcrumbs display correctly on all pages
- [ ] Back navigation works as expected
- [ ] NavLink highlights active routes
- [ ] Query parameters work with filters
- [ ] 404 page displays for invalid routes

### Automated Route Verification

```csharp
// Example unit test (for future implementation)
public class NavigationTests
{
    [Fact]
    public void NavigateToEventDetails_WithValidId_ShouldNavigate()
    {
        var navHelper = CreateNavigationHelper();
        navHelper.NavigateToEventDetails(1);
        // Assert navigation occurred
    }
    
    [Fact]
    public void IsValidEventId_WithInvalidId_ShouldReturnFalse()
    {
        var navHelper = CreateNavigationHelper();
        var result = navHelper.IsValidEventId("abc");
        Assert.False(result);
    }
}
```

---

## Troubleshooting

### Issue: Routes Not Working

**Symptoms:** Clicking links doesn't navigate, or routes show 404.

**Solutions:**
1. Check that `@page` directive is present
2. Verify route constraints match parameter types
3. Ensure `MapRazorComponents` is called in Program.cs
4. Check for typos in route definitions

### Issue: Parameters Not Binding

**Symptoms:** Route parameter is always null or default value.

**Solutions:**
1. Add `[Parameter]` attribute
2. Match parameter name case-insensitively with route
3. Use correct route constraint (`:int`, `:guid`, etc.)

```cs
// Component parameter must match route parameter name
@page "/events/{id:int}"

@code {
    [Parameter]
    public int Id { get; set; } // Name matches route
}
```

### Issue: Breadcrumbs Not Updating

**Symptoms:** Breadcrumbs show wrong page or don't update.

**Solutions:**
1. Call `SetupBreadcrumbs()` in `OnInitialized()` or `OnParametersSet()`
2. Ensure `StateHasChanged()` is called if updating after async operation
3. Check breadcrumb item URLs are correct

### Issue: NavigationHelper Not Found

**Symptoms:** Dependency injection fails for NavigationHelper.

**Solutions:**
1. Register service in Program.cs:
   ```csharp
   builder.Services.AddScoped<NavigationHelper>();
   ```
2. Inject in component:
   ```razor
   @inject NavigationHelper NavHelper
   ```

---

## Advanced Topics

### Deep Linking with Query Parameters

```csharp
// Read query parameters
var uri = new Uri(NavigationManager.Uri);
var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
var category = query["category"];
var searchTerm = query["search"];
```

### Preserving Scroll Position

```razor
@inject IJSRuntime JS

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("scrollTo", 0, 0);
        }
    }
}
```

### Handling External Links

```razor
<a href="https://external-site.com" target="_blank" rel="noopener noreferrer">
    External Link
</a>
```

---

## Route Constants Reference

All route constants are defined in `NavigationHelper.cs`:

```csharp
public const string HomeRoute = "/";
public const string EventsRoute = "/events";
public const string EventDetailsRoute = "/events/{0}";
public const string EventRegistrationRoute = "/events/{0}/register";
```

Use these constants to ensure consistency across the application.

---

## Summary

✅ **Always use NavigationHelper** for programmatic navigation  
✅ **Implement breadcrumbs** for deep pages  
✅ **Validate route parameters** before using them  
✅ **Use NavLink** for navigation menus  
✅ **Add route constraints** for type safety  
✅ **Handle 404 cases** gracefully  
✅ **Test all navigation paths** regularly  

For questions or issues with routing, refer to this guide or consult the [Blazor Routing Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing).
