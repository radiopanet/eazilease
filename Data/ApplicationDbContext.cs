using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EaziLease.Models;
namespace EaziLease.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //Add Models
    public DbSet<AuditLogs> AuditLogs => Set<AuditLogs>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleAssignment> VehicleAssignments => Set<VehicleAssignment>();
    public DbSet<VehicleLease> VehicleLeases => Set<VehicleLease>();
    public DbSet<VehicleMaintenance> VehicleMaintenance => Set<VehicleMaintenance>();
    public DbSet<RateOverrideRequest> RateOverrideRequests => Set<RateOverrideRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Unique constraints
        builder.Entity<Vehicle>()
            .HasIndex(v => v.VIN)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        builder.Entity<Supplier>()
            .HasIndex(s => s.Name);

        builder.Entity<Client>()
            .HasIndex(c => c.CompanyName);

        // One-to-many
        builder.Entity<Vehicle>()
            .HasOne(v => v.Supplier)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(v => v.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vehicle>()
            .HasOne(v => v.Branch)
            .WithMany(b => b.Vehicles)
            .HasForeignKey(v => v.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Current lease (one vehicle → one active lease)
        builder.Entity<Vehicle>()
            .HasOne(v => v.CurrentLease)
            .WithOne()
            .HasForeignKey<Vehicle>(v => v.CurrentLeaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Vehicle>()
            .HasOne(v => v.CurrentDriver)
            .WithOne(d => d.CurrentVehicle)
            .HasForeignKey<Driver>(d => d.CurrentVehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<VehicleLease>()
            .HasIndex(l => l.VehicleId)
            .HasFilter("\"ReturnDate\" IS NULL")
            .IsUnique();
    

        //Soft delete global query filter
        builder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Branch>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Client>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);

    }
}
