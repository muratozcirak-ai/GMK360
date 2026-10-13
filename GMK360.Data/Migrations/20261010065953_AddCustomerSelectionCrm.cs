using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSelectionCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerUnitId",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ElevationId = table.Column<int>(type: "int", nullable: false),
                    UnitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetAreaSquareMeters = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerUnitId = table.Column<int>(type: "int", nullable: false),
                    SelectionCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardMasterMaterialId = table.Column<int>(type: "int", nullable: false),
                    SelectedMasterMaterialId = table.Column<int>(type: "int", nullable: false),
                    UpgradePriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsApprovedByCustomer = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSelections_CustomerUnits_CustomerUnitId",
                        column: x => x.CustomerUnitId,
                        principalTable: "CustomerUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerSelections_MasterMaterials_SelectedMasterMaterialId",
                        column: x => x.SelectedMasterMaterialId,
                        principalTable: "MasterMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerSelections_MasterMaterials_StandardMasterMaterialId",
                        column: x => x.StandardMasterMaterialId,
                        principalTable: "MasterMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSelections_CustomerUnitId",
                table: "CustomerSelections",
                column: "CustomerUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSelections_SelectedMasterMaterialId",
                table: "CustomerSelections",
                column: "SelectedMasterMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSelections_StandardMasterMaterialId",
                table: "CustomerSelections",
                column: "StandardMasterMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerSelections");

            migrationBuilder.DropTable(
                name: "CustomerUnits");

            migrationBuilder.DropColumn(
                name: "CustomerUnitId",
                table: "ProjectTasks");
        }
    }
}
