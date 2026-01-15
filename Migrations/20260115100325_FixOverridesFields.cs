using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class FixOverridesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OverrideNotes",
                table: "Vehicles",
                newName: "OverrideRateNotes");

            migrationBuilder.RenameColumn(
                name: "OverrideHighMaintenanceBlock",
                table: "Vehicles",
                newName: "OverrideLeasingBlock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OverrideRateNotes",
                table: "Vehicles",
                newName: "OverrideNotes");

            migrationBuilder.RenameColumn(
                name: "OverrideLeasingBlock",
                table: "Vehicles",
                newName: "OverrideHighMaintenanceBlock");
        }
    }
}
