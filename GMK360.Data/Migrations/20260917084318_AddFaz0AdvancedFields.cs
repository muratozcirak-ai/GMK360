using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFaz0AdvancedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcquiredDate",
                table: "ProjectLegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "ProjectLegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentCost",
                table: "ProjectLegalDocuments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcquiredDate",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentCost",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ProjectLegalDocuments");
        }
    }
}
