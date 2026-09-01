using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthBanners_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthScreenBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ActionText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthScreenBanners", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AuthScreenBanners",
                columns: new[] { "Id", "ActionText", "ActionUrl", "CreatedAt", "DisplayOrder", "ImageUrl", "IsActive", "IsDeleted", "Subtitle", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Hemen İlan Ver", "/Property/Create", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/auth_bg.jpg", true, false, "Yeni nesil emlak platformuna katılarak ilanlarınızı milyonlara ulaştırın veya hayalinizdeki evi bulun.", "Sektörün Zirvesine Çıkın.", null },
                    { 2, "Kampanyaya Katıl", "/Campaigns/First3", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=1000&q=80", true, false, "Bulunduğunuz ildeki ilk 3 kurumsal üye arasına girin, 1 yıllık Premium Vitrin paketini anında kapın.", "İlk 3 Üyeye Özel Fırsat!", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthScreenBanners");
        }
    }
}
