# Data Binding Implementation Guide - EventEase

## Overview
This document explains the advanced data binding techniques implemented in the EventEase Blazor application, demonstrating various binding patterns and best practices.

## Data Binding Features Implemented

### 1. Two-Way Data Binding (`@bind`)

#### Location: [Events.razor](Components/Pages/Events.razor)

**Search Box with Real-time Filtering:**
```razor
<input type="text" 
       id="searchBox" 
       class="form-control" 
       placeholder="Search by name or location..."
       @bind="searchTerm"
       @bind:event="oninput" />
```

**Explanation:**
- `@bind="searchTerm"` creates a two-way binding to the `searchTerm` property
- `@bind:event="oninput"` triggers the binding on every keystroke (instead of onChange)
- Changes automatically call `ApplyFilters()` method through property setters

**Category Filter with @bind:**
```razor
<select id="categoryFilter" 
        class="form-select" 
        @bind="selectedCategory">
    <option value="">All Categories</option>
    @foreach (var category in categories)
    {
        <option value="@category">@category</option>
    }
</select>
```

**Checkbox Binding:**
```razor
<input class="form-check-input" 
       type="checkbox" 
       id="availableOnly"
       @bind="showAvailableOnly" />
```

### 2. Component Parameter Binding

#### Location: [EventCard.razor](Components/EventCard.razor)

**Component with Multiple Parameters:**
```razor
<EventCard Event="eventItem" 
           OnViewDetailsClicked="ViewEventDetails"
           ShowProgress="true"
           ShowAvailability="true"
           ShowCountdown="true"
           IsHighlighted="IsEventUpcoming(eventItem)" />
```

**Parameter Definitions:**
```csharp
[Parameter]
public Event Event { get; set; } = null!;

[Parameter]
public EventCallback<int> OnViewDetailsClicked { get; set; }

[Parameter]
public bool ShowProgress { get; set; } = true;

[Parameter]
public bool ShowAvailability { get; set; } = true;
```

**Key Concepts:**
- `[Parameter]` attribute exposes properties for parent components to bind
- `EventCallback<T>` enables child-to-parent communication
- Default values provide sensible behavior when parameters aren't specified

### 3. Event Callback Binding

#### Location: [EventPreview.razor](Components/EventPreview.razor)

**Mouse Event Binding:**
```razor
<div @onmouseenter="HandleMouseEnter" 
     @onmouseleave="HandleMouseLeave">
    <!-- Content -->
</div>
```

**EventCallback Implementation:**
```csharp
[Parameter]
public EventCallback<Event> OnEventHovered { get; set; }

private async Task HandleMouseEnter()
{
    IsHovered = true;
    if (OnEventHovered.HasDelegate)
    {
        await OnEventHovered.InvokeAsync(Event);
    }
}
```

**Usage in Parent Component ([Home.razor](Components/Pages/Home.razor)):**
```razor
<EventPreview Event="eventItem" 
              ShowTooltip="true"
              OnEventHovered="HandleEventHovered" />

@code {
    private Event? hoveredEvent;
    
    private void HandleEventHovered(Event eventItem)
    {
        hoveredEvent = eventItem;
        StateHasChanged();
    }
}
```

### 4. Dynamic Expression Binding

#### Location: Multiple components

**Computed Values in Bindings:**
```razor
<div class="progress-bar @GetProgressBarClass()" 
     style="width: @GetProgressPercentage()%">
    @Math.Round(GetProgressPercentage(), 1)%
</div>
```

**Method-Based Binding:**
```csharp
private double GetProgressPercentage()
{
    return ((double)Event.RegisteredAttendees / Event.Capacity) * 100;
}

private string GetProgressBarClass()
{
    var percentage = GetProgressPercentage();
    return percentage switch
    {
        >= 90 => "bg-danger",
        >= 70 => "bg-warning",
        _ => "bg-success"
    };
}
```

### 5. Conditional Rendering with Data Binding

#### Location: [Events.razor](Components/Pages/Events.razor)

**@if Directive with Bound Data:**
```razor
@if (!string.IsNullOrEmpty(searchTerm))
{
    <small class="text-muted">Found @filteredEvents.Count result(s)</small>
}

@if (filteredEvents == null || !filteredEvents.Any())
{
    <div class="alert alert-info">
        <i class="bi bi-info-circle"></i> No events found matching your criteria.
    </div>
}
else
{
    <div class="row">
        @foreach (var eventItem in filteredEvents)
        {
            <EventCard Event="eventItem" ... />
        }
    </div>
}
```

### 6. Live Data Updates with Timers

#### Location: [EventStatistics.razor](Components/EventStatistics.razor)

**Auto-Refresh Implementation:**
```csharp
private System.Threading.Timer? _timer;

protected override void OnInitialized()
{
    UpdateStatistics();
    
    // Set up timer for live updates
    _timer = new System.Threading.Timer(async _ =>
    {
        UpdateStatistics();
        await InvokeAsync(StateHasChanged);
    }, null, RefreshInterval, RefreshInterval);
}

public void Dispose()
{
    _timer?.Dispose();
}
```

**Usage:**
```razor
<EventStatistics ShowCategoryBreakdown="true" RefreshInterval="5000" />
```

### 7. Form Input Binding

#### Location: [EventRegistration.razor](Components/Pages/EventRegistration.razor)

