using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Middleware
{
    public class SubdomainRoutingMiddleware(RequestDelegate next)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host;

            // Kendi ana domainlerinizi ve localhost'u hariç tutun
            var ignoredHosts = new[] { "localhost", "127.0.0.1", "emlakburada.com", "www.emlakburada.com", "gmk360.com", "www.gmk360.com" };

            if (!ignoredHosts.Contains(host, System.StringComparer.OrdinalIgnoreCase))
            {
                // Örn: remax-yildiz.emlakburada.com veya testfirma.localhost veya www.kendiofisdomaini.com
                var hostParts = host.Split('.');
                string subdomain = null;

                if (hostParts.Length >= 3)
                {
                    // Eğer ana domain 'gmk360.com' veya 'emlakburada.com' ise subdomain'i al
                    if (host.EndsWith(".emlakburada.com", System.StringComparison.OrdinalIgnoreCase) || 
                        host.EndsWith(".gmk360.com", System.StringComparison.OrdinalIgnoreCase))
                    {
                        subdomain = hostParts[0];
                    }
                }
                else if (hostParts.Length == 2 && hostParts[1].Equals("localhost", System.StringComparison.OrdinalIgnoreCase))
                {
                    subdomain = hostParts[0];
                }

                context.Items["FullHost"] = host;

                if (!string.IsNullOrEmpty(subdomain) && !subdomain.Equals("www", System.StringComparison.OrdinalIgnoreCase))
                {
                    context.Items["Subdomain"] = subdomain; // Controller'da kullanmak için
                }

                // Subdomain veya CustomDomain olan her isteği AgencyStore'a yönlendir (Sadece ana sayfa ise)
                var path = context.Request.Path.Value;
                if (string.IsNullOrEmpty(path) || path == "/")
                {
                    // Şimdilik AgencyStore'a yönlendiriyoruz. 
                    // İleride Login ve diğer rotalar da tenant'a özel olabilir.
                    context.Request.Path = "/AgencyStore/Index";
                }
            }

            await next(context);
        }
    }
}
