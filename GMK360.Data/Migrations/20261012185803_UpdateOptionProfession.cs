using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOptionProfession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DnaTemplateOptions_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "DnaTemplateOptions");

            migrationBuilder.RenameColumn(
                name: "WorkGroupDefinitionId",
                table: "DnaTemplateOptions",
                newName: "DnaProfessionId");

            migrationBuilder.RenameIndex(
                name: "IX_DnaTemplateOptions_WorkGroupDefinitionId",
                table: "DnaTemplateOptions",
                newName: "IX_DnaTemplateOptions_DnaProfessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DnaTemplateOptions_DnaProfessions_DnaProfessionId",
                table: "DnaTemplateOptions",
                column: "DnaProfessionId",
                principalTable: "DnaProfessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DnaTemplateOptions_DnaProfessions_DnaProfessionId",
                table: "DnaTemplateOptions");

            migrationBuilder.RenameColumn(
                name: "DnaProfessionId",
                table: "DnaTemplateOptions",
                newName: "WorkGroupDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_DnaTemplateOptions_DnaProfessionId",
                table: "DnaTemplateOptions",
                newName: "IX_DnaTemplateOptions_WorkGroupDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DnaTemplateOptions_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "DnaTemplateOptions",
                column: "WorkGroupDefinitionId",
                principalTable: "WorkGroupDefinitions",
                principalColumn: "Id");
        }
    }
}
