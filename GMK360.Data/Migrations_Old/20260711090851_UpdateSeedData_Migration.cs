using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionUrl",
                value: "/Account/Register?referralCode=ILK3UYE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionUrl",
                value: "/Campaigns/First3");
        }
    }
}
