using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDocumentArchiveAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentArchives_Buildings_BuildingId",
                table: "DocumentArchives");

            migrationBuilder.AlterColumn<int>(
                name: "BuildingId",
                table: "DocumentArchives",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "DocumentArchives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "DocumentArchives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentArchives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "DocumentArchives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "DocumentArchives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DocumentArchives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "DocumentArchives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceModule",
                table: "DocumentArchives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SourceRecordId",
                table: "DocumentArchives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DocumentArchives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DocumentArchives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "DocumentArchives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTemplates_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentArchives_AgencyId",
                table: "DocumentArchives",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentArchives_ProjectId",
                table: "DocumentArchives",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_AgencyId",
                table: "DocumentTemplates",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentArchives_Agencies_AgencyId",
                table: "DocumentArchives",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentArchives_Buildings_BuildingId",
                table: "DocumentArchives",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentArchives_ConstructionProjects_ProjectId",
                table: "DocumentArchives",
                column: "ProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentArchives_Agencies_AgencyId",
                table: "DocumentArchives");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentArchives_Buildings_BuildingId",
                table: "DocumentArchives");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentArchives_ConstructionProjects_ProjectId",
                table: "DocumentArchives");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");

            migrationBuilder.DropIndex(
                name: "IX_DocumentArchives_AgencyId",
                table: "DocumentArchives");

            migrationBuilder.DropIndex(
                name: "IX_DocumentArchives_ProjectId",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "SourceModule",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "SourceRecordId",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DocumentArchives");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "DocumentArchives");

            migrationBuilder.AlterColumn<int>(
                name: "BuildingId",
                table: "DocumentArchives",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentArchives_Buildings_BuildingId",
                table: "DocumentArchives",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
