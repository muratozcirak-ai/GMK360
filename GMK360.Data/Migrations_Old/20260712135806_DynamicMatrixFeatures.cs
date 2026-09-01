using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class DynamicMatrixFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "PropertyFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "PropertyFeatures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedSubOptions",
                table: "PropertyFeatures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasCount",
                table: "DefinitionValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubOptions",
                table: "DefinitionValues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalFloors",
                table: "Complexes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "PropertyFeatures");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "PropertyFeatures");

            migrationBuilder.DropColumn(
                name: "SelectedSubOptions",
                table: "PropertyFeatures");

            migrationBuilder.DropColumn(
                name: "HasCount",
                table: "DefinitionValues");

            migrationBuilder.DropColumn(
                name: "SubOptions",
                table: "DefinitionValues");

            migrationBuilder.DropColumn(
                name: "TotalFloors",
                table: "Complexes");
        }
    }
}
