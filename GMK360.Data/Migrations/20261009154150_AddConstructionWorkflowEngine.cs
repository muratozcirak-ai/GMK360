using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionWorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectElevations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ElevationLevel = table.Column<int>(type: "int", nullable: false),
                    IsBasement = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectElevations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectElevations_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsGatekeeper = table.Column<bool>(type: "bit", nullable: false),
                    WaitDays = table.Column<int>(type: "int", nullable: false),
                    ExperienceNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTimesheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandbyReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DailyWage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTimesheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ElevationId = table.Column<int>(type: "int", nullable: true),
                    TaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsExtraWork = table.Column<bool>(type: "bit", nullable: false),
                    CostCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPaused = table.Column<bool>(type: "bit", nullable: false),
                    PauseReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    SubcontractorId = table.Column<int>(type: "int", nullable: true),
                    ProgressPaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiredCustomerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTasks_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTasks_ProjectElevations_ElevationId",
                        column: x => x.ElevationId,
                        principalTable: "ProjectElevations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectElevations_ProjectId",
                table: "ProjectElevations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ElevationId",
                table: "ProjectTasks",
                column: "ElevationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId",
                table: "ProjectTasks",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTasks");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.DropTable(
                name: "WorkflowTimesheets");

            migrationBuilder.DropTable(
                name: "ProjectElevations");
        }
    }
}
