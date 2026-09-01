using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/webhook/[controller]")]
    [ApiController]
    public class SupportWebhookController : ControllerBase
    {
        private readonly ISupportWebhookHandler _webhookHandler;

        public SupportWebhookController(ISupportWebhookHandler webhookHandler)
        {
            _webhookHandler = webhookHandler;
        }

        [HttpPost("whatsapp")]
        public IActionResult ReceiveWhatsAppMessage([FromBody] WebhookPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.MessageText))
            {
                return BadRequest("Invalid payload.");
            }

            payload.Channel = "WhatsApp";

            // Fire and Forget (Asenkron olarak arka plana devret)
            // Gerçek dünyada burada BackgroundJob (Hangfire/RabbitMQ) kullanılır ancak basit versiyonda Task.Run kullanılabilir.
            _ = Task.Run(async () => 
            {
                await _webhookHandler.ProcessIncomingMessageAsync(payload);
            });

            // Webhook gönderen sisteme (WhatsApp API) anında 200 OK dönüyoruz.
            return Ok(new { success = true, message = "Message received and queued." });
        }
    }
}
