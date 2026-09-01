using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageAccessAndPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthlyPrice",
                table: "SubscriptionPackages",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "AccessType",
                table: "SubscriptionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "SubscriptionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "SubscriptionPackages");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SubscriptionPackages",
                newName: "MonthlyPrice");
        }
    }
}
