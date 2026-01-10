using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;
using ShareItems_WebApp.Settings;
using ShareItems_WebApp.Helpers;
using SixLabors.ImageSharp;

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<UserContext>(options => options.UseSqlServer(connectionString));

// Configure Cloudinary settings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Add session support for PIN verification
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register services
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<NoteAuthorizationHelper>();

builder.Services.AddDataProtection();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();
var env = app.Services.GetRequiredService<IWebHostEnvironment>();

// Note: No longer need to create uploads directory since we're using Cloudinary

app.UseRouting();
app.UseStaticFiles();
app.UseSession(); // Enable session middleware for PIN verification
app.MapRazorPages();
app.MapControllers();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");
    

app.Run();

