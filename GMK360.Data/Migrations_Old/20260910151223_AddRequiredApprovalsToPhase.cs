using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredApprovalsToPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredApprovals",
                table: "ProjectPhases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PhaseApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseApprovals_ProjectPhases_ProjectPhaseId",
                        column: x => x.ProjectPhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhaseApprovals_ProjectPhaseId",
                table: "PhaseApprovals",
                column: "ProjectPhaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhaseApprovals");

            migrationBuilder.DropColumn(
                name: "RequiredApprovals",
                table: "ProjectPhases");
        }
    }
}
