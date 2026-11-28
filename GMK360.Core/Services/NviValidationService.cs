using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GMK360.Core.Services
{
    public class NviValidationService : INviValidationService
    {
        private readonly HttpClient _httpClient;

        public NviValidationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ValidateTcIdentityAsync(string tcIdentityNo, string firstName, string lastName, int birthYear)
        {
            if (string.IsNullOrWhiteSpace(tcIdentityNo) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return false;
            }
            if (birthYear < 1900 || birthYear > DateTime.Today.Year)
            {
                return false;
            }

            // Beta/KPS geçis modu: KPSPublic.asmx 30 Eylül 2025 itibarıyla kapatıldığı için
            // resmi KPS v2 kurumsal izni aktif edilene kadar (NVI_USE_REMOTE_KPS=true olmadıkça)
            // veya uzak servis yanıt vermediğinde resmi 11 haneli TCKN algoritma doğrulamasına düşülür.
            bool useRemote = string.Equals(Environment.GetEnvironmentVariable("NVI_USE_REMOTE_KPS"), "true", StringComparison.OrdinalIgnoreCase);
            if (!useRemote)
            {
                return IsValidTcChecksum(tcIdentityNo);
            }

            try
            {
                string soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <TCKimlikNoDogrula xmlns=""http://tckimlik.nvi.gov.tr/WS"">
      <TCKimlikNo>{tcIdentityNo}</TCKimlikNo>
      <Ad>{firstName.ToUpper(new System.Globalization.CultureInfo("tr-TR"))}</Ad>
      <Soyad>{lastName.ToUpper(new System.Globalization.CultureInfo("tr-TR"))}</Soyad>
      <DogumYili>{birthYear}</DogumYili>
    </TCKimlikNoDogrula>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                _httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await _httpClient.PostAsync("https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx", content);

                if (!response.IsSuccessStatusCode)
                {
                    return IsValidTcChecksum(tcIdentityNo);
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // Parse XML to get TCKimlikNoDogrulaResult
                XDocument doc = XDocument.Parse(responseString);
                XNamespace ns = "http://tckimlik.nvi.gov.tr/WS";

                var resultNode = doc.Descendants(ns + "TCKimlikNoDogrulaResult").FirstOrDefault();

                if (resultNode != null && bool.TryParse(resultNode.Value, out bool result))
                {
                    return result;
                }

                return IsValidTcChecksum(tcIdentityNo);
            }
            catch (Exception)
            {
                return IsValidTcChecksum(tcIdentityNo);
            }
        }

        private static bool IsValidTcChecksum(string? rawTc)
        {
            var tc = new string((rawTc ?? "").Where(char.IsDigit).ToArray());
            if (tc.Length != 11 || tc[0] == '0') return false;
            int[] d = tc.Select(c => c - '0').ToArray();
            int oddSum = d[0] + d[2] + d[4] + d[6] + d[8];
            int evenSum = d[1] + d[3] + d[5] + d[7];
            int check10 = ((oddSum * 7) - evenSum) % 10;
            if (check10 < 0) check10 += 10;
            if (check10 != d[9]) return false;
            int first10Sum = 0;
            for (int i = 0; i < 10; i++) first10Sum += d[i];
            return (first10Sum % 10) == d[10];
        }
    }
}
