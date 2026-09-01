using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public interface IYetkiBelgesiServisi
    {
        // Belge numarası ve TCKimlik/Vergi No ile bakanlığa soracak
        Task<bool> BelgeGecerliMiAsync(string yetkiBelgeNo, string tcVeyaVergiNo);
    }
}
