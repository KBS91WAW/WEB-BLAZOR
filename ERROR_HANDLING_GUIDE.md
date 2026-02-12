# Error Handling & Validation Guide

## Overview
This guide documents the comprehensive error handling and validation system implemented across the EventEase Blazor application. The system ensures robust error handling for invalid routes, missing data, and edge cases.

## Components with Error Handling

### 1. EventCard Component
**Location:** `Components/EventCard.razor`

**Validation Features:**
- **Data Validation:** `IsValid()` method checks:
  - Event object is not null
  - Event ID is greater than 0
  - Event name is not empty
  - Capacity is greater than 0
  - Registered attendees within valid range (0 to capacity)
  - Event date is valid (not default DateTime)

- **Null-Safe Rendering:**
  ```csharp
  private string GetSafeName() => Event?.Name ?? "Unknown Event";
  private string GetSafeLocation() => Event?.Location ?? "TBD";
  private string GetSafeCategory() => Event?.Category ?? "Uncategorized";
  ```

- **Error UI:**
  - Invalid events display warning icon with "Invalid event data" message
  - Registration button is disabled for invalid events
  - Red border highlights problematic cards

- **Performance Optimization:**
  - Caches expensive calculations in `OnParametersSet()`
  - Stores `_cachedProgressPercentage` and `_cachedProgressBarClass`
  - Prevents redundant calculations on each render

**Usage Example:**
```razor
<EventCard Event="@eventItem" />
```

---

### 2. EventDetails Page
**Location:** `Components/Pages/EventDetails.razor`

**Error Handling Features:**
- **Loading State:** Shows spinner while loading event data
- **Invalid ID Validation:** Checks if ID is positive and exists
- **Event Not Found:** User-friendly error with navigation options
- **Async Loading:** `LoadEventAsync()` with try-catch wrapper

**Error Types Handled:**
1. **Invalid Event ID (ID <= 0):**
   - Message: "Invalid Event ID"
   - Details: Shows the provided ID value

2. **Event Not Found:**
   - Message: "Event Not Found"
   - Details: Explains event may have been removed

3. **Loading Error:**
   - Message: "Error Loading Event"
   - Details: Shows exception message

**Error UI Components:**
```razor
@if (isLoading)
{
    <!-- Loading spinner -->
}
else if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="card border-danger">
        <i class="bi bi-exclamation-circle-fill text-danger"></i>
        <h3>@errorMessage</h3>
        <p>@errorDetails</p>
        <!-- Navigation buttons -->
    </div>
}
```

**Route Parameter Handling:**
- Uses `:int` constraint to ensure ID is numeric
- Validates ID in `OnParametersSetAsync()` when route changes
- Reloads data when navigating between different event IDs

---

### 3. EventRegistration Page
**Location:** `Components/Pages/EventRegistration.razor`

**Comprehensive Validation:**
1. **Event ID Validation:**
   - Checks if ID is positive
   - Validates event exists using `NavHelper.IsValidEventId()`

2. **Event Capacity Check:**
   - Prevents registration if event is full
   - Shows current capacity status

3. **Event Date Validation:**
   - Blocks registration for past events
   - Displays when event occurred

4. **Race Condition Handling:**
   - Re-validates capacity before final submission
   - Handles case where event fills up during form completion

**Error States:**
```csharp
// During initial load
validationError = "Event is Full";
validationDetails = "Event has reached maximum capacity...";

// During form submission
errorMessage = "Event is now full. Someone registered just before you.";
```

**Form Validation:**
- Uses DataAnnotations for input validation
- Required fields: FirstName, LastName, Email, Phone
- Email format validation
- Phone format validation
- Terms agreement checkbox with Range validation

**Error Messages:**
- **Invalid Event ID:** "The event ID must be a positive number"
- **Event Not Found:** "No event exists with ID {Id}"
- **Event Full:** "Event has reached its maximum capacity"
- **Event Passed:** "You cannot register for past events"
- **Loading Error:** "An unexpected error occurred"
- **Registration Failed:** "Registration failed. Event may be full"

---

### 4. NotFound Page
**Location:** `Components/Pages/NotFound.razor`

**Features:**
- **User-Friendly 404 Page:**
  - Large warning icon
  - Clear "404 Page Not Found" heading
  - Explanation message

- **Navigation Options:**
  - "Go Home" button
  - "Browse Events" button
  - "Go Back" link
  - Suggested links list

- **Requested Path Display:**
  - Shows the incorrect URL that was requested
  - Helps users understand what went wrong

