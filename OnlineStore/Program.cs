using System.Threading.RateLimiting;
using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Constants;
using OnlineStore.Models;
using OnlineStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddSingleton<ISalesReportWorkbookExporter, SalesReportWorkbookExporter>();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IAccountTokenService, AccountTokenService>();
builder.Services.AddSingleton<IStoreEmailSender, UnconfiguredStoreEmailSender>();
builder.Services.AddScoped<IExternalAccountService, ExternalAccountService>();

var externalAuthentication = new ExternalAuthenticationOptions();
builder.Configuration.GetSection("Authentication").Bind(externalAuthentication);
var externalAvailability = new ExternalProviderAvailability(
    externalAuthentication.Google.IsConfigured,
    externalAuthentication.Apple.IsConfigured);
builder.Services.AddSingleton(externalAvailability);

var authentication = builder.Services
    .AddAuthentication()
    .AddCookie(ExternalAuthenticationSchemes.ExternalCookie, options =>
    {
        options.Cookie.Name = ".AuraCommerce.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

if (externalAvailability.Google)
{
    authentication.AddGoogle(ExternalAuthenticationSchemes.Google, options =>
    {
        options.SignInScheme = ExternalAuthenticationSchemes.ExternalCookie;
        options.ClientId = externalAuthentication.Google.ClientId;
        options.ClientSecret = externalAuthentication.Google.ClientSecret;
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/Account/ExternalLoginCallback?remoteError=Google");
            return Task.CompletedTask;
        };
    });
}

if (externalAvailability.Apple)
{
    authentication.AddApple(ExternalAuthenticationSchemes.Apple, options =>
    {
        options.SignInScheme = ExternalAuthenticationSchemes.ExternalCookie;
        options.ClientId = externalAuthentication.Apple.ClientId;
        options.TeamId = externalAuthentication.Apple.TeamId;
        options.KeyId = externalAuthentication.Apple.KeyId;
        options.GenerateClientSecret = true;
        options.PrivateKey = (_, _) =>
            Task.FromResult(externalAuthentication.Apple.PrivateKey.AsMemory());
        options.Scope.Add("email");
        options.Scope.Add("name");
        options.ClaimActions.MapJsonKey("email_verified", "email_verified");
        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/Account/ExternalLoginCallback?remoteError=Apple");
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".AuraCommerce.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Set Security:RequireHttps=true when the public deployment is served through HTTPS.
    options.Cookie.SecurePolicy = builder.Configuration.GetValue<bool>("Security:RequireHttps")
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Automatic migrations are useful for local development, but can crash an IIS
// worker process when a hosted SQL connection is unavailable or lacks DDL rights.
// Production deployments should apply migrations as a deployment step and leave
// this switch disabled unless the host explicitly supports startup migrations.
if (builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
