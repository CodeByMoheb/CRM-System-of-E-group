using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Sector_13_Welfare_Society___Digital_Management_System.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Use SQL Server for both development and production with context pooling
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.CommandTimeout(180);
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Bind EmailSettings
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Register EmailSender
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));

builder.Services.AddScoped<Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms.ISmsSender, Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms.BulkSmsBdSender>();

// Add HttpClientFactory for Cloudinary uploads
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();

// Remove AddDefaultIdentity
// builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
//     .AddRoles<IdentityRole>()
//     .AddEntityFrameworkStores<ApplicationDbContext>();

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Register Leave Management Service
builder.Services.AddScoped<Sector_13_Welfare_Society___Digital_Management_System.Services.ILeaveManagementService, Sector_13_Welfare_Society___Digital_Management_System.Services.LeaveManagementService>();

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("bn")
    };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    // Relax password rules for employee self-service passwords
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Compression & caching
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/plain","text/html","text/css","application/javascript","application/json","image/svg+xml"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("Public60", b => b.Expire(TimeSpan.FromSeconds(60)).SetVaryByRouteValue("id"));
    options.AddPolicy("Public300", b => b.Expire(TimeSpan.FromSeconds(300)));
});
builder.Services.AddMemoryCache();

// Register Google authentication AFTER Identity
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    // In development, we'll skip HTTPS redirection to avoid the warning
    // app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseResponseCompression();

// Cache static files aggressively in production
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int oneYearSeconds = 60 * 60 * 24 * 365;
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={oneYearSeconds},immutable";
    }
});
// Add localization middleware before UseRouting
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCaching();
app.UseOutputCache();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed roles and SuperAdmin
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Ensure Member Directory tables exist (idempotent)
            var ensureSql = @"IF OBJECT_ID('dbo.BuyerContacts','U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BuyerContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ContactPerson] nvarchar(150) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BuyerContacts] PRIMARY KEY ([Id])
    );
END
IF OBJECT_ID('dbo.ClientContacts','U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ClientContacts](
        [Id] uniqueidentifier NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ContactPerson] nvarchar(150) NOT NULL,
        [Designation] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(30) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ClientContacts] PRIMARY KEY ([Id])
    );
END";
            context.Database.ExecuteSqlRaw(ensureSql);

            // Ensure missing columns exist for current model (idempotent)
            var ensureColumnsSql = @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Registration_fees')
BEGIN
    ALTER TABLE [dbo].[Services] ADD [Registration_fees] decimal(18,2) NOT NULL DEFAULT(0);
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'CompanyCalId')
BEGIN
    ALTER TABLE [dbo].[Services] ADD [CompanyCalId] nvarchar(50) NULL;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND name = 'CompanyCalId')
BEGIN
    ALTER TABLE [dbo].[Customers] ADD [CompanyCalId] nvarchar(50) NULL;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Invoices]') AND name = 'CompanyCalId')
BEGIN
    ALTER TABLE [dbo].[Invoices] ADD [CompanyCalId] nvarchar(50) NULL;
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[BuyerContacts]') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE [dbo].[BuyerContacts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT(GETDATE());
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClientContacts]') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE [dbo].[ClientContacts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT(GETDATE());
END";
            context.Database.ExecuteSqlRaw(ensureColumnsSql);
        }
        catch { /* ignore if lacks permissions */ }
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = new[] { "SuperAdmin", "Admin", "President", "Secretary", "Manager", "Member" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
        // Seed SuperAdmin user
        var superAdminEmail = "superadmin@sec13.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdminUser == null)
        {
            var user = new ApplicationUser 
            { 
                UserName = superAdminEmail, 
                Email = superAdminEmail, 
                EmailConfirmed = true, 
                PhoneNumber = "+8801700000000",
                PhoneNumberConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "SuperAdmin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
        
        // Seed Book Us data - temporarily disabled
        // await SeedBookUsData.SeedAsync(context);
    }
}
catch (Exception ex)
{
    // Log the error but don't stop the application
    Console.WriteLine($"Error during seeding: {ex.Message}");
}

app.Run();
