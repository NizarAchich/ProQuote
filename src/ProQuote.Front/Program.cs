using MudBlazor.Services;
using ProQuote.Front.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddHttpClient("ProQuoteBackend", client =>
{
    string baseUrl = builder.Configuration["Backend:BaseUrl"] ?? "https://localhost:5001";
    client.BaseAddress = new Uri(baseUrl);
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
    _ = app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
