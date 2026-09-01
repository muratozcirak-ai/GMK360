using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IGeminiAiService
    {
        /// <summary>
        /// Kiracının/Müşterinin girdiği serbest metinli arıza talebini analiz edip JSON döner.
        /// (Kategori, Aciliyet Derecesi, Tahmini Gereken Usta Tipi)
        /// </summary>
        Task<string> AnalyzeMaintenanceRequestAsync(string userMessage);

        /// <summary>
        /// Müşteri talebi ile daire özelliklerini kıyaslayıp, eşleşme yüzdesini ve neden uygun olduğunu döner.
        /// </summary>
        Task<string> EvaluateLeadMatchAsync(string leadRequirements, string unitDescription);
        
        /// <summary>
        /// Yüklenen uzun bir sözleşmenin en önemli 3 maddesini (Süre, Ücret, Cayma Bedeli vs.) özetler.
        /// </summary>
        Task<string> SummarizeContractAsync(string contractText);

        /// <summary>
        /// Kullanıcının girdiği mülk özelliklerini analiz edip, piyasa mantığıyla tahmini bir fiyat aralığı ve Değerleme Özeti döner.
        /// </summary>
        Task<string> AnalyzePropertyValuationAsync(GMK360.Core.DTOs.PropertyValuationRequestDto details);

        /// <summary>
        /// Günlük kiralık veya normal kiracı arızalarını analiz edip; Aciliyet, İlk yardım/Tavsiye ve Usta Tipi döner.
        /// </summary>
        Task<string> GenerateActionableFaultReportAsync(string issueDescription);

        /// <summary>
        /// Müşterinin talebine en uygun profesyonelleri (Emlakçı, Usta vs.) puanlayıp eşleştirir.
        /// </summary>
                Task<string> MatchProfessionalAsync(string userRequest, string professionalType);

        /// <summary>
        /// Mahalle bazında otomatik yatırım ve sosyal analiz raporu (HTML/Markdown) çıkarır.
        /// </summary>
        Task<string> AnalyzeNeighborhoodAsync(string districtName, string neighborhoodName);
    }
}

