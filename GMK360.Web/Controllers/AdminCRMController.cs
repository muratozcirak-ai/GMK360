using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,HalklaIliskiler,Destek")]
    public class AdminCRMController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCRMController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        // --- Müşteri ve Kullanıcı Listesi (Kullanıcılar & CRM) ---
        public async Task<IActionResult> Index(string query)
        {
            // Mevcut kullanıcıları çek
            var users = await _context.Users
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Package)
                .ToListAsync();

            // Geriye dönük yama (Patch) - Daha önce EndDate atanmamış deneme paketlerine 30 gün ata
            bool hasChanges = false;
            foreach (var user in users)
            {
                // Mevcut deneme paketlerinin bitiş tarihlerini ayarla
                var activeSubs = user.Subscriptions.Where(s => s.IsActive && s.EndDate == null);
                foreach (var sub in activeSubs)
                {
                    if (sub.Package != null && sub.Package.Period == GMK360.Core.Entities.PackagePeriod.Free)
                    {
                        // Geçmişe dönük olarak Başlangıç tarihine 30 gün ekle
                        sub.EndDate = sub.StartDate.AddDays(30);
                        hasChanges = true;
                    }
                }

                // HİÇ paketi olmayan eski üyelere (Örn: MURAT DENEME) ücretsiz başlangıç paketi ata
                if (!user.Subscriptions.Any(s => s.IsActive) && (user.UserType == GMK360.Core.Entities.Identity.UserType.Individual || user.UserType == GMK360.Core.Entities.Identity.UserType.PropertyOwner))
                {
                    var freePackage = await _context.SubscriptionPackages.FirstOrDefaultAsync(p => p.Period == GMK360.Core.Entities.PackagePeriod.Free);
                    if (freePackage == null)
                    {
                        freePackage = new GMK360.Core.Entities.SubscriptionPackage
                        {
                            Name = "Dijital Evim - Ücretsiz Başlangıç",
                            Period = GMK360.Core.Entities.PackagePeriod.Free,
                            Price = 0,
                            TargetUserType = GMK360.Core.Entities.Identity.UserType.Individual,
                            MaxPropertiesCount = 1,
                            MaxRentTrackingCount = 3,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.SubscriptionPackages.Add(freePackage);
                        await _context.SaveChangesAsync();
                    }

                    var newSub = new GMK360.Core.Entities.UserSubscription
                    {
                        UserId = user.Id,
                        PackageId = freePackage.Id,
                        StartDate = DateTime.UtcNow.AddDays(-5), // Örnek olarak 5 gün önce başlamış gibi gösterelim
                        EndDate = DateTime.UtcNow.AddDays(25), // 30 Günlük denemenin 25 günü kaldı
                        IsActive = true,
                        PricePaid = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserSubscriptions.Add(newSub);
                    hasChanges = true;
                }
            }
            if(hasChanges)
            {
                await _context.SaveChangesAsync();
            }

            var usersQuery = _context.Users
                .Include(u => u.Subscriptions) // Paketleri çekmek için
                    .ThenInclude(s => s.Package)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                usersQuery = usersQuery.Where(u => 
                    (u.FirstName != null && u.FirstName.Contains(query)) || 
                    (u.LastName != null && u.LastName.Contains(query)) || 
                    (u.Email != null && u.Email.Contains(query)));
            }

            // Sadece kullanıcıları getir (Personelleri getirme)
            // Kaba bir filtre: Shadow hesaplar veya UserType != Corporate
            usersQuery = usersQuery.Where(u => u.IsShadowAccount || u.UserType != GMK360.Core.Entities.Identity.UserType.Corporate);

            var finalUsers = await usersQuery.OrderByDescending(u => u.Id).Take(100).ToListAsync();
            return View(finalUsers);
        }

        // --- Kullanıcı Detay (Abonelikler ve Mülkler) ---
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Kullanıcının sahip olduğu mülkleri getir
            var properties = await _context.Properties
                .Include(p => p.Building)
                .Where(p => p.OwnerUserId == id)
                .ToListAsync();

            ViewBag.Properties = properties;

            return View(user);
        }
    }
}
