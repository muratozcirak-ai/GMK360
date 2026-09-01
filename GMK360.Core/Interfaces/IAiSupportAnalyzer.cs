using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public class AiSupportResult
    {
        public string Intent { get; set; } = null!; // "AutoReply" veya "HumanEscalation"
        public string? AiGeneratedResponse { get; set; } // AutoReply ise müşteriye dönülecek metin
    }

    public interface IAiSupportAnalyzer
    {
        // Mesajı Gemini API'ye gönderir ve JSON cevabını sınıfa çevirip döner
        Task<AiSupportResult> AnalyzeMessageAsync(string messageText);
    }
}
