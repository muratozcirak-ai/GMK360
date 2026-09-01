using Microsoft.AspNetCore.Mvc;
using GMK360.Core.Interfaces;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IGeminiAiService _aiService;
        private readonly IWebhookService _webhookManager;

        public AiController(IGeminiAiService aiService, IWebhookService webhookManager)
        {
            _aiService = aiService;
            _webhookManager = webhookManager;
        }

        [HttpPost("degerleme")]
        public async Task<IActionResult> EvDegerleme([FromBody] DegerlemeRequest request)
        {
            var prompt = $"Bu evin tahmini kira ve satış değeri nedir? Özellikler: Şehir: {request.City}, Oda: {request.RoomCount}, Metrekare: {request.SquareMeters}, Isıtma: {request.HeatingType}";
            
            // AI tahmini al
            var aiResponse = await _aiService.AnalyzePropertyValuationAsync(new GMK360.Core.DTOs.PropertyValuationRequestDto { City = request.City, District = "Merkez", RoomCount = 3, NetSquareMeters = 100, BuildingAge = 5, FloorLevel = 2 });

            // N8n otomasyonuna gönder (rapor pdf oluşturma vs.)
            _ = _webhookManager.SendValuationReportWebhookAsync(new
            {
                Request = request,
                AiEstimation = aiResponse
            });

            return Ok(new { success = true, estimation = aiResponse });
        }

        [HttpPost("usta-bul")]
        public async Task<IActionResult> AkilliUstaBul([FromBody] UstaBulRequest request)
        {
            var prompt = $"Kullanıcı evindeki şu arızayı bildirdi: '{request.ProblemDescription}'. Bu arıza için hangi uzmanlık alanındaki ustaya (Tesisatçı, Elektrikçi, Boyacı vb.) ihtiyaç var? Sadece usta kategorisini kısa cevap olarak ver.";
            
            // AI ile kategori analizi
            var ustaKategorisi = await _aiService.AnalyzeMaintenanceRequestAsync(prompt); // Metin analizi için mevcut metodu kullanıyoruz

            // N8n otomasyonuna gönder (Kullanıcıya usta listesi wp mesajı)
            _ = _webhookManager.SendProfessionalMatchWebhookAsync(new
            {
                Problem = request.ProblemDescription,
                SuggestedCategory = ustaKategorisi
            });

            return Ok(new { success = true, category = ustaKategorisi });
        }
    }

    public class DegerlemeRequest
    {
        public string City { get; set; }
        public string RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string HeatingType { get; set; }
    }

    public class UstaBulRequest
    {
        public string ProblemDescription { get; set; }
    }
}
