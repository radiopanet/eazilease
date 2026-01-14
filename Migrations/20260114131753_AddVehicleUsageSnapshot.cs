using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleUsageSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicleUsageSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    VehicleId = table.Column<string>(type: "text", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaintenanceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalMaintenanceCost = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalKmDriven = table.Column<decimal>(type: "numeric", nullable: false),
                    CalculatedBy = table.Column<string>(type: "text", nullable: true),
                    TriggerEvent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicleUsageSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicleUsageSnapshots_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicleUsageSnapshots_VehicleId",
                table: "vehicleUsageSnapshots",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicleUsageSnapshots");
        }
    }
}
