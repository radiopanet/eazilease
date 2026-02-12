using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EaziLease.Infrastructure.Persistence;
using Npgsql.EntityFrameworkCore;
using EaziLease.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using EaziLease.Infrastructure.Services;
using EaziLease.Extensions;
using Microsoft.AspNetCore.Http;
using EaziLease.Application.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using EaziLease.Jobs;
using Microsoft.CodeAnalysis.Options;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),

    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "EaziLease.Web", "wwwroot")
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<MonthlySnapshotJob>();
builder.Services.AddScoped<MonthlyCompanyFinancialSnapshot>();
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IDriverAssignmentService, DriverAssignmentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();


builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin")  // Must still be admin
              .RequireAssertion(context => 
                  context.Resource is HttpContext httpContext && 
                  httpContext.IsSuperAdminElevated()));
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

}).AddRoles<IdentityRole>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
   options.AddPolicy("ClientOnly", policy => policy.RequireRole("ClientUser"));
});  




builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(EaziLease.Web.Controllers.HomeController).Assembly)
    .AddRazorRuntimeCompilation();

    builder.Services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
{
   options.FileProviders.Add(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
});

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    // Use relative paths starting from 'src'
    options.PageViewLocationFormats.Add("src/EaziLease.Web/Views/Shared/{0}.cshtml"); 
    options.ViewLocationFormats.Add("src/EaziLease.Web/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("src/EaziLease.Web/Views/Shared/{0}.cshtml");

    options.PageViewLocationFormats.Clear();
    options.PageViewLocationFormats.Add("src/EaziLease.Web/Areas/Identity/Pages/{1}/{0}.cshtml");
    options.PageViewLocationFormats.Add("src/EaziLease.Web/Areas/Identity/Pages/Shared/{0}.cshtml");
    
    // Explicitly add the search path for Identity's internal ViewStart
    options.PageViewLocationFormats.Add("src/EaziLease.Web/Areas/Identity/Pages/{0}.cshtml");
});

    // options =>
// {
//     var policy = new AuthorizationPolicyBuilder()
//         .RequireAuthenticatedUser()
//         .Build();
//     options.Filters.Add(new AuthorizeFilter(policy));  
// });
builder.Services.AddDistributedMemoryCache();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Point to the new view
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
// app.Use(async (context, next) =>
// {
//     // Only check if we are heading to an Identity page
//     if (context.Request.Path.StartsWithSegments("/Identity"))
//     {
//         var viewEngine = context.RequestServices.GetRequiredService<IRazorViewEngine>();
//         var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//         // These are the names the Identity UI internal code looks for
//         string[] partialsToTest = { "_LoginPartial", "_Layout", "_ViewStart" };

//         logger.LogInformation("--- Razor Search Path Debug ---");
//         foreach (var partial in partialsToTest)
//         {
//             var result = viewEngine.FindView(new ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()), partial, isMainPage: false);
            
//             if (result.Success)
//             {
//                 logger.LogInformation($"✅ FOUND {partial} at: {result.View.Path}");
//             }
//             else
//             {
//                 logger.LogWarning($"❌ FAILED to find {partial}. Searched locations:");
//                 foreach (var location in result.SearchedLocations)
//                 {
//                     logger.LogWarning($"   -> {location}");
//                 }
//             }
//         }
//         logger.LogInformation("--------------------------------");
//     }
//     await next();
// });

app.UseHangfireDashboard();


// Schedule recurring job 
RecurringJob.AddOrUpdate<MonthlySnapshotJob>( job => job.Execute(), Cron.Monthly);
RecurringJob.AddOrUpdate<MonthlyCompanyFinancialSnapshot>( s => s.CreateCompanySnapshot(), Cron.Monthly);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();
// app.UseHttpsRedirection();
app.UseRouting();


app.UseAuthentication();
app.UseSession();
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=SuperDashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=ClientDashboard}/{action=index}/{id?}");    

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await IdentitySeedData.Seed(services);
        Console.WriteLine("Admin user seeded successfully.");
        await SeedData.InitializeAsync(context);

        await ClientSeeder.SeedAsync(services);
        Console.WriteLine("Client user seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding failed: {ex.Message}");
    }
}




app.Run();


// // Helper to run migrations and seed on startup in dev
// void UseMigrationsAndSeed(IApplicationBuilder app)
// {
//     using var scope = app.ApplicationServices.CreateScope();
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     db.Database.Migrate();

//     //Seed roles and admin user
//     IdentitySeedData.Seed(scope.ServiceProvider).Wait();
// }