using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyManagementAndRenovation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSmsNotifications",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRentTrackingCount",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "PropertyFinancialRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ManagementRole",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RenovationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BiddingType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenovationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenovationRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenovationRequests_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RenovationOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RenovationRequestId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<int>(type: "int", nullable: true),
                    ExternalProviderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenovationOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenovationOffers_RenovationRequests_RenovationRequestId",
                        column: x => x.RenovationRequestId,
                        principalTable: "RenovationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RenovationOffers_ServiceProviders_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "ServiceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenovationOffers_RenovationRequestId",
                table: "RenovationOffers",
                column: "RenovationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationOffers_ServiceProviderId",
                table: "RenovationOffers",
                column: "ServiceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_PropertyId",
                table: "RenovationRequests",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_UserId",
                table: "RenovationRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenovationOffers");

            migrationBuilder.DropTable(
                name: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "HasSmsNotifications",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "MaxRentTrackingCount",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "PropertyFinancialRecords");

            migrationBuilder.DropColumn(
                name: "ManagementRole",
                table: "Properties");
        }
    }
}
