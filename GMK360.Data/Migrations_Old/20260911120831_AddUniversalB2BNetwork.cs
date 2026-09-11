using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversalB2BNetwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "B2BNetworkContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerAgencyId = table.Column<int>(type: "int", nullable: false),
                    AddedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectorCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredAgencyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BNetworkContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BNetworkContacts_Agencies_OwnerAgencyId",
                        column: x => x.OwnerAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_B2BNetworkContacts_Agencies_RegisteredAgencyId",
                        column: x => x.RegisteredAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "B2BQuoteRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterAgencyId = table.Column<int>(type: "int", nullable: false),
                    RequesterUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceModule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceReferenceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BQuoteRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BQuoteRequests_Agencies_RequesterAgencyId",
                        column: x => x.RequesterAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "B2BQuoteInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteRequestId = table.Column<int>(type: "int", nullable: false),
                    NetworkContactId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OfferedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OfferNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BQuoteInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BQuoteInvites_B2BNetworkContacts_NetworkContactId",
                        column: x => x.NetworkContactId,
                        principalTable: "B2BNetworkContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_B2BQuoteInvites_B2BQuoteRequests_QuoteRequestId",
                        column: x => x.QuoteRequestId,
                        principalTable: "B2BQuoteRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Hemen �lan Ver", "Yeni nesil emlak platformuna kat�larak ilanlar�n�z� milyonlara ula�t�r�n veya hayalinizdeki evi bulun.", "Sekt�r�n Zirvesine ��k�n." });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Kampanyaya Kat�l", "Bulundu�unuz ildeki ilk 3 kurumsal �ye aras�na girin, 1 y�ll�k Premium Vitrin paketini an�nda kap�n.", "�lk 3 �yeye �zel F�rsat!" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34,
                column: "Name",
                value: "�stanbul");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35,
                column: "Name",
                value: "�zmir");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "T�rkiye");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Kad�k�y");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Be�ikta�");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "�i�li");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "�ankaya");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Ke�i�ren");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Kar��yaka");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Kira Gelir Vergisi (GMS�)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "�evre Temizlik Vergisi (�TV)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "�lan ve Reklam Vergisi (Tabela)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Kira Stopaj� (Muhtasar Beyanname)");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Ac�badem");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bostanc�");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bah�elievler");

            migrationBuilder.UpdateData(
                table: "Streets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "G�l Sokak");

            migrationBuilder.CreateIndex(
                name: "IX_B2BNetworkContacts_OwnerAgencyId",
                table: "B2BNetworkContacts",
                column: "OwnerAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BNetworkContacts_RegisteredAgencyId",
                table: "B2BNetworkContacts",
                column: "RegisteredAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BQuoteInvites_NetworkContactId",
                table: "B2BQuoteInvites",
                column: "NetworkContactId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BQuoteInvites_QuoteRequestId",
                table: "B2BQuoteInvites",
                column: "QuoteRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BQuoteRequests_RequesterAgencyId",
                table: "B2BQuoteRequests",
                column: "RequesterAgencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B2BQuoteInvites");

            migrationBuilder.DropTable(
                name: "B2BNetworkContacts");

            migrationBuilder.DropTable(
                name: "B2BQuoteRequests");

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Hemen İlan Ver", "Yeni nesil emlak platformuna katılarak ilanlarınızı milyonlara ulaştırın veya hayalinizdeki evi bulun.", "Sektörün Zirvesine Çıkın." });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Kampanyaya Katıl", "Bulunduğunuz ildeki ilk 3 kurumsal üye arasına girin, 1 yıllık Premium Vitrin paketini anında kapın.", "İlk 3 Üyeye Özel Fırsat!" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34,
                column: "Name",
                value: "İstanbul");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35,
                column: "Name",
                value: "İzmir");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Türkiye");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Kadıköy");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Beşiktaş");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Şişli");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Çankaya");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Keçiören");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Karşıyaka");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Kira Gelir Vergisi (GMSİ)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Çevre Temizlik Vergisi (ÇTV)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "İlan ve Reklam Vergisi (Tabela)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Kira Stopajı (Muhtasar Beyanname)");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Acıbadem");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bostancı");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bahçelievler");

            migrationBuilder.UpdateData(
                table: "Streets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Gül Sokak");
        }
    }
}

