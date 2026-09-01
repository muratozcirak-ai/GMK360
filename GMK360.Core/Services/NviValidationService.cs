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
                var response = await _httpClient.PostAsync("https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx", content);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
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

                return false;
            }
            catch (Exception)
            {
                // Log exception
                return false;
            }
        }
    }
}
