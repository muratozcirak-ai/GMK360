using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GMK360.Core.Entities;

namespace GMK360.Web.Helpers
{
    public class StaffHrDossier
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? BloodGroup { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? TitleOrPosition { get; set; }
        public string? Notes { get; set; }
    }

    public static class StaffPermissionHelper
    {
        public const string PermCandidate = "cand";   // Şantiye & Projeler — 1. Evre: Aday Proje & Teklifler
        public const string PermDna = "dna";          // Şantiye & Projeler — 2. Evre: Anlaşma, Çizim & DNA Metraj
        public const string PermSite = "site";        // Şantiye & Projeler — 3. Evre: Aktif Şantiye Saha & Puantaj
        public const string PermFinance = "fin";      // Sol Menü: Finans & Cari (Ana Kasa, Banka, Çek & Hakediş)
        public const string PermPurchasing = "pur";   // Sol Menü: Satın Alma & Taşeron İhale Masası
        public const string PermArchive = "doc";      // Sol Menü: Evrak & Dosya Arşivi
        public const string PermCrm = "crm";          // Sol Menü: Satış & Daire Portföyü
        public const string PermStaff = "staff";      // Sol Menü: Personel & Ekipler Yönetimi
        public const string PermInventory = "inv";    // Sol Menü: Demirbaş & Merkez Depo
        public const string PermWebsite = "web";      // Sol Menü: Kurumsal Web & Domain Yönetimi
        public const string PermStrategy = "strategy";// Üst Bar: Yönetim Strateji & AI Karar Merkezi

        // Her personelde otomatik açık olan sabit ana ekranlar
        public static readonly (string Title, string Description, string Icon)[] FixedMenuDefinitions = new[]
        {
            ("Özetim & Profilim", "Kişisel profil, şifre/onay PIN ayarları ve kendisine atanan görev özeti (Sabit Açık).", "bi-person-vcard text-primary"),
            ("Toplantı & Karar Masası", "Şirket içi toplantı gündemleri, alınan kararlar ve saha koordinasyon notları (Sabit Açık).", "bi-people-fill text-warning"),
            ("Şirket Garajı & Araçlar (Zimmet & Yakıt Fişi)", "Zimmetli aracını görme, km kaydı ve şantiyede alınan yakıt/bakım fişi fotoğrafını anında yükleme (Sabit Açık).", "bi-truck text-success")
        };

        // Yöneticinin tek tek açıp kapatabileceği sol menü linkleri ve proje evre yetkileri
        public static readonly (string Code, string Title, string Description, string Icon)[] AllPermissionDefinitions = new[]
        {
            (PermCandidate, "🏗️ Şantiye & Projeler — 1. Evre (Aday Proje & Teklifler)", "Ön etüd, imar/zemin incelemesi ve arsa sahibi teklif hazırlık ekranlarını görebilir.", "bi-search text-warning"),
            (PermDna, "📐 Şantiye & Projeler — 2. Evre (Anlaşma, Çizim & DNA Metraj)", "Resmi ruhsat evrakları, DWG/DXF çizim arşivi, Proje DNA metrajı ve iş programını yönetebilir.", "bi-diagram-3-fill text-primary"),
            (PermSite, "👷 Şantiye & Projeler — 3. Evre (Aktif Şantiye Saha & Puantaj)", "Günlük saha raporu, puantaj, İSG kontrolü, malzeme irsaliye kabulü ve mobil saha ekranlarını kullanabilir.", "bi-cone-striped text-danger"),
            (PermFinance, "💰 Finans & Cari (Ana Kasa, Banka, Çek & Hakediş)", "Sol menüdeki Finans & Cari başlığını, şirket ana kasasını, çek vadelerini ve hakediş ödemelerini görebilir.", "bi-cash-coin text-success"),
            (PermPurchasing, "🛒 Satın Alma & Taşeron İhale Masası", "Sol menüdeki Satın Alma & Taşeron başlığını, firma/usta telefon rehberini ve fiyat teklif masasını görebilir.", "bi-cart-check-fill text-info"),
            (PermArchive, "📁 Evrak & Dosya Arşivi", "Sol menüdeki Evrak & Dosya Arşivi başlığını, kurumsal belgeleri ve proje klasörlerini görebilir.", "bi-folder-fill text-warning"),
            (PermCrm, "🏘️ Satış & Daire Portföyü", "Sol menüdeki Satış & Daire Portföyü başlığını, daire fiyatlarını ve müşteri CRM kayıtlarını yönetebilir.", "bi-building-check text-success"),
            (PermStaff, "👥 Personel & Ekipler Yönetimi", "Sol menüdeki Personel & Ekipler başlığını, özlük dosyalarını, maaş ve PDKS kayıtlarını görebilir.", "bi-people-fill text-secondary"),
            (PermInventory, "🛠️ Demirbaş & Merkez Depo", "Sol menüdeki Demirbaş & Depo başlığını, kalıp/iskele/makine stoklarını ve saha zimmetlerini yönetebilir.", "bi-tools text-secondary"),
            (PermWebsite, "🌐 Kurumsal Web & Domain Yönetimi", "Sol menüdeki Kurumsal Web & Domain başlığını görebilir; şirket web sitesi ve vitrin ayarlarını düzenleyebilir.", "bi-globe2 text-primary"),
            (PermStrategy, "🧠 Yönetim Strateji & AI Karar Merkezi", "Tüm şantiyelerin m² birim maliyet özetini, bölge rayiçlerini ve finansal projeksiyon raporlarını görebilir.", "bi-cpu-fill text-dark")
        };

