using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentBuildingHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentBuildingId",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ParentBuildingId",
                table: "Buildings",
                column: "ParentBuildingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Buildings_ParentBuildingId",
                table: "Buildings",
                column: "ParentBuildingId",
                principalTable: "Buildings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Buildings_ParentBuildingId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_ParentBuildingId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ParentBuildingId",
                table: "Buildings");
        }
    }
}
