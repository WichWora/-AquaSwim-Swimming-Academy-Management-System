global using AQUA.Models;
using AQUA.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================
// 1. CONFIGURE SERVICES
// ======================
builder.Services.AddControllersWithViews();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Database
string dbPath = Path.Combine(builder.Environment.ContentRootPath, "DB.mdf");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer($@"
         Server=(localdb)\MSSQLLocalDB;
         AttachDbFilename={dbPath};
         Trusted_Connection=True;
         MultipleActiveResultSets=True;
         Connect Timeout=30
     "));

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<Helper>();
builder.Services.AddSingleton<InMemoryOrderConnector>(); // ← Add this line


// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// ======================
// 2. MIDDLEWARE
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Initialize database & seed Manager
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var helper = scope.ServiceProvider.GetRequiredService<Helper>();

        try
        {
            if (dbContext.Database.CanConnect())
            {
                logger.LogInformation("✅ Database connection successful!");

                // Check if tables exist
                var usersTableExists = dbContext.Database.ExecuteSqlRaw(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users') THEN 1 ELSE 0 END") == 1;

                if (!usersTableExists)
                {
                    logger.LogInformation("Tables don't exist. Creating database...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("✅ Database created with all tables!");
                }
                else
                {
                    logger.LogInformation("✅ Database tables already exist.");
                }

                // Seed Manager only if not exists
                if (!await dbContext.Users.AnyAsync(u => u.Role == "Manager"))
                {
                    var manager = new Manager
                    {
                        UserId = "M0001",
                        FullName = "System Manager",
                        Email = "manager@aquasys.local",
                        Phone = "0000000000",
                        Password = "MANAGERPASS123",  // ← CHANGE THIS in production!
                        Role = "Manager",
                        ImageUrl = "default.png",
                        RegistrationDate = DateTime.Now,
                        IsActive = true,
                        AccessLevel = 999
                    };

                    dbContext.Users.Add(manager);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("✅ Manager account seeded.");
                }
            }
            else
            {
                logger.LogWarning("⚠️ Cannot connect to database. Please check connection string.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database initialization error");
            Console.WriteLine($"Database Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Optional path base fix (from LearnSwim)
app.Use((context, next) =>
{
    context.Request.PathBase = "";
    return next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ======================
// 3. ROUTES
// ======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Test & Health endpoints
app.MapGet("/test", () => "Email Subscription System is Running!");
app.MapGet("/health", () => "POOL Application is running!");
app.MapGet("/db-check", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var userCount = await db.Users.CountAsync();
        return Results.Ok(new
        {
            DatabaseConnected = canConnect,
            UserCount = userCount,
            Message = "Database check successful"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});

app.MapDefaultControllerRoute();

app.Run();
