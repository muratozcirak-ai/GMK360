using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantUserIdToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantUserId",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TenantUserId",
                table: "Reservations",
                column: "TenantUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_TenantUserId",
                table: "Reservations",
                column: "TenantUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_TenantUserId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TenantUserId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TenantUserId",
                table: "Reservations");
        }
    }
}
