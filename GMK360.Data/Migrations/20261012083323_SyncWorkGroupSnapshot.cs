using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncWorkGroupSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinition_WorkGroupDefinitionId",
                table: "ProjectDnaDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkGroupDefinition",
                table: "WorkGroupDefinition");

            migrationBuilder.RenameTable(
                name: "WorkGroupDefinition",
                newName: "WorkGroupDefinitions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkGroupDefinitions",
                table: "WorkGroupDefinitions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                column: "WorkGroupDefinitionId",
                principalTable: "WorkGroupDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinitions_WorkGroupDefinitionId",
                table: "ProjectDnaDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkGroupDefinitions",
                table: "WorkGroupDefinitions");

            migrationBuilder.RenameTable(
                name: "WorkGroupDefinitions",
                newName: "WorkGroupDefinition");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkGroupDefinition",
                table: "WorkGroupDefinition",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinition_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                column: "WorkGroupDefinitionId",
                principalTable: "WorkGroupDefinition",
                principalColumn: "Id");
        }
    }
}
