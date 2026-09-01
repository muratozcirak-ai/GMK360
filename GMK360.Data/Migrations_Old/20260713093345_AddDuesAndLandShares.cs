using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuesAndLandShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BlockNumber",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeedStatus",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Dues",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FromWhomId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrossArea",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NetArea",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParcelNumber",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ShareAreaSqm",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAgencyInfo",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubTypeId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalAreaSqm",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_FromWhomId",
                table: "Properties",
                column: "FromWhomId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_SubTypeId",
                table: "Properties",
                column: "SubTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_DefinitionValues_FromWhomId",
                table: "Properties",
                column: "FromWhomId",
                principalTable: "DefinitionValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_DefinitionValues_SubTypeId",
                table: "Properties",
                column: "SubTypeId",
                principalTable: "DefinitionValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_DefinitionValues_FromWhomId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_DefinitionValues_SubTypeId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_FromWhomId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_SubTypeId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BlockNumber",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "DeedStatus",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Dues",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "FromWhomId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "GrossArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "NetArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ParcelNumber",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ShareAreaSqm",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ShowAgencyInfo",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "SubTypeId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TotalAreaSqm",
                table: "Properties");

            migrationBuilder.AlterColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
