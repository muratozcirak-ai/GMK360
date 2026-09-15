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
            var knownControllers = new[] { "Account", "Admin", "AdminCRM", "AdminFinance", "AdminLegalDocument", "AdminLocation", "AdminSeo", "AdminTeam", "AgencyStore", "AgencyWorkers", "Agreement", "AiAssistant", "B2BPurchasing", "Blog", "BuildingManager", "Campaign", "Commission", "ConstructionProject", "Contact", "Contract", "Crm", "CustomerDashboard", "CustomerPortal", "DailyRentalAgenda", "DailyRental", "DailyTimesheets", "Dashboard", "Definition", "DigitalHome", "DocumentArchive", "Export", "FeatureManager", "Financial", "Home", "Inventory", "InventoryReceipts", "Location", "MaterialList", "Meeting", "Message", "MobileTask", "Notifications", "Offer", "Payment", "PhaseWorkerDemands", "Profile", "ProjectMaterial", "Property", "Pwa", "Realtor", "Reservation", "ResidentPortal", "Sandbox", "Search", "Seed", "Service", "ServiceProvider", "Sitemap", "Solutions", "SupplierCurrentAccounts", "SupplierCurrentAccount", "SubcontractorContract", "Timesheet", "Finance", "PartnerPortal", "AgencyStaffFinance", "AgencyStaff", "AgencyPhonebook", "Agenda", "TaxAssistant", "Usta", "Valuation", "Wallet", "Auth", "Subscription" };;
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


