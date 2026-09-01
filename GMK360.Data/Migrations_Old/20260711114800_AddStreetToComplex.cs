using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStreetToComplex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "Complexes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complexes_StreetId",
                table: "Complexes",
                column: "StreetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complexes_Streets_StreetId",
                table: "Complexes",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complexes_Streets_StreetId",
                table: "Complexes");

            migrationBuilder.DropIndex(
                name: "IX_Complexes_StreetId",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "Complexes");
        }
    }
}
