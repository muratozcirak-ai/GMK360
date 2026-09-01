using GMK360.Web.Services.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyImageApiController : ControllerBase
    {
        private readonly IAiImageLabelingService _aiImageService;

        public PropertyImageApiController(IAiImageLabelingService aiImageService)
        {
            _aiImageService = aiImageService;
        }

        [HttpPost("upload-and-label")]
        public async Task<IActionResult> UploadAndLabel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Geçersiz dosya.");
            }

            // Resmi geçici klasöre kaydet
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "property_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Dosya ismi çakışmasını önlemek için Guid
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ML.NET AI ile resmi analiz et
            var label = _aiImageService.PredictImageLabel(filePath);
            var isVideo = file.ContentType.StartsWith("video/");

            return Ok(new
            {
                success = true,
                filePath = $"/uploads/property_images/{fileName}",
                tempPath = filePath,
                fileName = fileName,
                isVideo = isVideo,
                aiLabel = label
            });
        }
    }
}
