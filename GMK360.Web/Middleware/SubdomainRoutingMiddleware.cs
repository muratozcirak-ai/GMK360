using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Middleware
{
    public class SubdomainRoutingMiddleware(RequestDelegate next)
    {
        private static readonly string[] IgnoredHosts =
        {
            "localhost", "127.0.0.1", "emlakburada.com", "www.emlakburada.com", "gmk360.com", "www.gmk360.com"
        };

        private static readonly string[] ReservedSubdomains = { "www", "admin", "api", "mail", "ftp", "cdn" };

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var host = context.Request.Host.Host ?? string.Empty;
            var path = context.Request.Path.Value ?? string.Empty;

            // Statik dosyalar için DB sorgusu yapma
            if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            if (!string.IsNullOrEmpty(host) && !IgnoredHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
            {
                var hostParts = host.Split('.');
                string? subdomain = null;

                if (hostParts.Length >= 3)
                {
                    if (host.EndsWith(".emlakburada.com", StringComparison.OrdinalIgnoreCase)
                        || host.EndsWith(".gmk360.com", StringComparison.OrdinalIgnoreCase))
                    {
                        subdomain = hostParts[0];
                    }
                }
                else if (hostParts.Length == 2 && hostParts[1].Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    subdomain = hostParts[0];
                }

                context.Items["FullHost"] = host;

                if (!string.IsNullOrEmpty(subdomain) && !ReservedSubdomains.Contains(subdomain, StringComparer.OrdinalIgnoreCase))
                {
                    subdomain = subdomain.Trim().ToLowerInvariant();
                    context.Items["Subdomain"] = subdomain;

                    // 1. Bağımsız Site (HousingComplex) veya Müstakil Bina (Building) kontrolü
                    var complex = await db.HousingComplexes.AsNoTracking()
                        .FirstOrDefaultAsync(c => !c.IsDeleted && c.Subdomain == subdomain);

                    if (complex != null)
                    {
                        context.Items["TenantType"] = "Site";
                        context.Items["TenantComplexId"] = complex.Id;
                        context.Items["TenantName"] = complex.Name;
                    }
                    else
                    {
                        var building = await db.Buildings.AsNoTracking()
                            .FirstOrDefaultAsync(b => b.Subdomain == subdomain);
                        if (building != null)
                        {
                            context.Items["TenantType"] = "Site";
                            context.Items["TenantBuildingId"] = building.Id;
                            context.Items["TenantName"] = building.Name;
                        }
                        else
                        {
                            var mgmt = await db.ManagementCompanies.AsNoTracking()
                                .FirstOrDefaultAsync(m => !m.IsDeleted && m.Subdomain == subdomain);
                            if (mgmt != null)
                            {
                                context.Items["TenantType"] = "ManagementCompany";
                                context.Items["TenantManagementCompanyId"] = mgmt.Id;
                                context.Items["TenantName"] = mgmt.CompanyName;
                            }
                            else
                            {
                                var agency = await db.Agencies.AsNoTracking()
                                    .FirstOrDefaultAsync(a => !a.IsDeleted && (a.Subdomain == subdomain || a.CustomDomain == host));
                                if (agency != null)
                                {
                                    bool isConstruction = await db.ConstructionProjects.AsNoTracking()
                                            .AnyAsync(p => !p.IsDeleted && p.AgencyId == agency.Id)
                                        || await db.AgencyConsultants.AsNoTracking()
                                            .Include(c => c.User)
                                            .AnyAsync(c => c.AgencyId == agency.Id && c.IsActive
                                                && c.User != null && (c.User.ActiveModules ?? "").Contains("construction"));

                                    context.Items["TenantType"] = isConstruction ? "Construction" : "Agency";
                                    context.Items["TenantAgencyId"] = agency.Id;
                                    context.Items["TenantName"] = agency.CompanyName;
                                }
                            }
                        }
                    }
                }

                // Kök istekte kuruma özgü karşılama kapısına yönlendir
                if (string.IsNullOrEmpty(path) || path == "/")
                {
                    var tenantType = context.Items["TenantType"] as string;
                    if (tenantType == "Site" || tenantType == "ManagementCompany")
                    {
                        context.Request.Path = "/TenantPortal/Site";
                    }
                    else if (tenantType == "Construction")
                    {
                        context.Request.Path = "/TenantPortal/Construction";
                    }
                    else
                    {
                        context.Request.Path = "/AgencyStore/Index";
                    }
                }
            }

            await next(context);
        }
    }
}
