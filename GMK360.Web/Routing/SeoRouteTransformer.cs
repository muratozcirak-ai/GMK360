using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace GMK360.Web.Routing
{
    public class SeoRouteTransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            if (!values.TryGetValue("city", out var cityObj) || !values.TryGetValue("district", out var districtObj) || !values.TryGetValue("seoSlug", out var slugObj))
            {
                return ValueTask.FromResult(values);
            }

            var city = cityObj?.ToString();
            var knownControllers = new[] { "DigitalHome", "Admin", "Home", "Property", "Account", "Subscription", "Blog", "AgencyStore", "Auth" };
            if (city != null && knownControllers.Contains(city, System.StringComparer.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult(values);
            }

            var slug = slugObj?.ToString();
            if (string.IsNullOrEmpty(slug))
            {
                return ValueTask.FromResult(values);
            }

            // Slug sonundaki ID'yi kontrol et
            var parts = slug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[^1], out _))
            {
                // Geçerli bir ID ile bitmiyorsa, normal Controller/Action işlemine bırakalım
                return ValueTask.FromResult(values);
            }

            // Eğer usta ise UstaController'a yönlendir
            if (slug.Contains("ustas") || slug.Contains("temizlik") || slug.Contains("nakliyat"))
            {
                values["controller"] = "Usta";
                values["action"] = "DetailBySlug";
            }
            else
            {
                values["controller"] = "Property";
                values["action"] = "DetailBySlug";
            }

            return ValueTask.FromResult(values);
        }
    }
}
