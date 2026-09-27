using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectExpectations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyTimesheets_PhaseTasks_PhaseTaskId",
                table: "DailyTimesheets");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyTimesheets_ProjectPhases_ProjectPhaseId",
                table: "DailyTimesheets");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_PhaseTasks_PhaseTaskId",
                table: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "PhaseApprovals");

            migrationBuilder.DropTable(
                name: "PhaseMessages");

            migrationBuilder.DropTable(
                name: "PhaseTaskTimesheet");

            migrationBuilder.DropTable(
                name: "PhaseWorkerDemands");

            migrationBuilder.DropTable(
                name: "TaskCosts");

            migrationBuilder.DropTable(
                name: "TaskDocuments");

            migrationBuilder.DropTable(
                name: "TaskMessages");

            migrationBuilder.DropTable(
                name: "PhaseTasks");

            migrationBuilder.DropTable(
                name: "ProjectPhases");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_PhaseTaskId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_DailyTimesheets_PhaseTaskId",
                table: "DailyTimesheets");

            migrationBuilder.DropIndex(
                name: "IX_DailyTimesheets_ProjectPhaseId",
                table: "DailyTimesheets");

            migrationBuilder.DropColumn(
                name: "PhaseTaskId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "PhaseTaskId",
                table: "DailyTimesheets");

            migrationBuilder.DropColumn(
                name: "ProjectPhaseId",
                table: "DailyTimesheets");

            migrationBuilder.DropColumn(
                name: "BaseType",
                table: "CostCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageFlatSalePrice",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BuildingAge",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EskiBinaOturumAlani",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedTotalShopRevenue",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BuildingAge",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationStatus",
                table: "B2bCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageFlatSalePrice",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "BuildingAge",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "EskiBinaOturumAlani",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ExpectedTotalShopRevenue",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "BuildingAge",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "InvitationStatus",
                table: "B2bCompanies");

            migrationBuilder.AddColumn<int>(
                name: "PhaseTaskId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhaseTaskId",
                table: "DailyTimesheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectPhaseId",
                table: "DailyTimesheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BaseType",
                table: "CostCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProjectPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiredApprovals = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectPhases_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PhaseMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhaseMessages_ProjectPhases_ProjectPhaseId",
                        column: x => x.ProjectPhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseTasks_ProjectPhases_ProjectPhaseId",
                        column: x => x.ProjectPhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseWorkerDemands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RequiredProfession = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseWorkerDemands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseWorkerDemands_ProjectPhases_ProjectPhaseId",
                        column: x => x.ProjectPhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseTaskTimesheet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseTaskId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyWage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FoodExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HoursWorked = table.Column<double>(type: "float", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TravelExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkerName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseTaskTimesheet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhaseTaskTimesheet_PhaseTasks_PhaseTaskId",
                        column: x => x.PhaseTaskId,
                        principalTable: "PhaseTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CostCategoryId = table.Column<int>(type: "int", nullable: false),
                    PhaseTaskId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsUrgentMissingOrder = table.Column<bool>(type: "bit", nullable: false),
                    LinkedB2BQuoteRequestId = table.Column<int>(type: "int", nullable: true),
                    LinkedSystemMeetingId = table.Column<int>(type: "int", nullable: true),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCosts_CostCategories_CostCategoryId",
                        column: x => x.CostCategoryId,
                        principalTable: "CostCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCosts_PhaseTasks_PhaseTaskId",
                        column: x => x.PhaseTaskId,
                        principalTable: "PhaseTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseTaskId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskDocuments_PhaseTasks_PhaseTaskId",
                        column: x => x.PhaseTaskId,
                        principalTable: "PhaseTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseTaskId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "IX_InventoryTransactions_PhaseTaskId",
                table: "InventoryTransactions",
                column: "PhaseTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTimesheets_PhaseTaskId",
                table: "DailyTimesheets",
                column: "PhaseTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTimesheets_ProjectPhaseId",
                table: "DailyTimesheets",
                column: "ProjectPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseApprovals_ProjectPhaseId",
                table: "PhaseApprovals",
                column: "ProjectPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseMessages_ProjectPhaseId",
                table: "PhaseMessages",
                column: "ProjectPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseMessages_SenderUserId",
                table: "PhaseMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseTasks_ProjectPhaseId",
                table: "PhaseTasks",
                column: "ProjectPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseTaskTimesheet_PhaseTaskId",
                table: "PhaseTaskTimesheet",
                column: "PhaseTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseWorkerDemands_ProjectPhaseId",
                table: "PhaseWorkerDemands",
                column: "ProjectPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPhases_ConstructionProjectId",
                table: "ProjectPhases",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCosts_CostCategoryId",
                table: "TaskCosts",
                column: "CostCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCosts_PhaseTaskId",
                table: "TaskCosts",
                column: "PhaseTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocuments_PhaseTaskId",
                table: "TaskDocuments",
                column: "PhaseTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMessages_PhaseTaskId",
                table: "TaskMessages",
                column: "PhaseTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTimesheets_PhaseTasks_PhaseTaskId",
                table: "DailyTimesheets",
                column: "PhaseTaskId",
                principalTable: "PhaseTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTimesheets_ProjectPhases_ProjectPhaseId",
                table: "DailyTimesheets",
                column: "ProjectPhaseId",
                principalTable: "ProjectPhases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_PhaseTasks_PhaseTaskId",
                table: "InventoryTransactions",
                column: "PhaseTaskId",
                principalTable: "PhaseTasks",
                principalColumn: "Id");
        }
    }
}
