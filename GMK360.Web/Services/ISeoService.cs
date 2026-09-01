using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Web.Services
{
    public interface ISeoService
    {
        Task<SeoSetting?> GetSeoForPathAsync(string path);
    }
}
