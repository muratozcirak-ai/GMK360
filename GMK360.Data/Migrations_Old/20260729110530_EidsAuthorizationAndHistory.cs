using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class EidsAuthorizationAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationDocumentNo",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AuthorizationEndDate",
                table: "Properties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExternalMarketAlert",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasAuthorization",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerIdNumber",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationDocumentNo",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AuthorizationEndDate",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ExternalMarketAlert",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "HasAuthorization",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OwnerIdNumber",
                table: "Properties");
        }
    }
}
