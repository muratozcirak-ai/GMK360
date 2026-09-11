using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLegalDocumentTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedTo",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SystemTemplateId",
                table: "ProjectLegalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrackingPerson",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemLegalDocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetModule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    LegalReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLegalDocumentTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLegalDocuments_SystemTemplateId",
                table: "ProjectLegalDocuments",
                column: "SystemTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectLegalDocuments_SystemLegalDocumentTemplates_SystemTemplateId",
                table: "ProjectLegalDocuments",
                column: "SystemTemplateId",
                principalTable: "SystemLegalDocumentTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectLegalDocuments_SystemLegalDocumentTemplates_SystemTemplateId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropTable(
                name: "SystemLegalDocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ProjectLegalDocuments_SystemTemplateId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "AppliedTo",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "SystemTemplateId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "TrackingPerson",
                table: "ProjectLegalDocuments");
        }
    }
}
