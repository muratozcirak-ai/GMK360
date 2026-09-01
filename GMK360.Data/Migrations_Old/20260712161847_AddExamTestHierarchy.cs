using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamTestHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresTextInput",
                table: "DefinitionValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMultiSelect",
                table: "DefinitionCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "DefinitionCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefinitionCategories_ParentCategoryId",
                table: "DefinitionCategories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DefinitionCategories_DefinitionCategories_ParentCategoryId",
                table: "DefinitionCategories",
                column: "ParentCategoryId",
                principalTable: "DefinitionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DefinitionCategories_DefinitionCategories_ParentCategoryId",
                table: "DefinitionCategories");

            migrationBuilder.DropIndex(
                name: "IX_DefinitionCategories_ParentCategoryId",
                table: "DefinitionCategories");

            migrationBuilder.DropColumn(
                name: "RequiresTextInput",
                table: "DefinitionValues");

            migrationBuilder.DropColumn(
                name: "IsMultiSelect",
                table: "DefinitionCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "DefinitionCategories");
        }
    }
}
