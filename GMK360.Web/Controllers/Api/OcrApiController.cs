using GMK360.Web.Services.Ocr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrApiController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrApiController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("extract-identity")]
        public async Task<IActionResult> ExtractIdentity(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Lütfen bir görsel yükleyin.");
            }

            // KVKK gereği dosyayı diske kaydetmeden sadece RAM üzerinde (Stream/byte[]) tutuyoruz
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                // OCR işlemini başlat (Resim işlem biter bitmez GC tarafından temizlenecek)
                var result = _ocrService.ExtractIdentityInfo(imageBytes);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        tcIdentityNo = result.TcIdentityNo,
                        firstName = result.FirstName,
                        lastName = result.LastName,
                        rawText = result.RawText // İsteğe bağlı, debug için dönülebilir
                    });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = result.ErrorMessage });
                }
            }
        }
    }
}
