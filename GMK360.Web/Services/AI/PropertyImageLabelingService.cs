using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace GMK360.Web.Services.AI
{
    // Giriş verisi: Sadece resim yolu
    public class ModelInput
    {
        [LoadColumn(0)]
        public string ImagePath;
    }

    // Çıkış verisi: AI Tahmin Sonucu
    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }

        public float[] Score { get; set; }
    }

    public interface IAiImageLabelingService
    {
        string PredictImageLabel(string imagePath);
    }

    public class PropertyImageLabelingService : IAiImageLabelingService
    {
        private readonly MLContext _mlContext;
        private readonly string _modelPath;
        private PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        public PropertyImageLabelingService()
        {
            _mlContext = new MLContext();
            
            // Eğitimli model dosyasının yolu
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ai-models", "RoomClassifierModel.zip");

            // Not: İlk kurulumda model.zip olmadığı için bir exception atmaması adına 
            // try-catch bloğunda model yüklemeyi deniyoruz.
            try
            {
                if (File.Exists(_modelPath))
                {
                    ITransformer mlModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Model yüklenemedi: {ex.Message}");
            }
        }

        public string PredictImageLabel(string imagePath)
        {
            // Eğer model yüklendiyse (ve dosya varsa) ML.NET'i çalıştır
            if (_predictionEngine != null && File.Exists(imagePath))
            {
                var input = new ModelInput { ImagePath = imagePath };
                var result = _predictionEngine.Predict(input);
                
                // Belirli bir güven skorunun üzerindeyse döndür
                // (Örn: Eğer skorları okumak isterseniz result.Score array'ine bakabilirsiniz)
                return result.Prediction;
            }

            // --- STUB / MOCK ---
            // Sistemde gerçek model henüz eğitilmediği için simüle edilen sonuçlar
            return SimulateAiPrediction(imagePath);
        }

        private string SimulateAiPrediction(string imagePath)
        {
            // İsim içinde geçen kelimelere göre mock yapalım veya rastgele bir şey dönelim
            var lowerPath = imagePath.ToLower();
            if (lowerPath.Contains("banyo") || lowerPath.Contains("bath"))
                return "Banyo";
            if (lowerPath.Contains("mutfak") || lowerPath.Contains("kitchen"))
                return "Mutfak";
            if (lowerPath.Contains("salon") || lowerPath.Contains("living"))
                return "Salon";
            if (lowerPath.Contains("yatak") || lowerPath.Contains("bed"))
                return "Yatak Odası";
            if (lowerPath.Contains("balkon") || lowerPath.Contains("balcony"))
                return "Balkon";

            // Eğer tespit edilemiyorsa varsayılan
            return "Oda";
        }
    }
}
