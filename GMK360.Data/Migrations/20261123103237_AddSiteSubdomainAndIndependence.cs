using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSubdomainAndIndependence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIndependentSite",
                table: "HousingComplexes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "HousingComplexes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "Buildings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIndependentSite",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "Buildings");
        }
    }
}
