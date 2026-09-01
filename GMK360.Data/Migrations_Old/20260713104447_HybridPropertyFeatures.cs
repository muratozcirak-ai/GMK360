using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class HybridPropertyFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BathroomCount",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingAge",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HeatingId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFurnished",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoomCount",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "DefinitionCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ComplexFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplexId = table.Column<int>(type: "int", nullable: false),
                    DefinitionValueId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: true),
                    SelectedSubOptions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplexFeatures_Complexes_ComplexId",
                        column: x => x.ComplexId,
                        principalTable: "Complexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplexFeatures_DefinitionValues_DefinitionValueId",
                        column: x => x.DefinitionValueId,
                        principalTable: "DefinitionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_HeatingId",
                table: "Properties",
                column: "HeatingId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexFeatures_ComplexId",
                table: "ComplexFeatures",
                column: "ComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexFeatures_DefinitionValueId",
                table: "ComplexFeatures",
                column: "DefinitionValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_DefinitionValues_HeatingId",
                table: "Properties",
                column: "HeatingId",
                principalTable: "DefinitionValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_DefinitionValues_HeatingId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "ComplexFeatures");

            migrationBuilder.DropIndex(
                name: "IX_Properties_HeatingId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BathroomCount",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BuildingAge",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "HeatingId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsFurnished",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "RoomCount",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "DefinitionCategories");
        }
    }
}
