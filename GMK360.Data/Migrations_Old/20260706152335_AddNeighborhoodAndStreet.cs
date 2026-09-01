using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodAndStreet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Properties");

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Streets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streets_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Name", "PlateCode", "UpdatedAt" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 7, 6, 18, 23, 34, 978, DateTimeKind.Local).AddTicks(6682), false, "Ankara", "06", null },
                    { 34, new DateTime(2026, 7, 6, 18, 23, 34, 976, DateTimeKind.Local).AddTicks(8342), false, "İstanbul", "34", null },
                    { 35, new DateTime(2026, 7, 6, 18, 23, 34, 978, DateTimeKind.Local).AddTicks(6706), false, "İzmir", "35", null }
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "CityId", "CreatedAt", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 34, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3877), false, "Kadıköy", null },
                    { 2, 34, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3888), false, "Beşiktaş", null },
                    { 3, 34, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3890), false, "Şişli", null },
                    { 4, 6, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3892), false, "Çankaya", null },
                    { 5, 6, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3893), false, "Keçiören", null },
                    { 6, 35, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3895), false, "Karşıyaka", null },
                    { 7, 35, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(3896), false, "Bornova", null }
                });

            migrationBuilder.InsertData(
                table: "Neighborhoods",
                columns: new[] { "Id", "CreatedAt", "DistrictId", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(4704), 1, false, "Acıbadem", null },
                    { 2, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(4708), 1, false, "Bostancı", null },
                    { 3, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(4710), 4, false, "Bahçelievler", null }
                });

            migrationBuilder.InsertData(
                table: "Streets",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Name", "NeighborhoodId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(5394), false, "Gül Sokak", 1, null },
                    { 2, new DateTime(2026, 7, 6, 18, 23, 34, 979, DateTimeKind.Local).AddTicks(5397), false, "Lale Sokak", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_StreetId",
                table: "Properties",
                column: "StreetId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_DistrictId",
                table: "Neighborhoods",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Streets_NeighborhoodId",
                table: "Streets",
                column: "NeighborhoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Streets_StreetId",
                table: "Properties",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Streets_StreetId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "Streets");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropIndex(
                name: "IX_Properties_NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_StreetId",
                table: "Properties");

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "Properties");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
