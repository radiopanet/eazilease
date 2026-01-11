using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddFutureMaintenanceScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaintenanceIntervalKm",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceIntervalMonths",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Vehicles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaitenanceMileage",
                table: "Vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFutureScheduled",
                table: "VehicleMaintenance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleMileage",
                table: "VehicleMaintenance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "VehicleMaintenance",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehicleMaintenance",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintenanceIntervalKm",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "MaintenanceIntervalMonths",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NextMaitenanceMileage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsFutureScheduled",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "ScheduleMileage",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleMaintenance");
        }
    }
}
