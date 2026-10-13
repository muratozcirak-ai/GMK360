using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentUsageLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUsageLogs_CompanyEquipments_CompanyEquipmentId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUsageLogs_ProjectTasks_ProjectTaskId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropTable(
                name: "MaterialQuotes");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentUsageLogs_ProjectTaskId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "IsApprovedByChief",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "OperatorName",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "TotalCalculatedCost",
                table: "EquipmentUsageLogs");

            migrationBuilder.RenameColumn(
                name: "UsedQuantity",
                table: "EquipmentUsageLogs",
                newName: "TotalCost");

            migrationBuilder.RenameColumn(
                name: "UsageDate",
                table: "EquipmentUsageLogs",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "EquipmentUsageLogs",
                newName: "Duration");

            migrationBuilder.AddColumn<int>(
                name: "ConstructionProjectId",
                table: "EquipmentUsageLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "EquipmentUsageLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "EquipmentUsageLogs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "EquipmentUsageLogs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsageType",
                table: "EquipmentUsageLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentUsageLogs_ConstructionProjectId",
                table: "EquipmentUsageLogs",
                column: "ConstructionProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUsageLogs_CompanyEquipments_CompanyEquipmentId",
                table: "EquipmentUsageLogs",
                column: "CompanyEquipmentId",
                principalTable: "CompanyEquipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUsageLogs_ConstructionProjects_ConstructionProjectId",
                table: "EquipmentUsageLogs",
                column: "ConstructionProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUsageLogs_CompanyEquipments_CompanyEquipmentId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentUsageLogs_ConstructionProjects_ConstructionProjectId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentUsageLogs_ConstructionProjectId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "ConstructionProjectId",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "EquipmentUsageLogs");

            migrationBuilder.DropColumn(
                name: "UsageType",
                table: "EquipmentUsageLogs");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "EquipmentUsageLogs",
                newName: "UsedQuantity");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "EquipmentUsageLogs",
                newName: "UsageDate");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "EquipmentUsageLogs",
                newName: "ProjectId");

            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedByChief",
                table: "EquipmentUsageLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                table: "EquipmentUsageLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectTaskId",
                table: "EquipmentUsageLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCalculatedCost",
                table: "EquipmentUsageLogs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectMaterialDemandId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsSelectedBySystem = table.Column<bool>(type: "bit", nullable: false),
                    MaturityDays = table.Column<int>(type: "int", nullable: false),
                    OfferedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialQuotes_ProjectMaterialDemands_ProjectMaterialDemandId",
                        column: x => x.ProjectMaterialDemandId,
                        principalTable: "ProjectMaterialDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialQuotes_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentUsageLogs_ProjectTaskId",
                table: "EquipmentUsageLogs",
                column: "ProjectTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialQuotes_ProjectMaterialDemandId",
                table: "MaterialQuotes",
                column: "ProjectMaterialDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialQuotes_SupplierId",
                table: "MaterialQuotes",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUsageLogs_CompanyEquipments_CompanyEquipmentId",
                table: "EquipmentUsageLogs",
                column: "CompanyEquipmentId",
                principalTable: "CompanyEquipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentUsageLogs_ProjectTasks_ProjectTaskId",
                table: "EquipmentUsageLogs",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");
        }
    }
}
