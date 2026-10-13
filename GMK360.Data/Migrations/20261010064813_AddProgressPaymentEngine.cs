using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressPaymentEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskProgressLogs_ConstructionTasks_ConstructionTaskId",
                table: "TaskProgressLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskProgressLogs_ConstructionTaskId",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "TaskProgressLogs");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                table: "TaskProgressLogs",
                newName: "ReportDate");

            migrationBuilder.RenameColumn(
                name: "ConstructionTaskId",
                table: "TaskProgressLogs",
                newName: "SubcontractorId");

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedEarnedValue",
                table: "TaskProgressLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletedQuantity",
                table: "TaskProgressLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedByChief",
                table: "TaskProgressLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProjectTaskId",
                table: "TaskProgressLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletedQuantity",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "TaskProgressLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionTaskId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskProgressLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskProgressLog_ConstructionTasks_ConstructionTaskId",
                        column: x => x.ConstructionTaskId,
                        principalTable: "ConstructionTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemSubcontractorContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorId = table.Column<int>(type: "int", nullable: false),
                    WorkItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemSubcontractorContracts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskProgressLog_ConstructionTaskId",
                table: "TaskProgressLog",
                column: "ConstructionTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskProgressLog");

            migrationBuilder.DropTable(
                name: "WorkItemSubcontractorContracts");

            migrationBuilder.DropColumn(
                name: "CalculatedEarnedValue",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "CompletedQuantity",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "IsApprovedByChief",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "TaskProgressLogs");

            migrationBuilder.DropColumn(
                name: "CompletedQuantity",
                table: "ProjectTasks");

            migrationBuilder.RenameColumn(
                name: "SubcontractorId",
                table: "TaskProgressLogs",
                newName: "ConstructionTaskId");

            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "TaskProgressLogs",
                newName: "LogDate");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TaskProgressLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "TaskProgressLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskProgressLogs_ConstructionTaskId",
                table: "TaskProgressLogs",
                column: "ConstructionTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskProgressLogs_ConstructionTasks_ConstructionTaskId",
                table: "TaskProgressLogs",
                column: "ConstructionTaskId",
                principalTable: "ConstructionTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
