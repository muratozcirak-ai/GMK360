using System;
using System.IO;
using GMK360.Core.DTOs;
using GMK360.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GMK360.Web.Services.Construction
{
    public class ConstructionFeasibilityPdfService : IConstructionFeasibilityPdfService
    {
        public byte[] GenerateFeasibilityReport(FeasibilityPdfRequestDto dto)
        {
            byte[]? mapBytes = null;
            if (!string.IsNullOrWhiteSpace(dto.MapImageBase64))
            {
                try
                {
                    var rawBase64 = dto.MapImageBase64;
                    var commaIndex = rawBase64.IndexOf(',');
                    if (commaIndex >= 0) rawBase64 = rawBase64.Substring(commaIndex + 1);
                    mapBytes = Convert.FromBase64String(rawBase64);
                }
                catch
                {
                    mapBytes = null;
                }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    // ==========================================
                    // 1. ÜST BİLGİ / ANTET (HEADER)
                    // ==========================================
                    page.Header().Column(headerCol =>
                    {
                        headerCol.Item().Row(row =>
                        {
                            row.RelativeItem(3).Column(c =>
                            {
                                c.Item().Text(string.IsNullOrWhiteSpace(dto.CompanyName) ? "GMK360 İNŞAAT & MÜHENDİSLİK" : dto.CompanyName.ToUpper())
                                    .FontSize(13).Bold().FontColor(Colors.Blue.Darken4);
                                c.Item().Text("Aday Proje Resmi İmar Durumu, Kadastro & Mimari Ön Etüt Raporu")
                                    .FontSize(9).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"Proje: {dto.ProjectName ?? "Aday Proje"}")
                                    .FontSize(11).Bold().FontColor(Colors.Red.Darken2);
                            });

                            row.RelativeItem(2).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Rapor No: GMK360-IDB-{dto.ProjectId:D5}").FontSize(8).Bold().FontColor(Colors.Grey.Darken3);
                                c.Item().Text($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text("Statü: Aday Proje / Fizibilite").FontSize(8).Bold().FontColor(Colors.Orange.Darken3);
                            });
                        });

                        headerCol.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken4);
                    });

                    // ==========================================
                    // 2. İÇERİK (CONTENT)
                    // ==========================================
                    page.Content().PaddingTop(10).Column(content =>
                    {
                        content.Spacing(12);

                        // BÖLÜM 1: MÜLK & KADASTRO BİLGİLERİ (TKGM MEGSİS)
                        content.Item().Column(sec =>
                        {
                            sec.Item().Row(r =>
                            {
                                r.RelativeItem().Text("1. MÜLK VE RESMİ KADASTRO BİLGİLERİ (TKGM MEGSİS)").FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                                r.ConstantItem(120).AlignRight().Text("TKGM Canlı Verisi").FontSize(8).FontColor(Colors.Green.Darken2).Bold();
                            });

                            sec.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                    cols.RelativeColumn(2);
                                });

                                // Satır 1
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("İl / İlçe").Bold();
                                table.Cell().Padding(4).Text($"{dto.CityName ?? "-"} / {dto.DistrictName ?? "-"}");
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Mahalle").Bold();
                                table.Cell().Padding(4).Text($"{dto.NeighborhoodName ?? "-"}");

                                // Satır 2
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Ada / Parsel").Bold();
                                table.Cell().Padding(4).Text($"Ada: {dto.Ada ?? "-"} / Parsel: {dto.Parsel ?? "-"}").Bold().FontColor(Colors.Blue.Darken3);
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Pafta").Bold();
                                table.Cell().Padding(4).Text(string.IsNullOrWhiteSpace(dto.Pafta) ? "-" : dto.Pafta);

                                // Satır 3
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Nitelik").Bold();
                                table.Cell().Padding(4).Text(string.IsNullOrWhiteSpace(dto.Nitelik) ? "Arsa" : dto.Nitelik);
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Resmi Arsa Alanı").Bold();
                                table.Cell().Padding(4).Text($"{dto.LandArea:N2} m²").Bold().FontColor(Colors.Green.Darken3);

                                // Satır 4
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Koordinatlar").Bold();
                                table.Cell().ColumnSpan(3).Padding(4).Text(dto.Latitude.HasValue && dto.Longitude.HasValue ? $"{dto.Latitude:F6}, {dto.Longitude:F6} (WGS84)" : "Tanımlanmadı");
                            });
                        });

                        // BÖLÜM 2: RESMİ İMAR ŞARTLARI & MİMARİ KAPASİTE ANALİZİ
                        content.Item().Column(sec =>
                        {
                            sec.Item().Text("2. RESMİ İMAR KOŞULLARI VE ÇAP KAPASİTE SİMÜLASYONU").FontSize(10).Bold().FontColor(Colors.Blue.Darken4);

                            sec.Item().PaddingTop(4).Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(1.5f);
                                    cols.RelativeColumn(2.5f);
                                });

                                // Tablo Başlıkları
                                table.Cell().Background(Colors.Blue.Darken4).Padding(5).Text("İmar Parametresi").FontColor(Colors.White).Bold();
                                table.Cell().Background(Colors.Blue.Darken4).Padding(5).Text("Kriter / Oran").FontColor(Colors.White).Bold();
                                table.Cell().Background(Colors.Blue.Darken4).Padding(5).Text("Hesaplanan Kapasite").FontColor(Colors.White).Bold();

                                // Emsal (KAKS)
                                table.Cell().Padding(4).Text("Toplam Emsal İnşaat Alanı (KAKS)");
                                table.Cell().Padding(4).Text($"{dto.Emsal:N2}");
                                table.Cell().Padding(4).Text($"{dto.TotalBuildingArea:N2} m²").Bold().FontColor(Colors.Blue.Darken3);

                                // Taban Oturumu (TAKS)
                                table.Cell().Padding(4).Text("Zemin Taban Oturumu Alanı (TAKS)");
                                table.Cell().Padding(4).Text($"{dto.Taks:N2}");
                                table.Cell().Padding(4).Text($"{dto.GroundBaseArea:N2} m²");

                                // İzinli Kat (Yençok)
                                table.Cell().Padding(4).Text("İzinli Maksimum Kat Adedi (Yençok)");
                                table.Cell().Padding(4).Text($"{dto.TotalFloors} Kat");
                                table.Cell().Padding(4).Text("-");

                                // Hedef Ortalama Daire
                                table.Cell().Padding(4).Text("Planlanan Ortalama Daire Büyüklüğü");
                                table.Cell().Padding(4).Text($"{dto.FlatSqm:N0} m²");
                                table.Cell().Padding(4).Text("-");

                                // Üretilebilir Daire
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("Üretilebilir Toplam Daire Kapasitesi").Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("-");
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text($"~{dto.EstimatedUnits} Daire").Bold().FontColor(Colors.Blue.Darken4);

                                // Eski Yapı / Hak Sahibi
                                table.Cell().Padding(4).Text("Mevcut Yapıdaki Hak Sahibi Dairesi");
                                table.Cell().Padding(4).Text($"{dto.OldUnits} Daire");
                                table.Cell().Padding(4).Text("-");

                                // Firma Satış Potansiyeli
                                table.Cell().Background(Colors.Green.Lighten5).Padding(4).Text("Müteahhit / Firma Satış Potansiyeli (Fazla Daire)").Bold();
                                table.Cell().Background(Colors.Green.Lighten5).Padding(4).Text("-");
                                var surplusColor = dto.SurplusUnits >= 0 ? Colors.Green.Darken3 : Colors.Red.Darken3;
                                var surplusText = dto.SurplusUnits >= 0 ? $"+{dto.SurplusUnits} Daire" : $"{dto.SurplusUnits} Daire (Açık)";
                                table.Cell().Background(Colors.Green.Lighten5).Padding(4).Text(surplusText).Bold().FontColor(surplusColor);
                            });
                        });

                        // ==========================================
                        // MÜTEAHHİT SENARYO & TEKLİF KARŞILAŞTIRMA MASASI
                        // ==========================================
                        if (dto.Scenarios != null && dto.Scenarios.Count > 0)
                        {
                            content.Item().Column(sec =>
                            {
                                sec.Item().Text("MÜTEAHHİT ÖN FİZİBİLİTE & TEKLİF SENARYOLARI KARŞILAŞTIRMA TABLOSU")
                                    .FontSize(10).Bold().FontColor(Colors.Green.Darken3);

                                sec.Item().PaddingTop(4).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(2.6f);
                                        foreach (var sc in dto.Scenarios)
                                        {
                                            cols.RelativeColumn(2.2f);
                                        }
                                    });

                                    // Tablo Başlıkları
                                    table.Cell().Background(Colors.Green.Darken3).Padding(5).Text("Karşılaştırma Parametresi").FontColor(Colors.White).Bold();
                                    foreach (var sc in dto.Scenarios)
                                    {
                                        table.Cell().Background(Colors.Green.Darken3).Padding(5).Text(sc.Title).FontColor(Colors.White).Bold();
                                    }

                                    // Satır 1: Kat & Daire Düzeni
                                    table.Cell().Padding(4).Text("İmar Kat & Kat Başı Daire");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.Floors} Kat × {sc.UnitsPerFloor} Daire");

                                    // Satır 2: Net Daire Alanı & Tip
                                    table.Cell().Padding(4).Text("Daire Başı Net Alan & Tip");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"~{sc.UnitSqm:N0} m² ({sc.UnitType})").Bold();

                                    // Satır 3: Toplam Üretilen Daire
                                    table.Cell().Padding(4).Text("Yeni Toplam Üretilen Daire");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.TotalUnits} Daire");

                                    // Satır 4: Maliklere Verilen Daire
                                    table.Cell().Padding(4).Text("Maliklere Verilen Daire Payı");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.OwnerUnits} Daire (%{sc.OwnerSharePercent:N0})");

                                    // Satır 5: Müteahhide Kalan Daire
                                    table.Cell().Background(Colors.Green.Lighten5).Padding(4).Text("Müteahhide Kalan Satılabilir Daire").Bold();
                                    foreach (var sc in dto.Scenarios) table.Cell().Background(Colors.Green.Lighten5).Padding(4).Text($"{sc.ContractorUnits} DAİRE").Bold().FontColor(Colors.Green.Darken3);

                                    // Satır 6: Daire Satış Fiyatı
                                    table.Cell().Padding(4).Text("Tahmini Daire Satış Fiyatı");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.UnitPrice:N0} ₺");

                                    // Satır 7: Satış Cirosu
                                    table.Cell().Padding(4).Text("Tahmini Müteahhit Satış Cirosu");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.TotalRevenue:N0} ₺").Bold();

                                    // Satır 8: Toplam İnşaat Alanı & Maliyet
                                    table.Cell().Padding(4).Text("Toplam İnşaat Alanı & Şantiye Maliyeti");
                                    foreach (var sc in dto.Scenarios) table.Cell().Padding(4).Text($"{sc.TotalConstSqm:N0} m² | {sc.FirmCost:N0} ₺");

                                    // Satır 9: Net Müteahhit Kârı
                                    table.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("⭐ TAHMİNİ NET MÜTEAHHİT KÂRI").Bold().FontColor(Colors.Green.Darken4);
                                    foreach (var sc in dto.Scenarios) table.Cell().Background(Colors.Green.Lighten4).Padding(5).Text($"{sc.NetProfit:N0} ₺").Bold().FontColor(Colors.Green.Darken4);
                                });
                            });
                        }

                        // BÖLÜM 3: HARİTA KROKİSİ / UYDU GÖRÜNTÜSÜ
                        if (mapBytes != null && mapBytes.Length > 0)
                        {
                            content.Item().Column(sec =>
                            {
                                sec.Item().Text("3. RESMİ KADASTRO HARİTA VE PARSEL SINIR KROKİSİ").FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                                sec.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2).MaxHeight(180).Image(mapBytes);
                                sec.Item().PaddingTop(2).Text("Kaynak: TKGM MEGSİS CBS Uydu Katmanı & Resmi Parsel Sınırı (Kırmızı Poligon)")
                                    .FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
                            });
                        }

                        // BÖLÜM 4: BELEDİYE İMAR NOTLARI VE ŞARTLARI
                        content.Item().Column(sec =>
                        {
                            sec.Item().Text("4. BELEDİYE İMAR VE ÇAP NOTLARI").FontSize(10).Bold().FontColor(Colors.Blue.Darken4);

                            var notes = string.IsNullOrWhiteSpace(dto.ImarNotlari)
                                ? "Ön bahçe çekme mesafesi 5.00m, yan bahçe çekme mesafeleri 3.00m'dir. Otopark yönetmeliği gereği bodrum katlarda yeterli sayıda otopark parsel bünyesinde çözülecektir. Zemin etüt onayına müteakip statik proje hazırlanacaktır."
                                : dto.ImarNotlari;

                            sec.Item().PaddingTop(4).Background(Colors.Grey.Lighten5).Padding(8).Border(0.5f).BorderColor(Colors.Grey.Lighten3).Column(c =>
                            {
                                c.Item().Text(notes).FontSize(8.5f).FontColor(Colors.Grey.Darken3);
                                c.Item().PaddingTop(4).Text("• Zemin Kat Emniyet Payı (Risk Rezervi): Zemin kat toplam inşaat maliyet hacmine dahil edilmiş olup, daire üretim hesabına bilinçli olarak katılmamıştır. Bu alan; şantiyede oluşabilecek öngörülemeyen ekstra maliyetler, zemin ticari/dükkan potansiyeli ve piyasa dalgalanmalarına karşı müteahhit için emniyet tamponu niteliğindedir. Projenin kesinleşen mimari ve statik projelendirmesinde netleşecektir.")
                                    .FontSize(8f).FontColor(Colors.Grey.Darken3).Italic();
                            });
                        });

                        // BÖLÜM 5: YASAL UYARI VE İMZA ALANI
                        content.Item().Column(sec =>
                        {
                            sec.Item().Background(Colors.Grey.Lighten4).Padding(6).Column(c =>
                            {
                                c.Item().Text("⚠️ YASAL ÇEKİNCE VE BİLGİLENDİRME NOTU").FontSize(8).Bold().FontColor(Colors.Red.Darken3);
                                c.Item().Text("İşbu rapor, GMK360 Kurumsal Şantiye & Fizibilite Sistemi üzerinden TKGM MEGSİS resmi kadastro verileri ve ilgili belediyenin imar plan notları referans alınarak mimari ön etüt ve yatırım fizibilitesi amacıyla üretilmiştir. Resmi inşaat ruhsatı başvurularında ve kat karşılığı sözleşmelerde yerel belediyeden onaylı ıslak imzalı imar durumu belgesi (çap) ve LİHKAB aplikasyon krokisi esastır.")
                                    .FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                            });

                            sec.Item().PaddingTop(15).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Hazırlayan:").Bold().FontSize(8.5f);
                                    c.Item().Text("Mimar / Proje Geliştirme Uzmanı").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(25).Text("İmza: .......................................").FontSize(8);
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Onaylayan:").Bold().FontSize(8.5f);
                                    c.Item().Text(string.IsNullOrWhiteSpace(dto.CompanyName) ? "Şirket Yetkilisi" : dto.CompanyName).FontSize(8).FontColor(Colors.Grey.Darken1);
                                    c.Item().PaddingTop(25).Text("Kaşe / İmza: .......................................").FontSize(8);
                                });
                            });
                        });
                    });

                    // ==========================================
                    // 3. ALT BİLGİ (FOOTER)
                    // ==========================================
                    page.Footer().Column(footer =>
                    {
                        footer.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        footer.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text("GMK360 İnşaat & Gayrimenkul Geliştirme Platformu").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            row.RelativeItem().AlignCenter().Text(x =>
                            {
                                x.Span("Sayfa ").FontSize(7.5f);
                                x.CurrentPageNumber().FontSize(7.5f);
                                x.Span(" / ").FontSize(7.5f);
                                x.TotalPages().FontSize(7.5f);
                            });
                            row.RelativeItem().AlignRight().Text("www.gmk360.com").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
