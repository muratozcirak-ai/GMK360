using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

namespace GMK360.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WebhookController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class WhatsappWebhookPayload
        {
            public string PhoneNumber { get; set; } = null!;
            public string Action { get; set; } = null!; // Örn: "ApproveReference"
            public string ReferenceLogId { get; set; } = null!;
        }

        [HttpPost("whatsapp")]
        public async Task<IActionResult> HandleWhatsappCallback([FromBody] WhatsappWebhookPayload payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.Action))
                return BadRequest("Invalid payload.");

            if (payload.Action == "ApproveReference")
            {
                if (!int.TryParse(payload.ReferenceLogId, out int logId))
                    return BadRequest("Invalid ReferenceLogId");

                var referenceLog = await _context.ReferenceLogs
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == logId && r.TargetPhoneNumber == payload.PhoneNumber);

                if (referenceLog == null)
                    return NotFound("Reference log not found or phone mismatch.");

                if (referenceLog.Status != "Approved")
                {
                    referenceLog.Status = "Approved";
                    referenceLog.ApprovedAt = DateTime.UtcNow;
                    // IP adresini request üzerinden alıyoruz (Eğer n8n veya Twilio gibi bir aracıdan geliyorsa IP onlarınki olur, ama loglama maksadıyla)
                    referenceLog.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                    _context.ReferenceLogs.Update(referenceLog);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Reference approved successfully." });
            }

            return BadRequest("Unknown action.");
        }
    }
}
