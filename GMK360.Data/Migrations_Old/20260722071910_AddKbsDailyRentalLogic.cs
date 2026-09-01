using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKbsDailyRentalLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsKbsReported",
                table: "PropertyReservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KbsErrorMessage",
                table: "PropertyReservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "PropertyReservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "PropertyReservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PropertyReservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "GuestCheckInRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryGuest",
                table: "GuestCheckInRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "KbsFacilitySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EgmFacilityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EgmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KbsFacilitySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KbsFacilitySettings_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KbsFacilitySettings_ApplicationUserId",
                table: "KbsFacilitySettings",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KbsFacilitySettings");

            migrationBuilder.DropColumn(
                name: "IsKbsReported",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "KbsErrorMessage",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "GuestCheckInRecords");

            migrationBuilder.DropColumn(
                name: "IsPrimaryGuest",
                table: "GuestCheckInRecords");
        }
    }
}
