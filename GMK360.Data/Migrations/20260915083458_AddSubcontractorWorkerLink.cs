using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcontractorWorkerLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubcontractorContactId",
                table: "AgencyWorkers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyWorkers_SubcontractorContactId",
                table: "AgencyWorkers",
                column: "SubcontractorContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgencyWorkers_AgencyPhonebooks_SubcontractorContactId",
                table: "AgencyWorkers",
                column: "SubcontractorContactId",
                principalTable: "AgencyPhonebooks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgencyWorkers_AgencyPhonebooks_SubcontractorContactId",
                table: "AgencyWorkers");

            migrationBuilder.DropIndex(
                name: "IX_AgencyWorkers_SubcontractorContactId",
                table: "AgencyWorkers");

            migrationBuilder.DropColumn(
                name: "SubcontractorContactId",
                table: "AgencyWorkers");
        }
    }
}
