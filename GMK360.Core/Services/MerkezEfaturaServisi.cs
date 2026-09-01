using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GMK360.Core.DTOs;
using GMK360.Core.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace GMK360.Core.Services
{
    public class MerkezEfaturaServisi : IEfaturaServisi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MerkezEfaturaServisi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<KesilenFatura> FaturaKesAsync(FaturaGonderimDTO faturadto)
        {
            // 1. appsettings.json'dan API bilgilerini okuma
            var apiUrl = _configuration["EfaturaSettings:ApiUrl"];
            var apiUser = _configuration["EfaturaSettings:KullaniciAdi"];
            var apiPass = _configuration["EfaturaSettings:Sifre"];
            var saticiVkn = _configuration["EfaturaSettings:SaticiVKN"];

            // 2. Entegratöre Gönderilecek JSON veya XML yapısını oluşturma (Örnek Taslak)
            /*
            var entegratorIstek = new {
                Username = apiUser,
                Password = apiPass,
                VknTckn = faturadto.AliciVKN_TCKN,
                Total = faturadto.ToplamTutar,
                Type = faturadto.FaturaTipi == FaturaTipi.EFatura ? "E-Fatura" : "E-Arşiv"
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(entegratorIstek), Encoding.UTF8, "application/json");
            
            // 3. API'ye İstek Atma
            var response = await _httpClient.PostAsync(apiUrl, jsonContent);
            var responseStr = await response.Content.ReadAsStringAsync();
            */

            // 4. API Yanıtına Göre Sistemimize Kaydedilecek Fatura Nesnesini Doldurma
            // Şimdilik başarılı varsayıp sahte (mock) veri dönüyoruz. API bağlandığında burası dinamik olacak.
            
            var yeniFatura = new KesilenFatura
            {
                ETTN = Guid.NewGuid(),
                FaturaNo = "EML" + DateTime.Now.Year + "000000001",
                AliciUnvan = faturadto.AliciUnvan,
                AliciVKN_TCKN = faturadto.AliciVKN_TCKN,
                ToplamTutar = faturadto.ToplamTutar,
                FaturaTipi = faturadto.FaturaTipi,
                Durum = FaturaDurumu.Basarili, // Gerçek senaryoda API'den gelen sonuca göre
                Tarih = DateTime.UtcNow
            };

            return yeniFatura;
        }

        public async Task<FaturaDurumu> FaturaDurumSorgulaAsync(string ettn)
        {
            // İleride faturanın GİB'e gidip gitmediğini sorgulamak için kullanılacak
            throw new NotImplementedException();
        }
    }
}
