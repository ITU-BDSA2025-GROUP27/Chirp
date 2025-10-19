using Chirp.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Get database path from environment variable or use default temp directory
var dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH")
    ?? Path.Combine(Path.GetTempPath(), "chirp.db");

builder.Services.AddSingleton<DBFacade>(provider => new DBFacade(dbPath));
builder.Services.AddSingleton<ICheepService, CheepService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();

public partial class Program { }
