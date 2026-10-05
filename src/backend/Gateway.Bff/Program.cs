using BuildingBlocks.Web;

// Backend For Frontend (ADR-009). Login, session and the API proxy come with tickets #14 and #15.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddDefaultHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapDefaultHealthChecks();

await app.RunAsync();
