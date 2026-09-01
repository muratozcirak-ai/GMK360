using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEcosystemFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "NearbyPlaces",
                newName: "SubCategory");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "NearbyPlaces",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LocalProfessionals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfessionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentityUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalProfessionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalProfessionals_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NeighboringAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseNeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    NeighborNeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    DistanceInKilometers = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeighboringAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeighboringAreas_Neighborhoods_BaseNeighborhoodId",
                        column: x => x.BaseNeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NeighboringAreas_Neighborhoods_NeighborNeighborhoodId",
                        column: x => x.NeighborNeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyViewHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ViewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyViewHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyViewHistories_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalProfessionals_NeighborhoodId",
                table: "LocalProfessionals",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NeighboringAreas_BaseNeighborhoodId",
                table: "NeighboringAreas",
                column: "BaseNeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NeighboringAreas_NeighborNeighborhoodId",
                table: "NeighboringAreas",
                column: "NeighborNeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyViewHistories_PropertyId",
                table: "PropertyViewHistories",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalProfessionals");

            migrationBuilder.DropTable(
                name: "NeighboringAreas");

            migrationBuilder.DropTable(
                name: "PropertyViewHistories");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "NearbyPlaces");

            migrationBuilder.RenameColumn(
                name: "SubCategory",
                table: "NearbyPlaces",
                newName: "Type");
        }
    }
}
