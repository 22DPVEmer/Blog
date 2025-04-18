using Blog.Infrastructure.Entities;
using Blog.Core.Settings;
using Blog.Core.Interfaces;
using Blog.Core.Services;
using Blog.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Blog.Web.Services;
using Blog.Core.Models;
using Blog.Core.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection("Identity"));

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Shared Email Queue Service
builder.Services.AddSingleton<ISharedEmailQueueService>(_ => new SharedEmailQueueService(isServer: false));

// Add Email Sender that uses the queue service
builder.Services.AddScoped<IEmailSender, EmailSenderService>();

// Add Firebase Storage Service
builder.Services.AddScoped<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IArticleVoteService, ArticleVoteService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Add SignalR
builder.Services.AddSignalR();

// Add Image Processing Background Service
builder.Services.AddSingleton<ImageProcessingBackgroundService>();
builder.Services.AddHostedService(
    provider => provider.GetRequiredService<ImageProcessingBackgroundService>());

// Add FileSettings configuration
builder.Services.Configure<FileSettings>(
    builder.Configuration.GetSection("FileSettings"));

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Add Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IArticleService, ArticleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enable session
app.UseSession();

// Map SignalR Hubs
app.MapHub<Blog.Web.Hubs.CommentHub>(CommentConstants.ApiRoutes.CommentHubEndpoint);

// Configure routes
app.MapControllerRoute(
    name: "default",
    pattern: CommentConstants.ApiRoutes.DefaultRoute);

app.MapRazorPages();

// Create default roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Create roles if they don't exist
    string[] roleNames = { "Administrator", "Editor", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

app.Run();
