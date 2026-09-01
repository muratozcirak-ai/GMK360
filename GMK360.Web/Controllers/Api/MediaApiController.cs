using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaApiController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public MediaApiController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("UploadTemp")]
        public async Task<IActionResult> UploadTemp(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Geçersiz dosya.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isVideo = ext == ".mp4" || ext == ".m4v" || ext == ".webm" || ext == ".mov";

            var tempFolder = Path.Combine(_env.WebRootPath, "uploads", "temp");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var fileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(tempFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/temp/{fileName}";

            return Ok(new
            {
                url = url,
                tempPath = fileName,
                isVideo = isVideo
            });
        }
    }
}
