using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public class TestYetkiBelgesiServisi : IYetkiBelgesiServisi
    {
        public async Task<bool> BelgeGecerliMiAsync(string yetkiBelgeNo, string tcVeyaVergiNo)
        {
            // Şimdilik 123456 girilirse doğru kabul et, diğerlerini reddet (Test için)
            await Task.Delay(500); // 0.5 saniye bekleme efekti (Gerçek API hissi)
            return yetkiBelgeNo == "123456"; 
        }
    }
}
