using GMK360.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TtbsApiController : ControllerBase
    {
        private readonly IYetkiBelgesiServisi _yetkiBelgesiServisi;

        public TtbsApiController(IYetkiBelgesiServisi yetkiBelgesiServisi)
        {
            _yetkiBelgesiServisi = yetkiBelgesiServisi;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTtbs([FromBody] TtbsValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.YetkiBelgeNo) || string.IsNullOrWhiteSpace(request.TcNkVeyaVergiNo))
            {
                return BadRequest(new { success = false, message = "Eksik bilgi gönderildi." });
            }

            bool isValid = await _yetkiBelgesiServisi.BelgeGecerliMiAsync(request.YetkiBelgeNo, request.TcNkVeyaVergiNo);

            if (isValid)
            {
                return Ok(new { success = true, message = "Yetki Belgesi doğrulandı." });
            }
            else
            {
                return Ok(new { success = false, message = "Yetki Belgesi onaylanamadı. Lütfen numarayı kontrol edip tekrar deneyin." });
            }
        }
    }

    public class TtbsValidationRequest
    {
        public string YetkiBelgeNo { get; set; }
        public string TcNkVeyaVergiNo { get; set; }
    }
}
