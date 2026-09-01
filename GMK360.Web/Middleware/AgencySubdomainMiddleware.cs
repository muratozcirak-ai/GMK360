using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Middleware
{
    public class AgencySubdomainMiddleware
    {
        private readonly RequestDelegate _next;

        public AgencySubdomainMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host.ToLower();
            
            // Eðer doðrudan IP veya sadece localhost ise yoksay
            if (host == "localhost" || host == "127.0.0.1" || host == "gmk360.com")
            {
                await _next(context);
                return;
            }

            var hostParts = host.Split('.');
            
            // Ornek: ahmetemlak.gmk360.com (3 parca) veya ahmetemlak.localhost (2 parca - port Host.Host'ta yer almaz)
            if (hostParts.Length >= 2)
            {
                var subdomain = hostParts[0];

                // Özel ayrýlmýþ subdomainler (www, admin, api vb.) deðilse kurumsal maðazaya yönlendir.
                string[] reservedSubdomains = { "www", "admin", "api", "mail", "ftp" };
                
                if (!reservedSubdomains.Contains(subdomain))
                {
                    // /AgencyStore/Index sayfasýna request'i arka planda rewrite edelim.
                    // Fakat kullanici tarayicida hala "ahmetemlak.gmk360.com/iletisim" gorecek.
                    
                    // Sadece anasayfa ise AgencyStore/Index'e yönlendir. 
                    // Eger /iletisim gibi bir seyse AgencyStore/Contact'a yonlendirebiliriz. (Ileride gelistirilebilir)
                    if (context.Request.Path == "/")
                    {
                        context.Request.Path = "/AgencyStore/Index";
                        context.Request.QueryString = context.Request.QueryString.Add("subdomain", subdomain);
                    }
                }
            }

            await _next(context);
        }
    }
}
