using System;
using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public class TicaretBakanligiYetkiServisi : IYetkiBelgesiServisi
    {
        public async Task<bool> BelgeGecerliMiAsync(string yetkiBelgeNo, string tcVeyaVergiNo)
        {
            // Şifreler geldiğinde buraya HttpClient ile REST API veya SOAP kodları yazılacak.
            // var client = new HttpClient();
            // client.DefaultRequestHeaders.Add("Authorization", "Bearer BAKANLIKTAN_GELEN_SIFRE");
            // ... API İstek Kodları ...
            
            throw new NotImplementedException("Bakanlık şifreleri bekleniyor!");
        }
    }
}