**EditForm with Data Annotations:**
```razor
<EditForm Model="@registrationForm" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <InputText id="firstName" 
               class="form-control" 
               @bind-Value="registrationForm.FirstName" />
    <ValidationMessage For="@(() => registrationForm.FirstName)" />
    
    <InputCheckbox id="agreeToTerms" 
                   class="form-check-input" 
                   @bind-Value="registrationForm.AgreeToTerms" />
</EditForm>
```

**Model with Validation:**
```csharp
public class RegistrationForm
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "You must agree to the terms")]
    [Range(typeof(bool), "true", "true")]
    public bool AgreeToTerms { get; set; }
}
```

### 8. Interactive State Management

#### Location: [CapacityTracker.razor](Components/CapacityTracker.razor)

**Simulated Registration with State Update:**
```razor
<button class="btn btn-sm btn-outline-primary" 
        @onclick="SimulateRegistration"
        disabled="@(Event.RegisteredAttendees >= Event.Capacity)">
    <i class="bi bi-person-plus"></i> Simulate Registration
</button>
```

**State Management:**
```csharp
[Parameter]
public EventCallback OnSimulateRegistration { get; set; }

private async Task SimulateRegistration()
{
    await OnSimulateRegistration.InvokeAsync();
}
```

**Parent Component Handler ([EventDetails.razor](Components/Pages/EventDetails.razor)):**
```csharp
private void SimulateRegistration()
{
    if (eventItem != null && eventItem.RegisteredAttendees < eventItem.Capacity)
    {
        EventService.RegisterForEvent(Id);
        LoadEvent(); // Reload to get updated data
        StateHasChanged(); // Force UI refresh
    }
}
```

## Data Binding Best Practices Demonstrated

### 1. Property Change Tracking
```csharp
private string SearchTerm
{
    get => searchTerm;
    set
    {
        searchTerm = value;
        ApplyFilters(); // Auto-trigger filtering
    }
}
```

### 2. Debouncing (using @bind:event)
```razor
<!-- Triggers on every keystroke -->
@bind:event="oninput"

<!-- Triggers on blur (default) -->
@bind:event="onchange"
```

### 3. Conditional CSS Classes
```razor
<div class="card @(IsHighlighted ? "border-primary border-2" : "")">
```

### 4. Dynamic Attribute Binding
```razor
<button disabled="@isSubmitting">
    @if (isSubmitting)
    {
        <span class="spinner-border spinner-border-sm me-2"></span>
        <span>Processing...</span>
    }
</button>
```

### 5. Lambda Expression Binding
```razor
<button @onclick="() => ViewEventDetails(eventItem.Id)">
    View Details
</button>
```

## Mock Data Pattern

### Location: [EventService.cs](Services/EventService.cs)

The application uses a singleton service with in-memory mock data:

```csharp
public class EventService
{
    private readonly List<Event> _events;

    public EventService()
    {
        _events = new List<Event>
        {
            new Event { Id = 1, Name = "Tech Conference 2026", ... },
            new Event { Id = 2, Name = "Annual Gala Dinner", ... },
            // ... more events
        };
    }

    public List<Event> GetAllEvents() => _events.OrderBy(e => e.Date).ToList();
    
    public Event? GetEventById(int id) => _events.FirstOrDefault(e => e.Id == id);
    
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
}
```

**Service Registration ([Program.cs](Program.cs)):**
```csharp
builder.Services.AddSingleton<EventService>();
```

## Testing the Data Binding Features

1. **Two-Way Binding:**
   - Navigate to `/events`
   - Type in the search box → Results update in real-time
   - Select a category → Events filter immediately
   - Toggle "Available only" → List updates instantly

2. **Component Parameters:**
   - Each EventCard shows dynamic capacity indicators
   - Hover over events on homepage → See interactive tooltips
   - Categories display with color-coded badges

3. **Event Callbacks:**
   - Hover over events on homepage → "Currently viewing" message updates
   - Click "Simulate Registration" on event details → Capacity updates live

4. **Form Binding:**
   - Navigate to an event registration page
   - Leave required fields empty and click submit → Validation messages appear
   - Fill form correctly → Registration succeeds

5. **Live Updates:**
   - Event statistics dashboard refreshes every 5 seconds automatically
   - Capacity trackers update when registrations occur

## Key Blazor Directives Used

- `@bind` - Two-way data binding
- `@bind:event` - Specify which event triggers the binding
- `@onclick` - Click event handling
- `@onmouseenter` / `@onmouseleave` - Mouse events
- `@if` / `@else` - Conditional rendering
- `@foreach` - Loop rendering
- `[Parameter]` - Component parameter declaration
- `EventCallback<T>` - Type-safe event callbacks
- `StateHasChanged()` - Manual UI refresh trigger
- `@rendermode InteractiveServer` - Enable Blazor Server interactivity

## Summary

The EventEase application demonstrates:
- ✅ Two-way data binding with `@bind`
- ✅ Component parameter binding
- ✅ Event callback patterns
- ✅ Dynamic expression binding
- ✅ Conditional rendering with data
- ✅ Live updates with timers
- ✅ Form input binding with validation
- ✅ Interactive state management
- ✅ Mock data service pattern
- ✅ Real-time UI updates

All components use Blazor's data binding features to create a responsive, interactive user experience without writing JavaScript.
