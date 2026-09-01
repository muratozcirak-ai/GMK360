using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestDistributionAndSOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyModeActive",
                table: "ServiceProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveSmsNotifications",
                table: "ServiceProviders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReliabilityScore",
                table: "ServiceProviders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetCityId",
                table: "RenovationRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetDistrictIds",
                table: "RenovationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RenovationRequestInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RenovationRequestId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HasResponded = table.Column<bool>(type: "bit", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenovationRequestInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenovationRequestInvites_AspNetUsers_ServiceProviderId",
                        column: x => x.ServiceProviderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RenovationRequestInvites_RenovationRequests_RenovationRequestId",
                        column: x => x.RenovationRequestId,
                        principalTable: "RenovationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequestInvites_RenovationRequestId",
                table: "RenovationRequestInvites",
                column: "RenovationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequestInvites_ServiceProviderId",
                table: "RenovationRequestInvites",
                column: "ServiceProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenovationRequestInvites");

            migrationBuilder.DropColumn(
                name: "IsEmergencyModeActive",
                table: "ServiceProviders");

            migrationBuilder.DropColumn(
                name: "ReceiveSmsNotifications",
                table: "ServiceProviders");

            migrationBuilder.DropColumn(
                name: "ReliabilityScore",
                table: "ServiceProviders");

            migrationBuilder.DropColumn(
                name: "TargetCityId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "TargetDistrictIds",
                table: "RenovationRequests");
        }
    }
}
