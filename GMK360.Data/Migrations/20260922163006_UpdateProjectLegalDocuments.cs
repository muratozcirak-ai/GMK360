using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectLegalDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcquiredDate",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "AppliedTo",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "InstitutionPhone",
                table: "ProjectLegalDocuments");

            migrationBuilder.RenameColumn(
                name: "TrackingPerson",
                table: "ProjectLegalDocuments",
                newName: "IssueNotes");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "ProjectLegalDocuments",
                newName: "AssignedUserId");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "ProjectLegalDocuments",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "DocumentCost",
                table: "ProjectLegalDocuments",
                newName: "EstimatedCost");

            migrationBuilder.RenameColumn(
                name: "ApplicationDate",
                table: "ProjectLegalDocuments",
                newName: "CompletedDate");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "ProjectLegalDocuments",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "ProjectLegalDocuments");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "ProjectLegalDocuments",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "IssueNotes",
                table: "ProjectLegalDocuments",
                newName: "TrackingPerson");

            migrationBuilder.RenameColumn(
                name: "EstimatedCost",
                table: "ProjectLegalDocuments",
                newName: "DocumentCost");

            migrationBuilder.RenameColumn(
                name: "CompletedDate",
                table: "ProjectLegalDocuments",
                newName: "ApplicationDate");

            migrationBuilder.RenameColumn(
                name: "AssignedUserId",
                table: "ProjectLegalDocuments",
                newName: "Notes");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcquiredDate",
                table: "ProjectLegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedTo",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionPhone",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
