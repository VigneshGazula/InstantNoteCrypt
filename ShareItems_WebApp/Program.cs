using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;
using ShareItems_WebApp.Settings;
using SixLabors.ImageSharp;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnectionString");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string not found. " +
        "Please set 'DefaultConnectionString' in appsettings.json or 'DB_CONNECTION' environment variable.");
}

builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(connectionString));

// Configure Cloudinary settings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Register services
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddDataProtection();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();
var env = app.Services.GetRequiredService<IWebHostEnvironment>();

// Note: No longer need to create uploads directory since we're using Cloudinary

app.UseRouting();
app.UseStaticFiles();
app.MapRazorPages();
app.MapControllers();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");
    

app.Run();

