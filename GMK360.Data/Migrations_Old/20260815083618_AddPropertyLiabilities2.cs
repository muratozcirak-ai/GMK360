using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyLiabilities2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UtilityCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyLiabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UtilityCompanyId = table.Column<int>(type: "int", nullable: true),
                    LiabilityTypeId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecurringAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DueDayOfMonth = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyLiabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyLiabilities_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyLiabilities_UtilityCompanies_UtilityCompanyId",
                        column: x => x.UtilityCompanyId,
                        principalTable: "UtilityCompanies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropertyLiabilities_tbl_gmk_YukumlulukTipleri_LiabilityTypeId",
                        column: x => x.LiabilityTypeId,
                        principalTable: "tbl_gmk_YukumlulukTipleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLiabilities_LiabilityTypeId",
                table: "PropertyLiabilities",
                column: "LiabilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLiabilities_PropertyId",
                table: "PropertyLiabilities",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLiabilities_UtilityCompanyId",
                table: "PropertyLiabilities",
                column: "UtilityCompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyLiabilities");

            migrationBuilder.DropTable(
                name: "UtilityCompanies");
        }
    }
}
