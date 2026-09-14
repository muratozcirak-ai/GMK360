using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyPhonebook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConstructionProjects");

            migrationBuilder.AlterColumn<string>(
                name: "SenderUserId",
                table: "PhaseMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "ConstructionProjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "AgencyPhonebooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsRegistered = table.Column<bool>(type: "bit", nullable: false),
                    LinkedUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyPhonebooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyPhonebooks_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhaseMessages_SenderUserId",
                table: "PhaseMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyPhonebooks_AgencyId",
                table: "AgencyPhonebooks",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhaseMessages_AspNetUsers_SenderUserId",
                table: "PhaseMessages",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhaseMessages_AspNetUsers_SenderUserId",
                table: "PhaseMessages");

            migrationBuilder.DropTable(
                name: "AgencyPhonebooks");

            migrationBuilder.DropIndex(
                name: "IX_PhaseMessages_SenderUserId",
                table: "PhaseMessages");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ConstructionProjects");

            migrationBuilder.AlterColumn<string>(
                name: "SenderUserId",
                table: "PhaseMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ConstructionProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
