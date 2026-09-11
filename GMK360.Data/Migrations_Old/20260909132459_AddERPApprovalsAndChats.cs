using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddERPApprovalsAndChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "TaskCosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "TaskCosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "TaskCosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LinkedSystemMeetingId",
                table: "TaskCosts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseTaskId = table.Column<int>(type: "int", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskMessages_PhaseTasks_PhaseTaskId",
                        column: x => x.PhaseTaskId,
                        principalTable: "PhaseTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskMessages_PhaseTaskId",
                table: "TaskMessages",
                column: "PhaseTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskMessages");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "TaskCosts");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "TaskCosts");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "TaskCosts");

            migrationBuilder.DropColumn(
                name: "LinkedSystemMeetingId",
                table: "TaskCosts");
        }
    }
}
