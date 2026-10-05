using Account;
using BuildingBlocks.Web;
using Calendar;
using Menu;

// Composition root of the "core" process: hosts the Account, Menu and Calendar modules (ADR-001).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddDefaultHealthChecks();
builder.Services
    .AddAccountModule(builder.Configuration)
    .AddMenuModule(builder.Configuration)
    .AddCalendarModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapDefaultHealthChecks();
app.MapAccountEndpoints();
app.MapMenuEndpoints();
app.MapCalendarEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can start the host with WebApplicationFactory.</summary>
public partial class Program;
