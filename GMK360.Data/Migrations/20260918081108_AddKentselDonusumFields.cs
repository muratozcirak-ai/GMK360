using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKentselDonusumFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BinaYoneticisiId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EskiDaireSayisi",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EskiKatSayisi",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EskiToplamMetrekare",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    ContactId = table.Column<int>(type: "int", nullable: false),
                    FlatNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LandShare = table.Column<double>(type: "float", nullable: true),
                    IsCommitteeMember = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectOwners_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectOwners_CrmContacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "CrmContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectOwnerDebts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectOwnerId = table.Column<int>(type: "int", nullable: false),
                    DebtAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectOwnerDebts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectOwnerDebts_ProjectOwners_ProjectOwnerId",
                        column: x => x.ProjectOwnerId,
                        principalTable: "ProjectOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionProjects_BinaYoneticisiId",
                table: "ConstructionProjects",
                column: "BinaYoneticisiId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOwnerDebts_ProjectOwnerId",
                table: "ProjectOwnerDebts",
                column: "ProjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOwners_ConstructionProjectId",
                table: "ProjectOwners",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOwners_ContactId",
                table: "ProjectOwners",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConstructionProjects_CrmContacts_BinaYoneticisiId",
                table: "ConstructionProjects",
                column: "BinaYoneticisiId",
                principalTable: "CrmContacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConstructionProjects_CrmContacts_BinaYoneticisiId",
                table: "ConstructionProjects");

            migrationBuilder.DropTable(
                name: "ProjectOwnerDebts");

            migrationBuilder.DropTable(
                name: "ProjectOwners");

            migrationBuilder.DropIndex(
                name: "IX_ConstructionProjects_BinaYoneticisiId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "BinaYoneticisiId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "EskiDaireSayisi",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "EskiKatSayisi",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "EskiToplamMetrekare",
                table: "ConstructionProjects");
        }
    }
}
