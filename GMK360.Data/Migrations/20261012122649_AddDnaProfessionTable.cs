using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDnaProfessionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DnaTemplateQuestions_WorkGroupDefinitions_DefaultWorkGroupDefinitionId",
                table: "DnaTemplateQuestions");

            migrationBuilder.RenameColumn(
                name: "DefaultWorkGroupDefinitionId",
                table: "DnaTemplateQuestions",
                newName: "DnaProfessionId");

            migrationBuilder.RenameIndex(
                name: "IX_DnaTemplateQuestions_DefaultWorkGroupDefinitionId",
                table: "DnaTemplateQuestions",
                newName: "IX_DnaTemplateQuestions_DnaProfessionId");

            migrationBuilder.AddColumn<string>(
                name: "OptionSource",
                table: "DnaTemplateQuestions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DnaProfessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnaProfessions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DnaTemplateQuestions_DnaProfessions_DnaProfessionId",
                table: "DnaTemplateQuestions",
                column: "DnaProfessionId",
                principalTable: "DnaProfessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DnaTemplateQuestions_DnaProfessions_DnaProfessionId",
                table: "DnaTemplateQuestions");

            migrationBuilder.DropTable(
                name: "DnaProfessions");

            migrationBuilder.DropColumn(
                name: "OptionSource",
                table: "DnaTemplateQuestions");

            migrationBuilder.RenameColumn(
                name: "DnaProfessionId",
                table: "DnaTemplateQuestions",
                newName: "DefaultWorkGroupDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_DnaTemplateQuestions_DnaProfessionId",
                table: "DnaTemplateQuestions",
                newName: "IX_DnaTemplateQuestions_DefaultWorkGroupDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DnaTemplateQuestions_WorkGroupDefinitions_DefaultWorkGroupDefinitionId",
                table: "DnaTemplateQuestions",
                column: "DefaultWorkGroupDefinitionId",
                principalTable: "WorkGroupDefinitions",
                principalColumn: "Id");
        }
    }
}
