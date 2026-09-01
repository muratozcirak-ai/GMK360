using System;
using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public class MockSmsService : ISmsService
    {
        public Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            // Gerçek entegrasyon yapılana kadar console/log basıyoruz.
            Console.WriteLine($"[MOCK SMS] To: {phoneNumber} | Message: {message}");
            return Task.FromResult(true);
        }

        public Task<bool> SendTemplateSmsAsync(string phoneNumber, string templateCode, object parameters)
        {
            // Şablon işlemleri (ileride db den çekilebilir). Şimdilik mock log:
            Console.WriteLine($"[MOCK SMS TEMPLATE] To: {phoneNumber} | Template: {templateCode} | Params: {System.Text.Json.JsonSerializer.Serialize(parameters)}");
            return Task.FromResult(true);
        }
    }
}
