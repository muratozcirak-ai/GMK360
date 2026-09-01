using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [ApiController]
    [Route("api/v1/properties")]
    public class IntegrationApiController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProperties([FromHeader(Name = "X-Api-Key")] string apiKey, [FromQuery] string ilce, [FromQuery] string status)
        {
            // TODO: API Key doğrulama, rate limiting ve JSON ilan dönüşü
            if (string.IsNullOrEmpty(apiKey))
            {
                return Unauthorized(new { message = "API Key is required." });
            }

            return Ok(new { message = "Integration API is working." });
        }
    }
}
