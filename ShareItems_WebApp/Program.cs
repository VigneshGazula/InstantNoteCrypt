using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;
using SixLabors.ImageSharp;

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(connectionString));

// Register services
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddDataProtection();
builder.Services.AddControllersWithViews();

var app = builder.Build();
var env = app.Services.GetRequiredService<IWebHostEnvironment>();

// Ensure uploads directory exists
var uploadsPath = Path.Combine(env.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseRouting();
app.UseStaticFiles();
app.MapControllers();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");
    

app.Run();
