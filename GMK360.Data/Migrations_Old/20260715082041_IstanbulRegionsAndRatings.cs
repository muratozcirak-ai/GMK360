using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class IstanbulRegionsAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Side",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionName",
                table: "Districts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsultantRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommunicationScore = table.Column<int>(type: "int", nullable: false),
                    KnowledgeScore = table.Column<int>(type: "int", nullable: false),
                    RecommendationScore = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultantRatings_AspNetUsers_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultantRatings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HasRated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactLogs_AspNetUsers_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6,
                column: "RegionName",
                value: null);

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 7,
                column: "RegionName",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantRatings_ConsultantId",
                table: "ConsultantRatings",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantRatings_UserId",
                table: "ConsultantRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_ConsultantId",
                table: "ContactLogs",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_PropertyId",
                table: "ContactLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogs_UserId",
                table: "ContactLogs",
                column: "UserId");

            migrationBuilder.Sql(@"
                UPDATE Districts SET RegionName = 'Anadolu Yakası' WHERE CityId = 34 AND Name IN (N'Adalar', N'Ataşehir', N'Beykoz', N'Çekmeköy', N'Kadıköy', N'Kartal', N'Maltepe', N'Pendik', N'Sancaktepe', N'Sultanbeyli', N'Şile', N'Tuzla', N'Ümraniye', N'Üsküdar');
                UPDATE Districts SET RegionName = 'Avrupa Yakası' WHERE CityId = 34 AND Name IN (N'Arnavutköy', N'Avcılar', N'Bağcılar', N'Bahçelievler', N'Bakırköy', N'Başakşehir', N'Bayrampaşa', N'Beşiktaş', N'Beylikdüzü', N'Beyoğlu', N'Büyükçekmece', N'Çatalca', N'Esenler', N'Esenyurt', N'Eyüpsultan', N'Fatih', N'Gaziosmanpaşa', N'Güngören', N'Kâğıthane', N'Küçükçekmece', N'Sarıyer', N'Silivri', N'Sultangazi', N'Şişli', N'Zeytinburnu');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultantRatings");

            migrationBuilder.DropTable(
                name: "ContactLogs");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "RegionName",
                table: "Districts");
        }
    }
}
