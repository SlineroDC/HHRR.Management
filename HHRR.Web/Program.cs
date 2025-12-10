using HHRR.Application;
using HHRR.Application.Interfaces;
using HHRR.Infrastructure;
using HHRR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using HHRR.Infrastructure.Services;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 1. Load Configuration
var dbString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

Console.WriteLine("========================================");
Console.WriteLine($"[DEBUG] DB STRING: '{dbString}'");
Console.WriteLine($"[DEBUG] API KEY: '{apiKey}'");
Console.WriteLine("========================================");

// 2. Add Services
builder.Services.AddControllersWithViews();

// 3. Register Infrastructure (DbContext, Repos, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IAIService, AIService>();
// 4. Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 5. Configure Application Cookie (CRITICAL FOR LOGIN LOOP)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// 6. Pipeline Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled for Localhost debugging
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. Data Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    try 
    {
        await DbInitializer.SeedAsync(services, context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Seeding failed: {ex.Message}");
    }
}

app.Run();