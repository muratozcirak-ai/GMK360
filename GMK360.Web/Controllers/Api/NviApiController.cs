using GMK360.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NviApiController : ControllerBase
    {
        private readonly INviValidationService _nviValidationService;

        public NviApiController(INviValidationService nviValidationService)
        {
            _nviValidationService = nviValidationService;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTcIdentity([FromBody] TcValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TcKimlikNo) || string.IsNullOrWhiteSpace(request.Ad) || string.IsNullOrWhiteSpace(request.Soyad) || request.DogumYili <= 0)
            {
                return BadRequest(new { success = false, message = "Eksik bilgi gönderildi." });
            }

            if (request.TcKimlikNo.Length != 11)
            {
                return BadRequest(new { success = false, message = "TC Kimlik Numarası 11 haneli olmalıdır." });
            }

            bool isValid = await _nviValidationService.ValidateTcIdentityAsync(request.TcKimlikNo, request.Ad, request.Soyad, request.DogumYili);

            if (isValid)
            {
                return Ok(new { success = true, message = "Kimlik doğrulandı." });
            }
            else
            {
                return Ok(new { success = false, message = "Kimlik bilgileri hatalı veya NVİ ile eşleşmedi." });
            }
        }
    }

    public class TcValidationRequest
    {
        public string TcKimlikNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int DogumYili { get; set; }
    }
}
