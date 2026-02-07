using EaziLease.Domain.Entities;
using EaziLease.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EaziLease.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            context.Database.Migrate(); // ensures DB exists

            if (context.Suppliers.Any()) return; // already seeded

            // 1. Suppliers
            var suppliers = new[]
            {
                new Supplier { Name = "AutoFleet Imports",      ContactPerson = "Sarah Naidoo",    ContactEmail = "sarah@autofleet.co.za", ContactPhone = "011 234 5678", City = "Johannesburg", Country="South Africa" },
                new Supplier { Name = "Cape Vehicle Wholesalers", ContactPerson = "Johan van Wyk", ContactEmail = "johan@cvw.co.za",     ContactPhone = "021 876 5432", City = "Cape Town", Country="South Africa" },
                new Supplier { Name = "Durban Motor Traders",   ContactPerson = "Priya Singh",     ContactEmail = "priya@dmt.co.za",       ContactPhone = "031 456 7890", City = "Durban", Country="South Africa" },
                new Supplier { Name = "Pretoria Car Brokers",   ContactPerson = "Marcus Botha",    ContactEmail = "marcus@pcb.co.za",      ContactPhone = "012 345 6789", City = "Pretoria", Country="South Africa" },
                new Supplier { Name = "National Fleet Direct",  ContactPerson = "Linda Mokoena",   ContactEmail = "linda@nfd.co.za",       ContactPhone = "087 123 4567", City = "Sandton", Country="South Africa" }
            };
            context.Suppliers.AddRange(suppliers);
            await context.SaveChangesAsync();

            // 2. Branches
            var branches = new[]
            {
                new Branch { Name = "Johannesburg Central", Code = "JHB001", City = "Sandton",     Address = "100 West Street, Sandton" },
                new Branch { Name = "Cape Town Waterfront", Code = "CPT001", City = "Cape Town",  Address = "V&A Waterfront" },
                new Branch { Name = "Durban Umhlanga",      Code = "DBN001", City = "Umhlanga",   Address = "1 Palm Boulevard" },
                new Branch { Name = "Pretoria Menlyn",      Code = "PTA001", City = "Pretoria",   Address = "Menlyn Maine" },
                new Branch { Name = "Bloemfontein Depot",   Code = "BFN001", City = "Bloemfontein", Address = "Nelson Mandela Drive" }
            };
            context.Branches.AddRange(branches);
            await context.SaveChangesAsync();

            // 3. Clients
            var clients = new[]
            {
                new Client { CompanyName = "Vodacom Business",      ContactPerson = "Thabo Mokoena",   ContactEmail = "thabo@vodacom.co.za", CreditLimit=150000m, ContactPhone = "082 111 2222" },
                new Client { CompanyName = "MTN Corporate",         ContactPerson = "Lerato Pillay",    ContactEmail = "lerato@mtn.co.za", CreditLimit=150000m,   ContactPhone = "083 333 4444" },
                new Client { CompanyName = "Discovery Health",      ContactPerson = "Anita de Beer",   ContactEmail = "anita@discovery.co.za", CreditLimit=150000m, ContactPhone = "084 555 6666" },
                new Client { CompanyName = "Bidvest Facilities",    ContactPerson = "Craig Williams",  ContactEmail = "craig@bidvest.co.za", CreditLimit=150000m, ContactPhone = "076 777 8888" },
                new Client { CompanyName = "Standard Bank Fleet",   ContactPerson = "Nomsa Zungu",     ContactEmail = "nomsa@standardbank.co.za", CreditLimit=150000m, ContactPhone = "079 999 0000" }
            };
            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();

            // 4. Drivers
            var drivers = new[]
            {
                new Driver { FirstName = "Sipho", LastName = "Mthembu", LicenseNumber = "D12345678", LicenseExpiry = new DateTime(2027, 12, 31), Phone = "072 111 2233", Email = "sipho@eazilease.co.za", IsActive = true },
                new Driver { FirstName = "Fatima", LastName = "Patel",   LicenseNumber = "D87654321", LicenseExpiry = new DateTime(2026, 10, 15), Phone = "083 444 5566", Email = "fatima@eazilease.co.za", IsActive = true },
                new Driver { FirstName = "Jaco",   LastName = "van Zyl", LicenseNumber = "D11223344", LicenseExpiry = new DateTime(2028, 03, 20), Phone = "081 777 8899", IsActive = true },
                new Driver { FirstName = "Naledi", LastName = "Dlamini", LicenseNumber = "D55667788", LicenseExpiry = new DateTime(2027, 08, 08), Phone = "060 999 0001", IsActive = true },
                new Driver { FirstName = "Pieter", LastName = "Nel",     LicenseNumber = "D99887766", LicenseExpiry = new DateTime(2025, 11, 30), Phone = "082 555 6677", IsActive = true }
            };
            context.Drivers.AddRange(drivers);
            await context.SaveChangesAsync();

            // 5. Vehicles (15 vehicles covering different manufacturers)
            var vehicles = new[]
            {
                // Toyota (5)
                new Vehicle { VIN = "JT123456789012345", RegistrationNumber = "CA 123-456", Manufacturer = "Toyota", Model = "Hilux 2.8 GD-6", Year = 2024, Color = "White", FuelType = FuelType.Diesel, Transmission = TransmissionType.Automatic, DailyRate = 850, PurchasePrice = 685000, PurchaseDate = new DateTime(2024, 1, 15), SupplierId = suppliers[0].Id, BranchId = branches[0].Id, Status = VehicleStatus.Available, OdometerReading=25000 },
                new Vehicle { VIN = "JT223456789012346", RegistrationNumber = "CA 234-567", Manufacturer = "Toyota", Model = "Corolla Cross", Year = 2024, Color = "Silver", FuelType = FuelType.Petrol, Transmission = TransmissionType.Manual, DailyRate = 620, PurchasePrice = 485000, PurchaseDate = new DateTime(2024, 2, 20), SupplierId = suppliers[0].Id, BranchId = branches[1].Id, Status = VehicleStatus.Leased, OdometerReading=31000 },

                // Volkswagen (4)
                new Vehicle { VIN = "WVWZZZ12345678901", RegistrationNumber = "CA 345-678", Manufacturer = "Volkswagen", Model = "Polo Vivo", Year = 2023, Color = "Blue", FuelType = FuelType.Petrol, Transmission = TransmissionType.Manual, DailyRate = 420, PurchasePrice = 285000, PurchaseDate = new DateTime(2023, 11, 10), SupplierId = suppliers[1].Id, BranchId = branches[0].Id, Status = VehicleStatus.Available, OdometerReading=15000  },
                new Vehicle { VIN = "WVWZZZ12345678902", RegistrationNumber = "CA 456-789", Manufacturer = "Volkswagen", Model = "T-Cross", Year = 2024, Color = "Red", FuelType = FuelType.Petrol, Transmission = TransmissionType.Automatic, DailyRate = 580, PurchasePrice = 425000, PurchaseDate = new DateTime(2024, 3, 5), SupplierId = suppliers[1].Id, BranchId = branches[2].Id, Status = VehicleStatus.Leased },

                // Ford (3)
                new Vehicle { VIN = "1FMCU0G1234567890", RegistrationNumber = "CA 567-890", Manufacturer = "Ford", Model = "Ranger Raptor", Year = 2024, Color = "Orange", FuelType = FuelType.Diesel, Transmission = TransmissionType.Automatic, DailyRate = 1150, PurchasePrice = 980000, PurchaseDate = new DateTime(2024, 4, 12), SupplierId = suppliers[2].Id, BranchId = branches[0].Id, Status = VehicleStatus.InMaintenance,  OdometerReading=250000  },

                // Hyundai (3)
                new Vehicle { VIN = "KMH123456789012345", RegistrationNumber = "CA 678-901", Manufacturer = "Hyundai", Model = "Tucson", Year = 2024, Color = "Grey", FuelType = FuelType.Petrol, Transmission = TransmissionType.Automatic, DailyRate = 720, PurchasePrice = 560000, PurchaseDate = new DateTime(2024, 5, 18), SupplierId = suppliers[3].Id, BranchId = branches[3].Id, Status = VehicleStatus.Available, OdometerReading=125000  },

                // Isuzu (2)
                new Vehicle { VIN = "JA1234567890123456", RegistrationNumber = "CA 789-012", Manufacturer = "Isuzu", Model = "D-Max", Year = 2023, Color = "White", FuelType = FuelType.Diesel, Transmission = TransmissionType.Manual, DailyRate = 680, PurchasePrice = 520000, PurchaseDate = new DateTime(2023, 12, 1), SupplierId = suppliers[4].Id, BranchId = branches[4].Id, Status = VehicleStatus.Available, OdometerReading=95000  },
                new Vehicle { VIN = "KA1234567760123490", RegistrationNumber = "SWB 149 FS", Manufacturer = "Isuzu", Model = "KB-300", Year = 2024, Color = "Silver", FuelType = FuelType.Diesel, Transmission = TransmissionType.Automatic, DailyRate = 900, PurchasePrice = 810000, PurchaseDate = new DateTime(2024, 12, 1), SupplierId = suppliers[4].Id, BranchId = branches[1].Id, Status = VehicleStatus.Available, OdometerReading=195000  }

            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();

            var garages = new[]
            {
            new Garage{Id = Guid.NewGuid().ToString(), Name = "AutoFix Benoni", Address = "123 Main Rd", City = "Benoni", ContactPerson = "John Doe", Phone = "082 123 4567", Email = "info@autofix.co.za", IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "system" },
            new Garage{Id = Guid.NewGuid().ToString(), Name = "Botha & Deysel", Address = "23 Sterling Rd", City = "Vanderbijlpark", ContactPerson = "Deer Judine", Phone = "016 252 6589", Email = "info@band.co.za", IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "system" },
            new Garage{Id = Guid.NewGuid().ToString(), Name = "Synapsis Auto Repairs", Address = "12 Klassie Havenga Rd", City = "Sasolburg", ContactPerson = "Busi Dumani", Phone = "016 010 0125", Email = "info@synapsis.co.za", IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "system" },
            new Garage{Id = Guid.NewGuid().ToString(), Name = "VW Johannesburg", Address = "22 Sloane Ave", City = "Johannesburg", ContactPerson = "Thando Masutha", Phone = "011 123 4567", Email = "info@vwjhb.co.za", IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "system" },
            new Garage{Id = Guid.NewGuid().ToString(), Name = "Alpha Repairs", Address = "156 Main Rd", City = "Pretoria", ContactPerson = "Fannie Marie", Phone = "082 016 4567", Email = "info@alpharepairs.co.za", IsPreferred = true, CreatedAt = DateTime.UtcNow, CreatedBy = "system" }
            };
            context.Garages.AddRange(garages);
            await context.SaveChangesAsync();

        }
    }
}