**Example Usage:**
```
URL: /events/999/register (for non-existent event)
Result: Custom error page with navigation options
```

---

## Validation Patterns

### Client-Side Validation
**EventCard Component:**
```csharp
private bool IsValid()
{
    if (Event == null) return false;
    if (Event.Id <= 0) return false;
    if (string.IsNullOrWhiteSpace(Event.Name)) return false;
    if (Event.Capacity <= 0) return false;
    if (Event.RegisteredAttendees < 0 || 
        Event.RegisteredAttendees > Event.Capacity) return false;
    if (Event.Date == default) return false;
    return true;
}
```

### Route Parameter Validation
**EventDetails & EventRegistration:**
```csharp
// Validate ID is positive
if (Id <= 0)
{
    errorMessage = "Invalid Event ID";
    return;
}

// Validate event exists
if (!NavHelper.IsValidEventId(Id))
{
    errorMessage = "Event Not Found";
    return;
}
```

### Async Error Handling
```csharp
private async Task LoadEventAsync()
{
    isLoading = true;
    try
    {
        // Validation logic
        // Load event data
    }
    catch (Exception ex)
    {
        errorMessage = $"Error: {ex.Message}";
    }
    finally
    {
        isLoading = false;
    }
}
```

---

## Error Recovery Strategies

### 1. Graceful Degradation
- **EventCard:** Shows error message instead of crashing
- **Missing Data:** Falls back to default values ("Unknown Event", "TBD")

### 2. User Navigation
- **All error pages provide clear navigation options:**
  - Return to event list
  - Go to home page
  - View event details (if event exists)

### 3. Informative Messages
- **Technical details for debugging**
- **User-friendly language**
- **Actionable next steps**

### 4. Real-Time Validation
- **Registration form validates on submit**
- **Re-checks capacity before finalizing**
- **Updates UI with latest data**

---

## Testing Error Scenarios

### Test Invalid Event ID
```
URL: /events/-1
Expected: "Invalid Event ID" error page

URL: /events/999
Expected: "Event Not Found" error page
```

### Test Full Event Registration
```
1. Navigate to event with RegisteredAttendees >= Capacity
2. Attempt to register
Expected: "Event is Full" error message
```

### Test Past Event Registration
```
1. Navigate to event with Date < DateTime.Now
2. Attempt to register
Expected: "Event Has Passed" error message
```

### Test Invalid Event Data
```
1. Create Event with empty Name or Capacity = 0
2. Render in EventCard
Expected: Warning icon with "Invalid event data" message
```

---

## Performance Considerations

### Caching Strategy
**EventCard Component:**
- Calculates progress percentage once in `OnParametersSet()`
- Stores result in private field
- Avoids recalculation on every render

```csharp
protected override void OnParametersSet()
{
    if (!_cacheInitialized)
    {
        _cachedProgressPercentage = CalculateProgressPercentage();
        _cachedProgressBarClass = CalculateProgressBarClass();
        _cacheInitialized = true;
    }
}
```

### Async Operations
- Loading states prevent UI blocking
- User sees progress feedback
- Errors don't leave UI in loading state (finally block)

---

## Error Logging (Future Enhancement)

Consider adding:
```csharp
private void LogError(string context, Exception ex)
{
    // Log to console
    Console.WriteLine($"[{context}] {ex.Message}");
    
    // Future: Send to logging service
    // await LoggingService.LogErrorAsync(context, ex);
}
```

---

## Best Practices

1. **Always Validate Route Parameters:**
   - Check if IDs are positive
   - Verify entities exist before rendering

2. **Provide Loading Feedback:**
   - Show spinners during async operations
   - Never leave UI in indeterminate state

3. **Use Try-Catch for External Calls:**
   - Wrap service calls in try-catch
   - Handle exceptions gracefully

4. **Cache Expensive Calculations:**
   - Use `OnParametersSet()` for initialization
   - Store computed values in fields

5. **Display User-Friendly Messages:**
   - Avoid technical jargon in error messages
   - Provide clear next steps
   - Include navigation options

6. **Test Edge Cases:**
   - Invalid IDs (negative, zero, non-existent)
   - Null/missing data
   - Race conditions (capacity changes)
   - Past dates

---

## Related Documentation
- [ROUTING_GUIDE.md](ROUTING_GUIDE.md) - Navigation and routing patterns
- [DATA_BINDING_GUIDE.md](DATA_BINDING_GUIDE.md) - Data binding examples
- [NAVIGATION_SUMMARY.md](NAVIGATION_SUMMARY.md) - Navigation system overview
