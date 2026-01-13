using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageAndDecimalMileage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextMaitenanceMileage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ScheduleMileage",
                table: "VehicleMaintenance");

            migrationBuilder.RenameColumn(
                name: "GarageName",
                table: "VehicleMaintenance",
                newName: "GarageId");

            migrationBuilder.AlterColumn<decimal>(
                name: "OdometerReading",
                table: "Vehicles",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NextMaintenanceMileage",
                table: "Vehicles",
                type: "numeric",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MileageAtService",
                table: "VehicleMaintenance",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScheduledMileage",
                table: "VehicleMaintenance",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Garages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Garages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMaintenance_GarageId",
                table: "VehicleMaintenance",
                column: "GarageId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleMaintenance_Garages_GarageId",
                table: "VehicleMaintenance",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleMaintenance_Garages_GarageId",
                table: "VehicleMaintenance");

            migrationBuilder.DropTable(
                name: "Garages");

            migrationBuilder.DropIndex(
                name: "IX_VehicleMaintenance_GarageId",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceMileage",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ScheduledMileage",
                table: "VehicleMaintenance");

            migrationBuilder.RenameColumn(
                name: "GarageId",
                table: "VehicleMaintenance",
                newName: "GarageName");

            migrationBuilder.AlterColumn<int>(
                name: "OdometerReading",
                table: "Vehicles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaitenanceMileage",
                table: "Vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MileageAtService",
                table: "VehicleMaintenance",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleMileage",
                table: "VehicleMaintenance",
                type: "integer",
                nullable: true);
        }
    }
}
