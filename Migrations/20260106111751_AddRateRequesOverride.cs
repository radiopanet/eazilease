using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddRateRequesOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateOverrideRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    VehicleId = table.Column<string>(type: "text", nullable: false),
                    RequestedDailyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalDailyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedBy = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_RateOverrideRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RateOverrideRequests_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateOverrideRequests_VehicleId",
                table: "RateOverrideRequests",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateOverrideRequests");
        }
    }
}
