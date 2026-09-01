using System;
using System.IO;
using System.Text.RegularExpressions;
using Tesseract;

namespace GMK360.Web.Services.Ocr
{
    public class OcrResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string TcIdentityNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RawText { get; set; }
    }

    public interface IOcrService
    {
        OcrResult ExtractIdentityInfo(byte[] imageBytes);
    }

    public class TesseractOcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public TesseractOcrService()
        {
            // Proje kök dizinindeki tessdata klasörü
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }

        public OcrResult ExtractIdentityInfo(byte[] imageBytes)
        {
            var result = new OcrResult();

            try
            {
                using (var engine = new TesseractEngine(_tessDataPath, "tur", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(imageBytes))
                    {
                        using (var page = engine.Process(img))
                        {
                            result.RawText = page.GetText();
                            ParseIdentityInfo(result.RawText, result);
                            result.Success = true;
                        }
                    } // img.Dispose() otomatik çalışır, RAM'den silinir.
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "Okuma Hatası: " + ex.Message;
            }

            return result;
        }

        private void ParseIdentityInfo(string rawText, OcrResult result)
        {
            // Temel Regex pattern'leri (Gerçek senaryolarda daha kompleks hale getirilebilir)
            // TC Kimlik: 11 haneli sayı
            var tcMatch = Regex.Match(rawText, @"\b[1-9][0-9]{10}\b");
            if (tcMatch.Success)
            {
                result.TcIdentityNo = tcMatch.Value;
            }

            // Not: Yeni nesil kimlik kartlarında "Soyadı / Surname", "Adı / Given Name" şeklindedir.
            // Satır satır arama yapacağız.
            var lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                // Soyadı tespiti
                if ((line.Contains("Soyadı") || line.Contains("Surname")) && i + 1 < lines.Length)
                {
                    if (string.IsNullOrEmpty(result.LastName))
                    {
                        // Alt satırı al
                        result.LastName = CleanName(lines[i + 1]);
                    }
                }
                
                // Adı tespiti
                if ((line.Contains("Adı") || line.Contains("Name")) && !line.Contains("Soyadı") && i + 1 < lines.Length)
                {
                    if (string.IsNullOrEmpty(result.FirstName))
                    {
                        result.FirstName = CleanName(lines[i + 1]);
                    }
                }
            }
        }

        private string CleanName(string name)
        {
            // Fazlalık olabilecek özel karakterleri temizle
            return Regex.Replace(name, @"[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]", "").Trim();
        }
    }
}
