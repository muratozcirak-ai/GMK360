using GMK360.Data.Contexts;
using GMK360.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml;
using System.Globalization;

namespace GMK360.Web.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("sitemap.xml")]
        public async Task<IActionResult> Index()
        {
            var domain = $"{Request.Scheme}://{Request.Host}";
            var builder = new StringBuilder();

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                // Ana Sayfa
                AddUrl(writer, $"{domain}/", "1.0", "daily");

                // Sabit Sayfalar
                AddUrl(writer, $"{domain}/Property", "0.8", "daily");
                AddUrl(writer, $"{domain}/Agent", "0.7", "weekly");
                AddUrl(writer, $"{domain}/ServiceProvider", "0.8", "weekly");
                AddUrl(writer, $"{domain}/Blog", "0.7", "weekly");

                // 1. EMLAK İLANLARI
                var properties = await _context.Properties
                    .Include(p => p.Complex).ThenInclude(c => c.City)
                    .Include(p => p.Complex).ThenInclude(c => c.District)
                    .Include(p => p.Status)
                    .Include(p => p.Type)
                    .Include(p => p.User)
                        .ThenInclude(u => u.AgencyConsultants)
                            .ThenInclude(ac => ac.Agency)
                    .Where(p => p.State == GMK360.Core.Entities.ListingState.Active && !p.IsDeleted)
                    .ToListAsync();

                foreach (var prop in properties)
                {
                    // Slug Kurgusu: /ilan/istanbul-avcilar-satilik-daire-1045
                    var cityStr = prop.Complex?.City?.Name.ToSeoUrl() ?? "il";
                    var distStr = prop.Complex?.District?.Name.ToSeoUrl() ?? "ilce";
                    var statusStr = prop.Status?.Name.ToSeoUrl() ?? "satilik";
                    var typeStr = prop.Type?.Name.ToSeoUrl() ?? "emlak";
                    var catStr = $"{statusStr}-{typeStr}";
                    var titleStr = prop.Title.ToSeoUrl();
                    
                    var slug = $"{cityStr}-{distStr}-{catStr}-{titleStr}-{prop.Id}";

                    // Danışman (Subdomain) kontrolü
                    string itemDomain = domain; // Varsayılan: emlakburada.com
                    var consultant = prop.User;
                    if (consultant != null && consultant.AgencyConsultants != null && consultant.AgencyConsultants.Any(ac => ac.IsActive))
                    {
                        var agency = consultant.AgencyConsultants.First(ac => ac.IsActive).Agency;
                        if (!string.IsNullOrEmpty(agency.Subdomain))
                        {
                            // Örn: https://hasanemlak.emlakburada.com
                            itemDomain = $"{Request.Scheme}://{agency.Subdomain}.{Request.Host.Host.Replace("www.", "")}";
                        }
                    }

                    var url = $"{itemDomain}/ilan/{slug}";
                    
                    AddUrl(writer, url, "0.9", "weekly", prop.UpdatedAt ?? prop.CreatedAt);
                }

                // 2. USTALAR / ÇÖZÜM ORTAKLARI (MODÜL 9)
                // ServiceProvider'lar
                var providers = await _context.ServiceProviders
                    .Include(p => p.Areas).ThenInclude(a => a.District)
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();
                
                // NOT: City join yapılmadığı için District üzerinden veya BusinessName'den slug üretiyoruz
                foreach (var prov in providers)
                {
                    // Slug Kurgusu: /usta/kadikoy-hasan-usta-10
                    var distStr = prov.Areas?.FirstOrDefault()?.District?.Name.ToSeoUrl() ?? "bolge";
                    var nameStr = prov.BusinessName.ToSeoUrl();
                    
                    var slug = $"{distStr}-{nameStr}-{prov.Id}";
                    var url = $"{domain}/usta/{slug}";

                    AddUrl(writer, url, "0.8", "monthly", prov.UpdatedAt ?? prov.CreatedAt);
                }

                // 3. MAKALELER (MODÜL 7)
                var articles = await _context.Articles
                    .Where(a => !a.IsDeleted && !a.IsMembersOnly)
                    .ToListAsync();

                foreach (var article in articles)
                {
                    var slug = $"{article.Title.ToSeoUrl()}-{article.Id}";
                    var url = $"{domain}/blog/{slug}";

                    AddUrl(writer, url, "0.6", "monthly", article.PublishedAt);
                }

                writer.WriteEndElement(); // urlset
                writer.WriteEndDocument();
            }

            return Content(builder.ToString(), "application/xml", Encoding.UTF8);
        }

        private void AddUrl(XmlWriter writer, string url, string priority, string changeFreq, DateTime? lastMod = null)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", url);
            writer.WriteElementString("priority", priority);
            writer.WriteElementString("changefreq", changeFreq);
            
            if (lastMod.HasValue)
            {
                // W3C Datetime format
                writer.WriteElementString("lastmod", lastMod.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
            }
            
            writer.WriteEndElement();
        }
    }
}
