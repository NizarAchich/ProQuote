using Microsoft.EntityFrameworkCore;
using ProQuote.Backend.Hubs;
using ProQuote.Database;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ProQuoteDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? "Server=(localdb)\\mssqllocaldb;Database=ProQuote;Trusted_Connection=True;";
    _ = options.UseSqlServer(connectionString);
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () => Results.Ok("OK"))
    .WithName("Health")
    .WithOpenApi();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<DashboardHub>("/hubs/dashboards");
app.MapHub<TakeoffHub>("/hubs/takeoff");
app.MapHub<AutomationHub>("/hubs/automation");

app.Run();
