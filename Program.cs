using Blazor.Components;
using Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register EventService as a singleton
builder.Services.AddSingleton<EventService>();

// Register UserSessionService as a scoped service (per-user state)
builder.Services.AddScoped<UserSessionService>();

// Register AttendanceService as a scoped service (needs user session)
builder.Services.AddScoped<AttendanceService>();

// Register NavigationHelper as a scoped service
builder.Services.AddScoped<NavigationHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
