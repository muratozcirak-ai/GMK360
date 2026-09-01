using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    // Webhook'tan gelen ham veriyi (Payload) temsil eder
    public class WebhookPayload
    {
        public string SenderId { get; set; } = null!;
        public string MessageText { get; set; } = null!;
        public string Channel { get; set; } = "WhatsApp"; // Opsiyonel
    }

    public interface ISupportWebhookHandler
    {
        // Gelen mesajı asenkron olarak arka plana iletir
        Task ProcessIncomingMessageAsync(WebhookPayload payload);
    }
}
