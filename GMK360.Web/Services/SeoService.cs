using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public class SeoService : ISeoService
    {
        private readonly ApplicationDbContext _context;

        public SeoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SeoSetting?> GetSeoForPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                path = "/Home/Index";
            }
            
            // Tam eslesme arayalim
            var seo = await _context.SeoSettings.FirstOrDefaultAsync(s => s.PagePath.ToLower() == path.ToLower());
            
            // Eger bulamazsa genel site ayari (PagePath = "*") var mi diye bakalim
            if (seo == null)
            {
                seo = await _context.SeoSettings.FirstOrDefaultAsync(s => s.PagePath == "*");
            }

            return seo;
        }
    }
}
