using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using Npgsql.EntityFrameworkCore;
using EaziLease.Models;
using Microsoft.AspNetCore.Authorization;
using EaziLease.Services;
using EaziLease.Extensions;
using Microsoft.AspNetCore.Http;
using EaziLease.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();

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



builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
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