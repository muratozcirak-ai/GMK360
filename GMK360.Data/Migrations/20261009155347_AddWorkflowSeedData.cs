using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WorkflowTemplates",
                columns: new[] { "Id", "CreatedAt", "ExperienceNote", "IsDeleted", "IsGatekeeper", "PhaseCategory", "StepOrder", "TaskName", "UpdatedAt", "WaitDays", "WorkflowType" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(8355), "Sınır kotları haritacı tarafından net işaretlenmeden kazıya başlama!", false, false, "Temel", 1, "Aplikasyon ve Harita Ölçümü", null, 0, "Kaba" },
                    { 2, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9800), "Hafriyat toprağını şantiye sahasında gereksiz yer kaplamayacak şekilde istifle.", false, false, "Temel", 2, "Hafriyat ve Temel Kazısı", null, 0, "Kaba" },
                    { 3, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9802), "Yalıtım yırtılmasın diye grobeton yüzeyi pürüzsüz olmalı.", false, false, "Temel", 3, "Grobeton ve İzolasyon (Bohçalama)", null, 1, "Kaba" },
                    { 4, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9804), "DİKKAT KİLİT NOKTASI: Yapı denetim gelip paspayı ve demir sayımını onaylamadan kesinlikle beton dökülemez!", false, true, "Temel", 4, "Temel Demiri ve Paspayı Kontrolü", null, 0, "Kaba" },
                    { 5, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9826), "Beton döküldükten sonra yanmaması için yaz aylarında günde en az 2 kez sulama (kürleme) yapılacak.", false, false, "Temel", 5, "Temel Betonu Dökümü ve Kürleme", null, 3, "Kaba" },
                    { 6, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9828), "Belediyeden subasman vize belgesi alınmadan 1. Katın kolon filizlerine başlama!", false, true, "Temel", 6, "Subasman Vizesi (Onay)", null, 0, "Kaba" },
                    { 7, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9829), "Şakül kontrollerini çift taraflı yap.", false, false, "TipKat", 1, "Kolon ve Perde Kalıbı", null, 0, "Kaba" },
                    { 8, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9831), "Yapı denetim kolon demirlerini onaylamadan kalıbı kapatma!", false, true, "TipKat", 2, "Kolon ve Perde Demiri", null, 0, "Kaba" },
                    { 9, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9832), "Tesisat rezervasyon (boşluk) borularını yerleştirmeyi unutma.", false, true, "TipKat", 3, "Döşeme Kalıp ve Demiri", null, 0, "Kaba" },
                    { 10, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9835), "Vibratörsüz kesinlikle döküm yapılmayacak.", false, false, "TipKat", 4, "Kat Betonu Dökümü", null, 3, "Kaba" },
                    { 11, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9837), "DİKKAT: Temizlikten çıkan molozları kesinlikle asansör boşluğuna veya tesisat şaftlarına dökmeyin!", false, false, "TipKat", 5, "Kaba Temizlik", null, 0, "InceSabit" },
                    { 12, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9838), "Kapı kasa boşluk ölçülerine standartlara göre dikkat et.", false, false, "TipKat", 6, "Duvar / Tuğla Örümü", null, 0, "InceSabit" },
                    { 13, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9840), "Tesisat testi (basınç testi) yapılmadan sıva aşamasına geçilmeyecek!", false, true, "TipKat", 7, "Altyapı Tesisat (Elektrik & Sıhhi)", null, 0, "InceSabit" },
                    { 14, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9842), "Şap dökülmeden önce zemin temizliği tam yapılmalı.", false, false, "TipKat", 8, "Kaba Sıva ve Kat Şapı", null, 2, "InceSabit" },
                    { 15, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9844), "CRM'den müşterinin renk/desen seçimi bekleniyor! Onay gelmeden döşemeye başlama.", false, true, "TipKat", 9, "Islak Hacim Seramik/Fayans", null, 0, "InceSecmeli" },
                    { 16, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9845), "Müşteri tercihi teyit edilecek. Şapın tam kuruduğundan emin ol.", false, true, "TipKat", 10, "Zemin Kaplama (Parke/Mermer)", null, 0, "InceSecmeli" },
                    { 17, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9847), "Müşteri aksesuar/model seçimi bekleniyor.", false, true, "TipKat", 11, "Mutfak Dolapları ve İç Kapılar", null, 0, "InceSecmeli" },
                    { 18, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9848), "Rüzgar bağlantılarını sağlamlaştır.", false, false, "Cati", 1, "Çelik/Ahşap Konstrüksiyon İşleri", null, 0, "Kaba" },
                    { 19, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9850), "Yağmur testi yapılmadan kiremit/kaplama işine geçme.", false, true, "Cati", 2, "Çatı İzolasyonu (Su ve Isı)", null, 0, "Kaba" },
                    { 20, new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9851), "Dere oluklarının eğimlerini son kez kontrol et.", false, false, "Cati", 3, "Çatı Kaplama (Kiremit/Membran)", null, 0, "Kaba" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 20);
        }
    }
}
