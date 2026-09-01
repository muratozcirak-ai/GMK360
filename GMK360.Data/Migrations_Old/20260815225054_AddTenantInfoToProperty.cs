using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantInfoToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUtilitiesOnOwner",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LeaseContractFilePath",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantEmail",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantIdentityNumber",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantName",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantPhone",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUtilitiesOnOwner",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "LeaseContractFilePath",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TenantEmail",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TenantIdentityNumber",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TenantName",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TenantPhone",
                table: "Properties");
        }
    }
}
