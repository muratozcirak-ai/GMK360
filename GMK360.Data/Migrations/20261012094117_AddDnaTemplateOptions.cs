using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDnaTemplateOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectDnaTemplateOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkGroupDefinitionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDnaTemplateOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDnaTemplateOptions_WorkGroupDefinitions_WorkGroupDefinitionId",
                        column: x => x.WorkGroupDefinitionId,
                        principalTable: "WorkGroupDefinitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDnaTemplateOptions_WorkGroupDefinitionId",
                table: "ProjectDnaTemplateOptions",
                column: "WorkGroupDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectDnaTemplateOptions");
        }
    }
}
