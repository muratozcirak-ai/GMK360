using System;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GMK360.Data.Contexts; // IRepository/Context için

namespace GMK360.Web.Services.AI
{
    public class AiSupportRouterService : ISupportWebhookHandler
    {
        private readonly IAiSupportAnalyzer _aiAnalyzer;
        private readonly IServiceProvider _serviceProvider; // Kapsamlı DB işlemleri için (Background Service Scope)
        private readonly ILogger<AiSupportRouterService> _logger;

        public AiSupportRouterService(
            IAiSupportAnalyzer aiAnalyzer,
            IServiceProvider serviceProvider,
            ILogger<AiSupportRouterService> logger)
        {
            _aiAnalyzer = aiAnalyzer;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ProcessIncomingMessageAsync(WebhookPayload payload)
        {
            try
            {
                // 1. AI'dan Analiz İste
                var aiResult = await _aiAnalyzer.AnalyzeMessageAsync(payload.MessageText);

                // 2. Karar Motoru (Switch-Case) ve DB Kaydı için Scope oluştur (Çünkü Fire-and-Forget içindeyiz)
                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IRepository<CustomerSupportTicket>>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>(); // Varsa Notification Service

                var ticket = new CustomerSupportTicket
                {
                    SenderId = payload.SenderId,
                    Channel = payload.Channel,
                    OriginalMessage = payload.MessageText,
                    CreatedAt = DateTime.UtcNow
                };

                switch (aiResult.Intent)
                {
                    case "AutoReply":
                        // Zero Friction: Adminin haberi yok, sistem cevapladı.
                        ticket.IsResolvedByAi = true;
                        ticket.AiResponse = aiResult.AiGeneratedResponse;
                        ticket.Status = "AutoResolved";
                        
                        // TODO: WhatsApp API'ye veya SMTP'ye "aiResult.AiGeneratedResponse" mesajını gönder.
                        _logger.LogInformation($"[AI-Otonom Çözüm] -> Kullanıcıya mesaj gönderildi: {aiResult.AiGeneratedResponse}");
                        break;

                    case "HumanEscalation":
                    default:
                        // Karmaşık sorun: İnsana (Admin'e) aktar.
                        ticket.IsResolvedByAi = false;
                        ticket.Status = "PendingAdmin";
                        
                        // TODO: Kullanıcıya "Talebiniz uzman ekibimize iletilmiştir." mesajı gönder.
                        _logger.LogInformation($"[Human-Escalation] -> Admin paneline ticket düşürüldü. Gönderen: {payload.SenderId}");
                        break;
                }

                await repo.AddAsync(ticket);
                _logger.LogInformation($"Ticket başarıyla veritabanına kaydedildi. ID: {ticket.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj işlenirken hata oluştu.");
            }
        }
    }
}
