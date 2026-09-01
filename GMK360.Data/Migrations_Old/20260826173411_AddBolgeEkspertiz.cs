using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBolgeEkspertiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BolgeEkspertizHafizalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Il = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ilce = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mahalle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MulkTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OdaSayisi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrtalamaKira = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrtalamaSatisDegeri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmortismanSuresiYil = table.Column<int>(type: "int", nullable: false),
                    YakinDonatilarJSON = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SorgulamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BolgeEkspertizHafizalari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BolgeEkspertizHafizalari");
        }
    }
}
