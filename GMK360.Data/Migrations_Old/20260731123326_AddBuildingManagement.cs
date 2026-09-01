using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentSubscriptions_SubscriptionPackages_SubscriptionPackageId",
                table: "AgentSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPackages",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "SubscriptionPackages");

            migrationBuilder.RenameTable(
                name: "SubscriptionPackages",
                newName: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.AddColumn<bool>(
                name: "IsEidsVerified",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxListingCount",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaxFeaturedListingCount",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxNeighborhoodsCount",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPropertiesCount",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetUserType",
                table: "tbl_gmk_AbonelikPaketleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_gmk_AbonelikPaketleri",
                table: "tbl_gmk_AbonelikPaketleri",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EidsValidationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    ConsultantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseCode = table.Column<int>(type: "int", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    SystemReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EidsValidationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EidsValidationLogs_AspNetUsers_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EidsValidationLogs_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_gmk_DijitalSozlesmeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SecondPartyUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SecondPartyFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondPartyIdentityNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondPartyPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondPartyIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_gmk_DijitalSozlesmeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_DijitalSozlesmeler_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_DijitalSozlesmeler_AspNetUsers_SecondPartyUserId",
                        column: x => x.SecondPartyUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_DijitalSozlesmeler_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tbl_gmk_Dokumanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BagliTablo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BagliKayitId = table.Column<int>(type: "int", nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uzanti = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    YuklenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_gmk_Dokumanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_gmk_KullaniciAbonelikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PricePaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_gmk_KullaniciAbonelikleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_KullaniciAbonelikleri_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_KullaniciAbonelikleri_tbl_gmk_AbonelikPaketleri_PackageId",
                        column: x => x.PackageId,
                        principalTable: "tbl_gmk_AbonelikPaketleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_gmk_YukumlulukTipleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SistemTipiMi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_gmk_YukumlulukTipleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_gmk_Odemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MulkId = table.Column<int>(type: "int", nullable: false),
                    YukumlulukTipId = table.Column<int>(type: "int", nullable: false),
                    Donem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "date", nullable: false),
                    OdendiMi = table.Column<bool>(type: "bit", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_gmk_Odemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_Odemeler_Properties_MulkId",
                        column: x => x.MulkId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_gmk_Odemeler_Properties_PropertyId1",
                        column: x => x.PropertyId1,
                        principalTable: "Properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_gmk_Odemeler_tbl_gmk_YukumlulukTipleri_YukumlulukTipId",
                        column: x => x.YukumlulukTipId,
                        principalTable: "tbl_gmk_YukumlulukTipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerUserId",
                table: "Properties",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EidsValidationLogs_ConsultantId",
                table: "EidsValidationLogs",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_EidsValidationLogs_PropertyId",
                table: "EidsValidationLogs",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_DijitalSozlesmeler_CreatorUserId",
                table: "tbl_gmk_DijitalSozlesmeler",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_DijitalSozlesmeler_PropertyId",
                table: "tbl_gmk_DijitalSozlesmeler",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_DijitalSozlesmeler_SecondPartyUserId",
                table: "tbl_gmk_DijitalSozlesmeler",
                column: "SecondPartyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_KullaniciAbonelikleri_PackageId",
                table: "tbl_gmk_KullaniciAbonelikleri",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_KullaniciAbonelikleri_UserId",
                table: "tbl_gmk_KullaniciAbonelikleri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_Odemeler_MulkId",
                table: "tbl_gmk_Odemeler",
                column: "MulkId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_Odemeler_PropertyId1",
                table: "tbl_gmk_Odemeler",
                column: "PropertyId1");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_Odemeler_YukumlulukTipId",
                table: "tbl_gmk_Odemeler",
                column: "YukumlulukTipId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentSubscriptions_tbl_gmk_AbonelikPaketleri_SubscriptionPackageId",
                table: "AgentSubscriptions",
                column: "SubscriptionPackageId",
                principalTable: "tbl_gmk_AbonelikPaketleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_OwnerUserId",
                table: "Properties",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentSubscriptions_tbl_gmk_AbonelikPaketleri_SubscriptionPackageId",
                table: "AgentSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_OwnerUserId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "EidsValidationLogs");

            migrationBuilder.DropTable(
                name: "tbl_gmk_DijitalSozlesmeler");

            migrationBuilder.DropTable(
                name: "tbl_gmk_Dokumanlar");

            migrationBuilder.DropTable(
                name: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropTable(
                name: "tbl_gmk_Odemeler");

            migrationBuilder.DropTable(
                name: "tbl_gmk_YukumlulukTipleri");

            migrationBuilder.DropIndex(
                name: "IX_Properties_OwnerUserId",
                table: "Properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_gmk_AbonelikPaketleri",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "IsEidsVerified",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "MaxNeighborhoodsCount",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "MaxPropertiesCount",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.DropColumn(
                name: "TargetUserType",
                table: "tbl_gmk_AbonelikPaketleri");

            migrationBuilder.RenameTable(
                name: "tbl_gmk_AbonelikPaketleri",
                newName: "SubscriptionPackages");

            migrationBuilder.AlterColumn<int>(
                name: "MaxListingCount",
                table: "SubscriptionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxFeaturedListingCount",
                table: "SubscriptionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessType",
                table: "SubscriptionPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPackages",
                table: "SubscriptionPackages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentSubscriptions_SubscriptionPackages_SubscriptionPackageId",
                table: "AgentSubscriptions",
                column: "SubscriptionPackageId",
                principalTable: "SubscriptionPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
