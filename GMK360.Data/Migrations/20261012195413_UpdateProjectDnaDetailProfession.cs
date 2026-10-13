using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectDnaDetailProfession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "ProjectDnaDetails");

            migrationBuilder.RenameColumn(
                name: "WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                newName: "DnaProfessionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDnaDetails_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                newName: "IX_ProjectDnaDetails_DnaProfessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDnaDetails_DnaProfessions_DnaProfessionId",
                table: "ProjectDnaDetails",
                column: "DnaProfessionId",
                principalTable: "DnaProfessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDnaDetails_DnaProfessions_DnaProfessionId",
                table: "ProjectDnaDetails");

            migrationBuilder.RenameColumn(
                name: "DnaProfessionId",
                table: "ProjectDnaDetails",
                newName: "WorkGroupDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDnaDetails_DnaProfessionId",
                table: "ProjectDnaDetails",
                newName: "IX_ProjectDnaDetails_WorkGroupDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                column: "WorkGroupDefinitionId",
                principalTable: "WorkGroupDefinitions",
                principalColumn: "Id");
        }
    }
}
