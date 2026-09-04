using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitTemplateId",
                table: "BuildingUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnitTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomLayout = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitTemplates_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitTemplateSpaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitTemplateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SquareMeters = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTemplateSpaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitTemplateSpaces_UnitTemplates_UnitTemplateId",
                        column: x => x.UnitTemplateId,
                        principalTable: "UnitTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_UnitTemplateId",
                table: "BuildingUnits",
                column: "UnitTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTemplates_ConstructionProjectId",
                table: "UnitTemplates",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTemplateSpaces_UnitTemplateId",
                table: "UnitTemplateSpaces",
                column: "UnitTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUnits_UnitTemplates_UnitTemplateId",
                table: "BuildingUnits",
                column: "UnitTemplateId",
                principalTable: "UnitTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUnits_UnitTemplates_UnitTemplateId",
                table: "BuildingUnits");

            migrationBuilder.DropTable(
                name: "UnitTemplateSpaces");

            migrationBuilder.DropTable(
                name: "UnitTemplates");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_UnitTemplateId",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "UnitTemplateId",
                table: "BuildingUnits");
        }
    }
}
