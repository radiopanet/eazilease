# EaziLease

EaziLease is a modern **fleet and vehicle leasing management system** designed for businesses to manage vehicles, leases, clients, suppliers, drivers, branches, and maintenance operations efficiently.

Built with **ASP.NET Core MVC**, **Entity Framework Core** (PostgreSQL), and a clean layered architecture, the system emphasizes security, auditability, and separation of concerns.

## Features

### Core Functionality
- Vehicle management (CRUD, status tracking: Available, Leased, InMaintenance, OutOfService)
- Client leasing & lease lifecycle (start, extend, end, penalties, billable maintenance)
- Driver assignment & return (with double-booking prevention and auto-return on lease/maintenance end)
- Maintenance scheduling & recording
  - Immediate, future scheduled, and historical/past records
  - Automatic status changes (InMaintenance), driver auto-return, and blocking
  - Financial impact tracking (billable to client, warranty, insurance claims)
- Rate override requests (with SuperAdmin approval flow)
- Credit limit enforcement per client (prevents over-leasing)
- Soft delete & audit logging for all major actions

### Security & Roles
- Regular Admin: Day-to-day operations (leasing, assignment, maintenance recording)
- SuperAdmin: Elevated privileges (user management, rate/lease overrides, approvals)

### Architecture Highlights
- Layered design: Controllers → Services → Entities
- Policy-based authorization (`RequireSuperAdmin`)
- Soft delete pattern (`IsDeleted`, `DeletedAt`, `DeletedBy`)
- Full audit trail via `AuditService`
- Decimal precision for financial & mileage fields

## Tech Stack

- **Backend**: ASP.NET Core 8.0 (MVC)
- **Database**: PostgreSQL + Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5 + Bootstrap Icons
- **Other**: MediatR (planned), Hangfire (planned for reminders)

## Project Structure
EaziLease/
├── Areas/
│   └── SuperAdmin/             # Elevated admin area (user mgmt, approvals, overrides)
├── Controllers/
│   ├── VehiclesController.cs   # Main operational controller (leasing, assignment, maintenance)
│   └── ...
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Entities/               # Domain models (Vehicle, Client, Lease, etc.)
│   ├── ViewModels/             # DTOs for forms & views
│   └── Enums.cs
├── Services/
│   ├── Interfaces/             # IService contracts
│   └── LeaseService.cs, MaintenanceService.cs, etc.
├── Views/
│   ├── Vehicles/
│   └── Dashboard/
└── wwwroot/                     # CSS, JS, lib


## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 15+ (or Docker)
- Visual Studio / VS Code


1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/EaziLease.git
   cd EaziLease

