using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComplexEntityAndRemovePOI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyPOIDistances");

            migrationBuilder.DropColumn(
                name: "ComplexName",
                table: "Properties");

            migrationBuilder.AddColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Complexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complexes_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ComplexId",
                table: "Properties",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_Complexes_NeighborhoodId",
                table: "Complexes",
                column: "NeighborhoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "Complexes");

            migrationBuilder.DropIndex(
                name: "IX_Properties_ComplexId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ComplexId",
                table: "Properties");

            migrationBuilder.AddColumn<string>(
                name: "ComplexName",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PropertyPOIDistances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistanceText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PoiType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPOIDistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPOIDistances_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPOIDistances_PropertyId",
                table: "PropertyPOIDistances",
                column: "PropertyId");
        }
    }
}
