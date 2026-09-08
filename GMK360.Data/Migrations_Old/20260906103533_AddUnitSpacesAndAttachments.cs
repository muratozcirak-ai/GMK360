using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitSpacesAndAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomizable",
                table: "BuildingUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentUnitId",
                table: "BuildingUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SpaceFixtures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitSpaceId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardBrand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StandardModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaintenanceNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsCustomizable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceFixtures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceFixtures_UnitSpaces_UnitSpaceId",
                        column: x => x.UnitSpaceId,
                        principalTable: "UnitSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaceMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitSpaceId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaceMeasurements_UnitSpaces_UnitSpaceId",
                        column: x => x.UnitSpaceId,
                        principalTable: "UnitSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpaceFixtureId = table.Column<int>(type: "int", nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceDifference = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsStandard = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialOptions_SpaceFixtures_SpaceFixtureId",
                        column: x => x.SpaceFixtureId,
                        principalTable: "SpaceFixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_ParentUnitId",
                table: "BuildingUnits",
                column: "ParentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialOptions_SpaceFixtureId",
                table: "MaterialOptions",
                column: "SpaceFixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceFixtures_UnitSpaceId",
                table: "SpaceFixtures",
                column: "UnitSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceMeasurements_UnitSpaceId",
                table: "SpaceMeasurements",
                column: "UnitSpaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUnits_BuildingUnits_ParentUnitId",
                table: "BuildingUnits",
                column: "ParentUnitId",
                principalTable: "BuildingUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUnits_BuildingUnits_ParentUnitId",
                table: "BuildingUnits");

            migrationBuilder.DropTable(
                name: "MaterialOptions");

            migrationBuilder.DropTable(
                name: "SpaceMeasurements");

            migrationBuilder.DropTable(
                name: "SpaceFixtures");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_ParentUnitId",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "IsCustomizable",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "ParentUnitId",
                table: "BuildingUnits");
        }
    }
}
