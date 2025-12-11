using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using Npgsql.EntityFrameworkCore;
using EaziLease.Models;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();



builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireRole("Admin")
    .Build();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    //Seed roles + admin user
    using var scope = app.Services.CreateScope();
    await IdentitySeedData.Seed(scope.ServiceProvider);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

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