        public static HashSet<string> GetDefaultPermissionsForRole(AgencyRole role)
        {
            switch (role)
            {
                case AgencyRole.Owner:
                case AgencyRole.Manager:
                    // Firma Sahibi / Genel Müdür -> Tüm Menü Linkleri Açık
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        PermCandidate, PermDna, PermSite, PermFinance, PermPurchasing, PermArchive, PermCrm, PermStaff, PermInventory, PermWebsite, PermStrategy
                    };

                case AgencyRole.Engineer:
                case AgencyRole.Architect:
                case AgencyRole.Purchasing:
                    // Teknik Ofis / Mühendis / Mimar -> 2. Evre, 3. Evre, Satın Alma, Evrak Arşivi ve Demirbaş Açık; Kasa/Finans ve Kurumsal Web Kapalı
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        PermDna, PermSite, PermPurchasing, PermArchive, PermInventory
                    };

                case AgencyRole.Accountant:
                    // Ön Muhasebe -> Finans, Satın Alma Cari, Evrak Arşivi ve Personel Bordro Açık; Kurumsal Web Kapalı
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        PermFinance, PermPurchasing, PermArchive, PermStaff
                    };

                case AgencyRole.Consultant:
                case AgencyRole.PublicRelations:
                    // Satış & Müşteri İlişkileri -> Aday Proje & Satış Portföyü Açık
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        PermCandidate, PermCrm
                    };

                case AgencyRole.SiteManager:
                case AgencyRole.Driver:
                case AgencyRole.Secretary:
                default:
                    // Şantiye Şefi / Saha Formeni -> Sadece 3. Evre Aktif Şantiye & Demirbaş (Finans ve Kurumsal Web Kapalı)
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        PermSite, PermInventory
                    };
            }
        }

        public static HashSet<string> GetEffectivePermissions(AgencyRole? dbRole, string? activeModules, string? previewModeCookie = null)
        {
            // 1. Eğer Yönetici üst bardan "Rol Ön İzleme" seçtiyse birebir o rolün şablonunu uygula
            if (!string.IsNullOrWhiteSpace(previewModeCookie))
            {
                var mode = previewModeCookie.Trim().ToLowerInvariant();
                if (mode == "owner")
                    return GetDefaultPermissionsForRole(AgencyRole.Owner);
                if (mode == "engineer")
                    return GetDefaultPermissionsForRole(AgencyRole.Engineer);
                if (mode == "formen")
                    return GetDefaultPermissionsForRole(AgencyRole.SiteManager);
            }

            // 2. Eğer kullanıcıya Personel Detayından özel menü link yetkileri (custom_perms) kaydedildiyse onları oku
            if (!string.IsNullOrWhiteSpace(activeModules))
            {
                var tokens = activeModules.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Any(t => t.Equals("custom_perms", StringComparison.OrdinalIgnoreCase)))
                {
                    var customSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var token in tokens)
                    {
                        if (token.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
                        {
                            customSet.Add(token.Substring(5));
                        }
                    }
                    return customSet;
                }
            }

            // 3. Varsayılan olarak kullanıcının şirket içi rol şablonunu döndür
            return GetDefaultPermissionsForRole(dbRole ?? AgencyRole.Owner);
        }

        public static string BuildUpdatedActiveModulesString(string? currentActiveModules, IEnumerable<string>? selectedPerms)
        {
            var baseTokens = new List<string>();
            if (!string.IsNullOrWhiteSpace(currentActiveModules))
            {
                foreach (var t in currentActiveModules.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!t.Equals("custom_perms", StringComparison.OrdinalIgnoreCase) &&
                        !t.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
                    {
                        baseTokens.Add(t);
                    }
                }
            }
            if (!baseTokens.Any()) baseTokens.Add("construction");

            baseTokens.Add("custom_perms");
            if (selectedPerms != null)
            {
                foreach (var p in selectedPerms.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    baseTokens.Add($"perm:{p.ToLowerInvariant()}");
                }
            }

            return string.Join(",", baseTokens);
        }

        public static string GetFriendlyRoleBadge(AgencyRole role)
        {
            return role switch
            {
                AgencyRole.Owner => "👑 Firma Sahibi / Yönetim Kurulu (Tam Yetki)",
                AgencyRole.Manager => "👔 Genel Müdür / Firma Yetkilisi (Tam Yetki)",
                AgencyRole.Engineer => "📐 Teknik Ofis & Saha Mühendisi",
                AgencyRole.Architect => "📐 Proje Mimarı & Çizim Sorumlusu",
                AgencyRole.Purchasing => "🛒 Satın Alma & İhale Sorumlusu",
                AgencyRole.Accountant => "💼 Ön Muhasebe & Finans Sorumlusu",
                AgencyRole.SiteManager => "👷 Şantiye Şefi / Saha Formeni",
                AgencyRole.Driver => "🚛 Araç & Saha Lojistik Sorumlusu",
                _ => "👷 Şirket Personeli"
            };
        }

        public static StaffHrDossier GetHrDossier(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.TrimStart().StartsWith("{"))
                return new StaffHrDossier();

            try
            {
                return JsonSerializer.Deserialize<StaffHrDossier>(rawJson) ?? new StaffHrDossier();
            }
            catch
            {
                return new StaffHrDossier();
            }
        }

        public static string SerializeHrDossier(StaffHrDossier dossier)
        {
            return JsonSerializer.Serialize(dossier ?? new StaffHrDossier());
        }

        public static async Task<bool> HasUserPermissionAsync(
            GMK360.Data.Contexts.ApplicationDbContext db,
            System.Security.Claims.ClaimsPrincipal user,
            Microsoft.AspNetCore.Http.HttpRequest? request,
            string requiredPerm)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            GMK360.Core.Entities.AgencyConsultant? consultant = null;
            string? activeModules = null;
            if (!string.IsNullOrEmpty(userId))
            {
                consultant = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(
                        System.Linq.Queryable.Where(
                            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(db.AgencyConsultants),
                            c => c.UserId == userId && c.IsActive));

                activeModules = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(
                        System.Linq.Queryable.Select(
                            System.Linq.Queryable.Where(
                                Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(db.Users),
                                u => u.Id == userId),
                            u => u.ActiveModules));
            }

            bool isAdmin = user.IsInRole("Admin") || user.IsInRole("SuperAdmin");
            bool isRealBoss = isAdmin ||
                              (consultant != null && (consultant.Role == AgencyRole.Owner || consultant.Role == AgencyRole.Manager));

            string? previewCookie = (isRealBoss && request != null)
                ? request.Cookies["gmk360_role_preview"]
                : null;

            if (consultant == null)
            {
                if (!isAdmin) return false;
                if (string.IsNullOrWhiteSpace(previewCookie) || previewCookie.Equals("owner", StringComparison.OrdinalIgnoreCase))
                    return true;
                return GetEffectivePermissions(AgencyRole.Owner, activeModules, previewCookie).Contains(requiredPerm);
            }

            var perms = GetEffectivePermissions(consultant.Role, activeModules, previewCookie);
            return perms.Contains(requiredPerm);
        }
    }
}
