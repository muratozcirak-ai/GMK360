using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectStatusAndMapConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ConstructionProjects");

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsUrl",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicDescription",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ConstructionProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasMapConsent",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MapConsentDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleMapsUrl",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PublicDescription",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "HasMapConsent",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MapConsentDate",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "ConstructionProjects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
