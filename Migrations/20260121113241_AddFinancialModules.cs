using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyFinancialSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPenalties = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalBillableMaintenance = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalNonBillableCosts = table.Column<decimal>(type: "numeric", nullable: false),
                    ActiveLeaseCount = table.Column<int>(type: "integer", nullable: false),
                    EndedLeaseCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CompanyFinancialSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaseFinacialSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LeaseId = table.Column<string>(type: "text", nullable: false),
                    TotalLeaseRevenue = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPenaltyFees = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalBillableMaitenance = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalculateBy = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_LeaseFinacialSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseFinacialSummaries_VehicleLeases_LeaseId",
                        column: x => x.LeaseId,
                        principalTable: "VehicleLeases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseFinacialSummaries_LeaseId",
                table: "LeaseFinacialSummaries",
                column: "LeaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyFinancialSnapshots");

            migrationBuilder.DropTable(
                name: "LeaseFinacialSummaries");
        }
    }
}
