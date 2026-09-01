using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services.Integration
{
    public class KbsIntegrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Logging.ILogger<KbsIntegrationService> _logger;
        
        // EGM Test Endpoint (Bu adres gerçeğinde EGM'nin verdiği SOAP URL'si olacaktır)
        private const string KbsEndpoint = "https://kbs.egm.gov.tr/KBS/ws/KBSReceiveService";

        public KbsIntegrationService(ApplicationDbContext context, HttpClient httpClient, Microsoft.Extensions.Logging.ILogger<KbsIntegrationService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SendGuestDataToEgmAsync(int guestRecordId)
        {
            var record = await _context.GuestCheckInRecords
                .Include(g => g.Reservation)
                .FirstOrDefaultAsync(g => g.Id == guestRecordId);

            if (record == null) throw new Exception("Misafir kaydı bulunamadı.");
            
            var facilitySettings = await _context.KbsFacilitySettings.FirstOrDefaultAsync(s => s.IsActive);
            if (facilitySettings == null) throw new Exception("KBS Tesis ayarları bulunamadı. Lütfen EGM bilgilerinizi giriniz.");

            string xmlPayload = GenerateSoapPayload(facilitySettings, record);

            try
            {
                var content = new StringContent(xmlPayload, Encoding.UTF8, "text/xml");
                // var response = await _httpClient.PostAsync(KbsEndpoint, content);
                
                // MOCK BAŞARILI SONUÇ
                var responseStatusCode = System.Net.HttpStatusCode.OK;

                if (responseStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"[MOCK KBS GÖNDERİMİ] Misafir {guestRecordId} başarıyla EGM'ye bildirildi.");
                    record.KbsStatus = KbsBildirimDurumu.GirisBildirildi;
                    
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    record.KbsStatus = KbsBildirimDurumu.Hata;
                    await _context.SaveChangesAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                record.KbsStatus = KbsBildirimDurumu.Hata;
                await _context.SaveChangesAsync();
                return false;
            }
        }

        private string GenerateSoapPayload(KbsFacilitySettings settings, GuestCheckInRecord guest)
        {
            // EGM KBS standartlarına uygun basit bir XML zarfı
            XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace kbs = "http://kbs.egm.gov.tr/";

            var checkInDate = guest.Reservation?.CheckInDate ?? DateTime.UtcNow;

            var doc = new XDocument(
                new XElement(soap + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soap),
                    new XAttribute(XNamespace.Xmlns + "kbs", kbs),
                    new XElement(soap + "Header"),
                    new XElement(soap + "Body",
                        new XElement(kbs + "KonaklamaBildirimi",
                            new XElement("TesisKodu", settings.EgmFacilityCode),
                            new XElement("Sifre", settings.EgmPasswordEncrypted),
                            new XElement("Misafir",
                                new XElement("Ad", guest.FirstName),
                                new XElement("Soyad", guest.LastName),
                                new XElement("KimlikNo", guest.TcOrPassportNo),
                                new XElement("Cinsiyet", guest.Gender),
                                new XElement("DogumTarihi", guest.BirthDate.ToString("yyyy-MM-dd")),
                                new XElement("GirisTarihi", checkInDate.ToString("yyyy-MM-dd HH:mm:ss"))
                            )
                        )
                    )
                )
            );

            return doc.ToString();
        }
    }
}
