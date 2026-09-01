using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        public ContractController()
        {
            // QuestPDF lisans yapılandırması - Community lisansı ücretsiz projeler için uygundur.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Tüm sözleşmelerin listelendiği sayfa (Dashboard sekmesi)
        public IActionResult Index()
        {
            return View();
        }

        // Yeni sözleşme oluşturma sihirbazı (Wizard)
        public IActionResult Wizard()
        {
            return View();
        }

        // Emlakçı Yetki Belgesi taslağı
        public IActionResult AgencyAuthorization()
        {
            return View();
        }

        // Yer Gösterme Belgesi
        public IActionResult PropertyShowing()
        {
            return View();
        }

        // PDF Üretme Motoru (Önizleme veya İndirme)
        [HttpPost]
        public IActionResult GeneratePDF(int contractType, int propertyId, string secondPartyTc, string terms)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));
                    
                    page.Header()
                        .Text(contractType == 1 ? "Emlakçı Yetki Belgesi" : "Sözleşme / Yer Gösterme Belgesi")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);
                            
                            x.Item().Text($"Mülk ID: {propertyId}");
                            x.Item().Text($"İkinci Taraf TC: {secondPartyTc}");
                            x.Item().Text($"Tarih: {System.DateTime.Now:dd.MM.yyyy HH:mm}");
                            
                            x.Item().Text("Şartlar:");
                            x.Item().Text(string.IsNullOrEmpty(terms) ? "Belirtilmemiş" : terms);
                            
                            x.Item().PaddingTop(25).Text("Taraflar yukarıdaki şartları kabul etmiştir.");
                            
                            x.Item().PaddingTop(50).Row(row =>
                            {
                                row.RelativeItem().Text("Mülk Sahibi / Emlakçı").Bold();
                                row.RelativeItem().Text("Müşteri / Alıcı / Kiracı").Bold();
                            });
                        });
                        
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Sayfa ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            // Dosya adı
            var fileName = $"Sozlesme_{propertyId}_{System.DateTime.Now:yyyyMMddHHmmss}.pdf";
            
            // Generate PDF as byte array
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
