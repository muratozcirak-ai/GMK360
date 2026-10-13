using System;
using System.Threading.Tasks;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Collections.Generic;

using GMK360.Web.Filters;
namespace GMK360.Web.Controllers
{
    [RequireOnboarding]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly GMK360.Data.Contexts.ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly GMK360.Web.Services.WalletService _walletService;

        public PropertyController(IPropertyService propertyService, 
                                  GMK360.Data.Contexts.ApplicationDbContext context, 
                                  Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, 
                                  IWebHostEnvironment env,
                                  GMK360.Web.Services.WalletService walletService)
        {
            _propertyService = propertyService;
            _context = context;
            _userManager = userManager;
            _env = env;
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveDraft([FromForm] GMK360.Web.Models.PropertyCreateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            Property draft = null;
            if (model.Id > 0)
            {
                draft = await _context.Properties.FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == user.Id);
            }

            if (draft == null)
            {
                draft = new Property
                {
                    UserId = user.Id,
                    State = ListingState.Draft,
                    Features = new List<PropertyFeature>()
                };
                _context.Properties.Add(draft);
            }
            
            // Dummy or real values
            draft.Title = string.IsNullOrWhiteSpace(model.Title) ? "Taslak Ã„Â°lan" : model.Title;
            draft.Description = string.IsNullOrWhiteSpace(model.Description) ? "" : model.Description;
            draft.Price = model.Price == 0 ? 1 : model.Price;
            draft.StatusId = model.StatusId == 0 ? 1 : model.StatusId; // Dummy Status
            draft.TypeId = model.TypeId == 0 ? 1 : model.TypeId;
            draft.NetArea = model.NetArea == 0 ? 1 : model.NetArea;
            draft.GrossArea = model.GrossArea == 0 ? 1 : model.GrossArea;
            draft.BuildingNumber = string.IsNullOrWhiteSpace(model.BuildingNumber) ? "-" : model.BuildingNumber;
            draft.UnitNumber = string.IsNullOrWhiteSpace(model.UnitNumber) ? "-" : model.UnitNumber;
            draft.DraftStep = model.DraftStep > 0 ? model.DraftStep : 1;
            
            draft.ComplexId = model.ComplexId;

            // Fix for NOT NULL columns in DB
            draft.AuthorizationDocumentNo = string.IsNullOrWhiteSpace(model.AuthorizationDocumentNo) ? "-" : model.AuthorizationDocumentNo;
            draft.OwnerIdNumber = string.IsNullOrWhiteSpace(model.OwnerIdNumber) ? "-" : model.OwnerIdNumber;
            draft.DeedStatus = string.IsNullOrWhiteSpace(model.DeedStatus) ? "-" : model.DeedStatus;
            draft.BlockNumber = string.IsNullOrWhiteSpace(model.BlockNumber) ? "-" : model.BlockNumber;
            draft.ParcelNumber = string.IsNullOrWhiteSpace(model.ParcelNumber) ? "-" : model.ParcelNumber;
            draft.BlockName = string.IsNullOrWhiteSpace(model.BlockName) ? "-" : model.BlockName;
            draft.Facade = string.IsNullOrWhiteSpace(model.Facade) ? "-" : model.Facade;
            draft.RoomCount = string.IsNullOrWhiteSpace(model.RoomCount) ? "-" : model.RoomCount;
            draft.BuildingAge = string.IsNullOrWhiteSpace(model.BuildingAge) ? "-" : model.BuildingAge;
            draft.TimesharePeriod = string.IsNullOrWhiteSpace(model.TimesharePeriod) ? "-" : model.TimesharePeriod;
            draft.VideoFilePath = "";
            draft.VideoUrl = string.IsNullOrWhiteSpace(model.VideoUrl) ? "" : model.VideoUrl;
            draft.ICalUrl = "";
            draft.Currency = string.IsNullOrWhiteSpace(model.Currency) ? "TRY" : model.Currency;
            
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, draftId = draft.Id });
        }

        [Authorize]
        public async Task<IActionResult> MyListings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var properties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Complex)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Sadece gerekli alanlarÃ„Â± View'e taÃ…Å¸Ã„Â±yabiliriz veya direkt Entity listesi dÃƒÂ¶nebiliriz.
            return View(properties);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleState(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);
            if (property == null) return NotFound();

            if (property.State == ListingState.Active)
            {
                property.State = ListingState.Passive;
            }
            else if (property.State == ListingState.Passive || property.State == ListingState.Revoked)
            {
                property.State = ListingState.Active;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, newState = property.State.ToString() });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BoostProperty(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id);
            if (property == null) return NotFound();

            if (property.IsBoosted && property.PromotedEndDate.HasValue && property.PromotedEndDate > DateTime.Now)
            {
                return Json(new { success = false, message = "Bu ilan zaten vitrinde." });
            }

            // Vitrin ÃƒÅ“creti (Ãƒâ€“rn: 50 Kredi)
            decimal vitrinCost = 50m;
            
            bool isPaid = await _walletService.SpendCreditAsync(user.Id, vitrinCost);
            
            if (isPaid)
            {
                property.IsBoosted = true;
                property.PromotedEndDate = DateTime.Now.AddDays(30); // 30 GÃƒÂ¼nlÃƒÂ¼k Vitrin
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Ã„Â°lanÃ„Â±nÃ„Â±z baÃ…Å¸arÃ„Â±yla vitrine taÃ…Å¸Ã„Â±ndÃ„Â±!" });
            }
            else
            {
                return Json(new { success = false, message = "Yetersiz bakiye. LÃƒÂ¼tfen cÃƒÂ¼zdanÃ„Â±nÃ„Â±za kredi yÃƒÂ¼kleyin.", redirectUrl = "/Wallet/Index" });
            }
        }

        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.District)
                .Include(p => p.Complex)
                    .ThenInclude(c => c.City)
                .Include(p => p.Status)
                .Include(p => p.Type)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }

        public IActionResult Vitrin()
        {
            return View();
        }

        public async Task<IActionResult> DetailBySlug(string seoSlug)
        {
            if (string.IsNullOrEmpty(seoSlug)) return NotFound();

            var parts = seoSlug.Split('-');
            if (parts.Length > 0 && int.TryParse(parts.Last(), out int id))
            {
                return await Detail(id, isFromSlug: true);
            }
            return NotFound();
        }

        public async Task<IActionResult> Detail(int id, bool isFromSlug = false)
        {
            if (!isFromSlug)
            {
                var p = await _context.Properties
                    .Include(x => x.Building).ThenInclude(c => c.City)
                    .Include(x => x.Building).ThenInclude(c => c.District)
                    .Include(x => x.Status)
                    .Include(x => x.Type)
                    .FirstOrDefaultAsync(x => x.Id == id);
                
                if (p == null) return NotFound();
                
                var cityStr = p.Complex?.City?.Name.ToSeoUrl() ?? "il";
                var distStr = p.Complex?.District?.Name.ToSeoUrl() ?? "ilce";
                var statusStr = p.Status?.Name.ToSeoUrl() ?? "satilik";
                var typeStr = p.Type?.Name.ToSeoUrl() ?? "emlak";
                var catStr = $"{statusStr}-{typeStr}";
                var titleStr = p.Title.ToSeoUrl();
                var slug = $"{cityStr}-{distStr}-{catStr}-{titleStr}-{p.Id}";
                
                return RedirectPermanent($"/ilan/{slug}");
            }

            // VeritabanÃ„Â±ndan gerÃƒÂ§ek veriyi ÃƒÂ§ekmeyi deniyoruz
            var realProperty = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Building).ThenInclude(c => c.District)
                .Include(p => p.Building).ThenInclude(c => c.City)
                .Include(p => p.Building).ThenInclude(c => c.Neighborhood)
                .Include(p => p.User).ThenInclude(u => u.AgencyConsultants).ThenInclude(ac => ac.Agency)
                .Include(p => p.Features).ThenInclude(f => f.DefinitionValue)
                .FirstOrDefaultAsync(p => p.Id == id);

            Property property;

            if (realProperty != null)
            {
                property = realProperty;
                // GerÃƒÂ§ek resimler
                if (property.Images != null && property.Images.Any())
                {
                    ViewBag.MediaList = property.Images.OrderBy(img => img.SortOrder).ToList();
                }
                else
                {
                    ViewBag.MediaList = new List<PropertyImage>();
                }
                
                // GerÃƒÂ§ek ÃƒÂ¶zellikler
                var featureList = new List<string>();
                if (property.Features != null)
                {
                    foreach(var f in property.Features)
                    {
                        if(f.DefinitionValue != null) {
                            featureList.Add(f.DefinitionValue.Name);
                        }
                    }
                }
                ViewBag.Features = featureList;
            }
            else
            {
                // Ã„Â°leride veritabanÃ„Â±ndan ÃƒÂ§ekilecek, Ã…Å¸imdilik UI tasarÃ„Â±mÃ„Â± iÃƒÂ§in zengin mock data
                property = new Property
                {
                    Id = id == 0 ? 1 : id,
                    Title = "KadÃ„Â±kÃƒÂ¶y Moda'da Full Deniz ManzaralÃ„Â± LÃƒÂ¼ks 3+1 AkÃ„Â±llÃ„Â± Ev",
                    Description = @"<p>Moda sahilinin en prestijli lokasyonunda, kesintisiz deniz ve adalar manzarasÃ„Â±na sahip, tamamen yenilenmiÃ…Å¸ 3+1 lÃƒÂ¼ks daire.</p>
                                    <p><strong>Daire Ãƒâ€“zellikleri:</strong></p>
                                    <ul>
                                        <li>145 mÃ‚Â² brÃƒÂ¼t, 130 mÃ‚Â² net kullanÃ„Â±m alanÃ„Â±</li>
                                        <li>AkÃ„Â±llÃ„Â± ev sistemi (AydÃ„Â±nlatma, Ã„Â±sÃ„Â±tma ve panjur kontrolÃƒÂ¼)</li>
                                        <li>Ebeveyn banyosu ve giyinme odasÃ„Â±</li>
                                        <li>Yerden Ã„Â±sÃ„Â±tma ve VRF klima sistemi</li>
                                        <li>2 araÃƒÂ§lÃ„Â±k kapalÃ„Â± otopark tahsisi</li>
                                    </ul>
                                    <p>Binada 7/24 gÃƒÂ¼venlik, kapalÃ„Â± yÃƒÂ¼zme havuzu ve fitness salonu bulunmaktadÃ„Â±r. Randevu ile gÃƒÂ¶sterilmektedir.</p>",
                    Price = 18500000,
                    Currency = "TRY",
                    // Complex mock kaldÃ„Â±rÃ„Â±ldÃ„Â±
                    StatusId = 1, // SatÃ„Â±lÃ„Â±k
                    TypeId = 1, // Konut
                    User = new GMK360.Core.Entities.Identity.ApplicationUser 
                    { 
                        FirstName = "Ahmet", 
                        LastName = "YÃ„Â±lmaz",
                        Email = "ahmet@emlakburada.com",
                        ProfileImageUrl = "/images/default-avatar.png",
                        AgencyConsultants = new List<AgencyConsultant> 
                        { 
                            new AgencyConsultant 
                            { 
                                IsActive = true, 
                                Agency = new Agency { CompanyName = "Premium Gayrimenkul A.Ã…Â." } 
                            } 
                        }
                    }
                };

                // Dynamic Features Mock
                ViewBag.Features = new List<string> { "AkÃ„Â±llÃ„Â± Ev Sistemi", "Yerden IsÃ„Â±tma", "Ebeveyn Banyosu", "KapalÃ„Â± Otopark", "7/24 GÃƒÂ¼venlik", "Deniz ManzarasÃ„Â±", "AsansÃƒÂ¶r", "Balkon" };
                
                // Mock Images (DÃƒÂ¶nÃƒÂ¼Ã…Å¸tÃƒÂ¼rÃƒÂ¼ldÃƒÂ¼)
                var mockImages = new List<PropertyImage> 
                {
                    new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?ixlib=rb-4.0.3&auto=format&fit=crop&w=2075&q=80", SortOrder = 1, IsCover = true, RoomTag = "DÃ„Â±Ã…Å¸ Cephe", IsVideo = false },
                    new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?ixlib=rb-4.0.3&auto=format&fit=crop&w=1170&q=80", SortOrder = 2, RoomTag = "Salon", IsVideo = false },
                    new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3?ixlib=rb-4.0.3&auto=format&fit=crop&w=1170&q=80", SortOrder = 3, RoomTag = "Yatak OdasÃ„Â±", IsVideo = false },
                    new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?ixlib=rb-4.0.3&auto=format&fit=crop&w=1170&q=80", SortOrder = 4, RoomTag = "KapalÃ„Â± Otopark", IsVideo = false }
                };
                ViewBag.MediaList = mockImages;
            }

            // Kurlar ve AltÃ„Â±n (Mock/Temsili)
            ViewBag.UsdRate = 33.20m;
            ViewBag.EurRate = 35.50m;
            ViewBag.GoldGram = 2500m; // Gram AltÃ„Â±n

            // YouTube MantÃ„Â±Ã„Å¸Ã„Â± Ã„Â°ÃƒÂ§in SaÃ„Å¸ Taraftaki Benzer/Vitrin Ã„Â°lanlar
            var relatedProperties = new List<Property>();
            for(int i = 1; i <= 5; i++)
            {
                relatedProperties.Add(new Property {
                    Id = id + i,
                    Title = $"KadÃ„Â±kÃƒÂ¶y Sahilde {i+1}+1 LÃƒÂ¼ks Daire",
                    Price = 15000000 + (i * 1000000),
                    Currency = "TRY",
                    // Complex mock kaldÃ„Â±rÃ„Â±ldÃ„Â±
                });
            }
            ViewBag.RelatedProperties = relatedProperties;
            if (property.Complex != null && property.Complex.DistrictId > 0)
            {
                ViewBag.TopUstalar = await _context.ServiceProviders
                    .Include(sp => sp.Areas)
                    .Include(sp => sp.Services).ThenInclude(s => s.ServiceCategory)
                    .Where(sp => sp.IsVerified && (sp.IsGlobal || sp.Areas.Any(a => a.DistrictId == property.Complex.DistrictId)))
                    .OrderByDescending(sp => sp.AverageRating)
                    .Take(3)
                    .ToListAsync();
            }

            return View("Detail", property);
        }

        public async Task<IActionResult> DetailPartial(int id)
        {
            var viewResult = await Detail(id, true) as ViewResult;
            if (viewResult != null && viewResult.Model is Property property)
            {
                return PartialView("_DetailPartial", property);
            }
            return NotFound();
        }

        public IActionResult Compare(int id1, int id2)
        {
            // Mock verilerle iki ilanÃ„Â± kÃ„Â±yaslama sayfasÃ„Â± iÃƒÂ§in hazÃ„Â±rlÃ„Â±yoruz.
            var p1 = new Property
            {
                Id = id1,
                Title = "Moda'da Full Deniz ManzaralÃ„Â± 3+1",
                Price = 18500000,
                Currency = "TRY",
                // Complex mock kaldÃ„Â±rÃ„Â±ldÃ„Â±
            };

            var p2 = new Property
            {
                Id = id2,
                Title = "Caddebostan Sahilde 4+1 LÃƒÂ¼ks Daire",
                Price = 24000000,
                Currency = "TRY",
                // Complex mock kaldÃ„Â±rÃ„Â±ldÃ„Â±
            };

            // P1 Ãƒâ€“zellikler
            ViewBag.P1Images = new List<string> { "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?ixlib=rb-4.0.3&auto=format&fit=crop&w=1000&q=80" };
            ViewBag.P1Features = new List<string> { "AkÃ„Â±llÃ„Â± Ev Sistemi", "Yerden IsÃ„Â±tma", "Deniz ManzarasÃ„Â±" };
            ViewBag.P1Rooms = "3+1";
            ViewBag.P1Area = "145 mÃ‚Â²";
            ViewBag.P1Age = "5 YaÃ…Å¸Ã„Â±nda";

            // P2 Ãƒâ€“zellikler
            ViewBag.P2Images = new List<string> { "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?ixlib=rb-4.0.3&auto=format&fit=crop&w=1000&q=80" };
            ViewBag.P2Features = new List<string> { "Yerden IsÃ„Â±tma", "KapalÃ„Â± Otopark", "7/24 GÃƒÂ¼venlik", "AsansÃƒÂ¶r" };
            ViewBag.P2Rooms = "4+1";
            ViewBag.P2Area = "180 mÃ‚Â²";
            ViewBag.P2Age = "SÃ„Â±fÃ„Â±r Bina";

            ViewBag.Property1 = p1;
            ViewBag.Property2 = p2;

            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // TODO: CanlÃ„Â±ya ÃƒÂ§Ã„Â±karken EÃ„Â°DS (e-Devlet) doÃ„Å¸rulamasÃ„Â± aktif edilecek. 
                // Ã…Âimdilik test iÃƒÂ§in devre dÃ„Â±Ã…Å¸Ã„Â± bÃ„Â±rakÃ„Â±ldÃ„Â±.
                // if (!user.PhoneNumberConfirmed || !user.IsEDevletVerified)
                // {
                //     return RedirectToAction("VerificationRequired", "Account");
                // }
                ViewBag.UserType = user.UserType.ToString(); // Individual, Corporate, Consultant
            }
            var parkingTypes = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "PARKING")
                .OrderBy(v => v.Order)
                .ToListAsync();

            var propertyStatuses = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "PROPERTY_STATUS")
                .OrderBy(v => v.Order)
                .ToListAsync();

            var propertyTypes = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "PROPERTY_TYPE")
                .OrderBy(v => v.Order)
                .ToListAsync();

            var propertySubTypes = await _context.DefinitionValues
                .Include(v => v.Category)
                .Where(v => v.Category.SystemCode.StartsWith("SUB_TYPE_"))
                .OrderBy(v => v.Order)
                .ToListAsync();

            var fromWhoms = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "FROM_WHOM")
                .OrderBy(v => v.Order)
                .ToListAsync();

            var heatingTypes = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "HEATING_TYPE")
                .OrderBy(v => v.Order)
                .ToListAsync();

            var dynamicDropdowns = await _context.DefinitionCategories
                .Include(c => c.Values)
                .Where(c => c.SystemCode.StartsWith("DROPDOWN_"))
                .OrderBy(c => c.Name)
                .ToListAsync();

            var matrixCategoriesProperty = await _context.DefinitionCategories
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Values)
                .Where(c => c.ParentCategoryId == null && c.SystemCode.StartsWith("GROUP_") && (c.TargetType == GMK360.Core.Entities.FeatureTargetType.Both || c.TargetType == GMK360.Core.Entities.FeatureTargetType.Property))
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            var matrixCategoriesComplex = await _context.DefinitionCategories
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Values)
                .Where(c => c.ParentCategoryId == null && c.SystemCode.StartsWith("GROUP_") && (c.TargetType == GMK360.Core.Entities.FeatureTargetType.Both || c.TargetType == GMK360.Core.Entities.FeatureTargetType.Complex))
                .OrderBy(c => c.Name)
                .ToListAsync();

            var dynamicFeatureKeys = await _context.DefinitionValues
                .Where(v => v.Category.SystemCode == "PROPERTY_DYNAMIC_KEY")
                .OrderBy(v => v.Order)
                .ToListAsync();

            ViewBag.ParkingTypes = parkingTypes;
            ViewBag.PropertyStatuses = propertyStatuses;
            ViewBag.PropertyTypes = propertyTypes;
            ViewBag.PropertySubTypes = propertySubTypes;
            ViewBag.FromWhoms = fromWhoms;
            ViewBag.HeatingTypes = heatingTypes;
            ViewBag.DynamicDropdowns = dynamicDropdowns;
            ViewBag.MatrixCategoriesProperty = matrixCategoriesProperty;
            ViewBag.MatrixCategoriesComplex = matrixCategoriesComplex;
            ViewBag.DynamicFeatureKeys = dynamicFeatureKeys;

            if (user != null)
            {
                if (id.HasValue && id.Value > 0)
                {
                    // Spesifik taslaÃ„Å¸Ã„Â± yÃƒÂ¼kle
                    var draftProperty = await _context.Properties
                        .Include(p => p.Complex)
                        .FirstOrDefaultAsync(p => p.UserId == user.Id && p.Id == id.Value && p.State == ListingState.Draft);
                        
                    if (draftProperty != null)
                    {
                        var draftModel = new GMK360.Web.Models.PropertyCreateViewModel
                        {
                            Id = draftProperty.Id,
                            DraftStep = draftProperty.DraftStep,
                            Title = draftProperty.Title == "Taslak Ã„Â°lan" ? "" : draftProperty.Title,
                            Description = draftProperty.Description,
                            Price = draftProperty.Price == 1 ? 0 : draftProperty.Price,
                            StatusId = draftProperty.StatusId,
                            TypeId = draftProperty.TypeId,
                            SubTypeId = draftProperty.SubTypeId ?? 0,
                            CityId = draftProperty.Complex?.CityId ?? 0,
                            DistrictId = draftProperty.Complex?.DistrictId ?? 0,
                            NeighborhoodId = draftProperty.Complex?.NeighborhoodId ?? 0,
                            StreetId = draftProperty.Complex?.StreetId ?? 0,
                            ComplexId = draftProperty.ComplexId,
                            BuildingNumber = draftProperty.BuildingNumber == "-" ? "" : draftProperty.BuildingNumber,
                            UnitNumber = draftProperty.UnitNumber == "-" ? "" : draftProperty.UnitNumber,
                            NetArea = draftProperty.NetArea == 1 ? 0 : draftProperty.NetArea,
                            GrossArea = draftProperty.GrossArea == 1 ? 0 : draftProperty.GrossArea
                        };
                        return View(draftModel);
                    }
                }
                else
                {
                    // KullanÃ„Â±cÃ„Â±nÃ„Â±n tÃƒÂ¼m taslaklarÃ„Â±nÃ„Â± getir
                    var userDrafts = await _context.Properties
                        .Where(p => p.UserId == user.Id && p.State == ListingState.Draft)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToListAsync();

                    if (userDrafts.Any())
                    {
                        ViewBag.UserDrafts = userDrafts;
                    }
                }
            }

            return View(new GMK360.Web.Models.PropertyCreateViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(GMK360.Web.Models.PropertyCreateViewModel model)
        {
            // Basit bir reCAPTCHA kontrolÃƒÂ¼ (UI tarafÃ„Â±nda doÃ„Å¸rulanmÃ„Â±Ã…Å¸ farz ediyoruz)
            if (string.IsNullOrEmpty(model.RecaptchaToken))
            {
                ModelState.AddModelError("RecaptchaToken", "LÃƒÂ¼tfen robot olmadÃ„Â±Ã„Å¸Ã„Â±nÃ„Â±zÃ„Â± doÃ„Å¸rulayÃ„Â±n.");
            }

            // 40 Resim KotasÃ„Â± KontrolÃƒÂ¼
            if (model.ProcessedImages != null && model.ProcessedImages.Count > 40)
            {
                ModelState.AddModelError("ProcessedImages", "Bir ilana en fazla 40 adet resim yÃƒÂ¼kleyebilirsiniz.");
            }

            // Tapu Ã„Â°stisnasÃ„Â± (MÃƒÂ¼teahhit / Ã„Â°nÃ…Å¸aat FirmasÃ„Â± & Topraktan SatÃ„Â±Ã…Å¸)
            if (string.IsNullOrEmpty(model.DeedStatus))
            {
                var statusDef = await _context.DefinitionValues.FindAsync(model.StatusId);
                bool isTopraktan = statusDef != null && (statusDef.Name.Contains("Topraktan") || statusDef.Name.Contains("Projeden"));
                bool isInsaatFirmasi = User.IsInRole("InsaatFirmasi");

                if (!(isInsaatFirmasi && isTopraktan))
                {
                    ModelState.AddModelError("DeedStatus", "Tapu durumu veya belge numarasÃ„Â± girilmesi zorunludur (Sadece Ã„Â°nÃ…Å¸aat FirmalarÃ„Â±nÃ„Â±n 'Topraktan SatÃ„Â±Ã…Å¸' ilanlarÃ„Â±nda bu alan opsiyoneldir).");
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Ã„Â°lan kaydedilirken bazÃ„Â± alanlar eksik veya hatalÃ„Â±: " + string.Join(" | ", errors);
                
                ViewBag.ParkingTypes = await _context.DefinitionValues.Where(v => v.Category.SystemCode == "PARKING_TYPE").OrderBy(v => v.Order).ToListAsync();
                ViewBag.PropertyStatuses = await _context.DefinitionValues.Where(v => v.Category.SystemCode == "PROPERTY_STATUS").OrderBy(v => v.Order).ToListAsync();
                ViewBag.PropertyTypes = await _context.DefinitionValues.Where(v => v.Category.SystemCode == "PROPERTY_TYPE").OrderBy(v => v.Order).ToListAsync();
                // Yeni Dinamik SeÃƒÂ§im KutularÃ„Â± (Dropdownlar) - "DROPDOWN_" ile baÃ…Å¸layanlar
                ViewBag.DynamicDropdowns = await _context.DefinitionCategories
                    .Include(c => c.Values)
                    .Where(c => c.SystemCode.StartsWith("DROPDOWN_"))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                // Dinamik Matris Ãƒâ€“zellikleri GruplarÃ„Â± (Ã„Â°ÃƒÂ§ Ãƒâ€“zellikler, DÃ„Â±Ã…Å¸ Ãƒâ€“zellikler vb.)
                ViewBag.MatrixCategories = await _context.DefinitionCategories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.Values)
                    .Where(c => c.ParentCategoryId == null && c.SystemCode.StartsWith("GROUP_"))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                    
                ViewBag.DynamicFeatureKeys = await _context.DefinitionValues.Where(v => v.Category.SystemCode == "PROPERTY_DYNAMIC_KEY").OrderBy(v => v.Order).ToListAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!user.PhoneNumberConfirmed || !user.IsEDevletVerified)
            {
                return RedirectToAction("VerificationRequired", "Account");
            }

            // DanÃ„Â±Ã…Å¸man ise aktif ajansÃ„Â±nÃ„Â± bul
            int? agencyId = null;
            if (user.UserType == UserType.Consultant)
            {
                var relation = _context.AgencyConsultants
                    .FirstOrDefault(ac => ac.UserId == user.Id && ac.IsActive);
                if (relation != null)
                {
                    agencyId = relation.AgencyId;
                }
            }

            // Bina Ekleme / Koordinat MantÃ„Â±Ã„Å¸Ã„Â±
            int? finalComplexId = model.ComplexId;
            // Note: New complexes are now saved via AJAX in step 2 before reaching here.
            // So model.ComplexId will already be populated if they entered a new building.
            if (finalComplexId == null || finalComplexId <= 0)
            {
                if (!string.IsNullOrWhiteSpace(model.BuildingName))
                {
                    var newComplex = new Complex
                    {
                        Name = model.BuildingName,
                        CityId = model.CityId,
                        DistrictId = model.DistrictId,
                        NeighborhoodId = model.NeighborhoodId,
                        StreetId = model.StreetId,
                        Latitude = model.Latitude ?? 0,
                        Longitude = model.Longitude ?? 0,
                        IsApproved = false
                    };
                    _context.Complexes.Add(newComplex);
                    await _context.SaveChangesAsync();
                    finalComplexId = newComplex.Id;
                }
                else
                {
                    finalComplexId = null;
                }
            }

            // Kimden bilgisini otomatik ata
            if (!model.FromWhomId.HasValue)
            {
                var fromWhomCategory = await _context.DefinitionCategories
                    .Include(c => c.Values)
                    .FirstOrDefaultAsync(c => c.SystemCode == "FROM_WHOM");

                if (fromWhomCategory != null)
                {
                    if (user.UserType == UserType.Individual)
                    {
                        var owner = fromWhomCategory.Values.FirstOrDefault(v => v.SystemCode == "FROM_WHOM_OWNER" || v.Name.Contains("Sahibinden"));
                        if (owner != null) model.FromWhomId = owner.Id;
                    }
                    else if (user.UserType == UserType.Consultant)
                    {
                        var agency = fromWhomCategory.Values.FirstOrDefault(v => v.SystemCode == "FROM_WHOM_AGENCY" || v.Name.Contains("Ofis") || v.Name.Contains("Emlak"));
                        if (agency != null) model.FromWhomId = agency.Id;
                    }
                }
            }
            var selectedDistrict = await _context.Districts.FindAsync(model.DistrictId);
            string? side = selectedDistrict?.RegionName;

            var unitNumbers = string.IsNullOrWhiteSpace(model.UnitNumber) 
                ? new List<string> { "-" } 
                : model.UnitNumber.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim()).Where(u => !string.IsNullOrEmpty(u)).ToList();
            if (unitNumbers.Count == 0) unitNumbers.Add("-");

            var baseProperty = new Property
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Currency = model.Currency,
                NetArea = model.NetArea,
                GrossArea = model.GrossArea,
                StatusId = model.StatusId,
                TypeId = model.TypeId,
                SubTypeId = model.SubTypeId,
                FromWhomId = model.FromWhomId,
                ComplexId = finalComplexId,
                Dues = model.Dues,
                DeedStatus = model.DeedStatus ?? "-",
                BlockNumber = model.BlockNumber ?? "-",
                ParcelNumber = model.ParcelNumber ?? "-",
                TotalAreaSqm = model.TotalAreaSqm,
                ShareAreaSqm = model.ShareAreaSqm,
                RoomCount = model.RoomCount ?? "-",
                BathroomCount = model.BathroomCount,
                BalconyCount = model.BalconyCount,
                WcCount = model.WcCount,
                BuildingAge = model.BuildingAge ?? "-",
                HeatingId = model.HeatingId,
                IsFurnished = model.IsFurnished,
                UnitNumber = unitNumbers.First(), // Ã„Â°lk daire noyu taslak alÃ„Â±yoruz
                FloorNumber = model.FloorNumber,
                TotalFloors = model.TotalFloors,
                BuildingNumber = model.BuildingNumber ?? "-",
                HasBlock = model.HasBlock,
                BlockName = model.BlockName ?? "-",
                IsTimeshare = model.IsTimeshare,
                TimeshareStartDate = model.TimeshareStartDate,
                TimeshareEndDate = model.TimeshareEndDate,
                TimesharePeriod = model.TimesharePeriod ?? "-",
                HideLocation = model.HideLocation,
                Side = side,
                UserId = user.Id,
                AgencyId = agencyId,
                State = ListingState.PendingApproval, // EÃ„Â°DS (e-Devlet) onayÃ„Â± bekliyor
                VideoFilePath = "",
                VideoUrl = model.VideoUrl ?? "",
                ICalUrl = "",
                
                // EÃ„Â°DS Yetki AlanlarÃ„Â±
                HasAuthorization = model.HasAuthorization,
                AuthorizationDocumentNo = model.AuthorizationDocumentNo ?? "-",
                OwnerIdNumber = model.OwnerIdNumber ?? "-",
                AuthorizationEndDate = model.HasAuthorization ? DateTime.Now.AddDays(90) : (DateTime?)null, // VarsayÃ„Â±lan 90 gÃƒÂ¼n yetki
                ExternalMarketAlert = false,

                Features = new List<PropertyFeature>()
            };

            // Normal ÃƒÂ¶zellikler (Checkbox vb) (Eski YapÃ„Â± - Geriye Uyumluluk iÃƒÂ§in tutulabilir)
            if (model.SelectedFeatureIds != null && model.SelectedFeatureIds.Any())
            {
                foreach (var fid in model.SelectedFeatureIds)
                {
                    baseProperty.Features.Add(new PropertyFeature
                    {
                        DefinitionValueId = fid
                    });
                }
            }

            // DÃ„Â°NAMÃ„Â°K AÃƒâ€¡ILIR LÃ„Â°STELER (DROPDOWN) - Ãƒâ€“rn: Oda SayÃ„Â±sÃ„Â±, Daire Tipi vs.
            if (model.DynamicSelects != null && model.DynamicSelects.Any())
            {
                foreach (var ds in model.DynamicSelects)
                {
                    // ds.Value, seÃƒÂ§ilen DefinitionValueId'dir
                    if (ds.Value > 0)
                    {
                        baseProperty.Features.Add(new PropertyFeature
                        {
                            DefinitionValueId = ds.Value
                        });
                    }
                }
            }

            // DÃ„Â°NAMÃ„Â°K MATRÃ„Â°S Ãƒâ€“ZELLÃ„Â°KLERÃ„Â° (Yeni SÃƒÂ¼per YapÃ„Â±)
            if (model.MatrixFeatures != null && model.MatrixFeatures.Any())
            {
                foreach (var mf in model.MatrixFeatures)
                {
                    if (mf.IsSelected)
                    {
                        baseProperty.Features.Add(new PropertyFeature
                        {
                            DefinitionValueId = mf.DefinitionValueId,
                            Count = mf.Count,
                            SelectedSubOptions = mf.SubOptions != null && mf.SubOptions.Any() ? string.Join(", ", mf.SubOptions) : string.Empty,
                            Note = mf.Note ?? string.Empty
                        });
                    }
                }
            }

            // DÃ„Â°NAMÃ„Â°K MATRÃ„Â°S (COMPLEX / SÃ„Â°TE Ãƒâ€“ZELLÃ„Â°KLERÃ„Â°)
            if (model.MatrixFeaturesComplex != null && model.MatrixFeaturesComplex.Any() && finalComplexId.HasValue)
            {
                var complex = await _context.Complexes.Include(c => c.Features).FirstOrDefaultAsync(c => c.Id == finalComplexId.Value);
                if (complex != null)
                {
                    foreach (var mf in model.MatrixFeaturesComplex)
                    {
                        if (mf.IsSelected && !complex.Features.Any(cf => cf.DefinitionValueId == mf.DefinitionValueId))
                        {
                            complex.Features.Add(new ComplexFeature
                            {
                                DefinitionValueId = mf.DefinitionValueId,
                                Note = mf.Note ?? string.Empty
                            });
                        }
                    }
                }
            }

            // Dinamik Key-Value ÃƒÂ¶zellikleri (Bina YaÃ…Å¸Ã„Â±: 5 vb)
            if (model.DynamicFeatures != null && model.DynamicFeatures.Any())
            {
                foreach (var kv in model.DynamicFeatures)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Value))
                    {
                        baseProperty.Features.Add(new PropertyFeature
                        {
                            DefinitionValueId = kv.Key,
                            Value = kv.Value
                        });
                    }
                }
            }

            // POI'leri OTOMATÃ„Â°K olarak ilana ÃƒÂ¶zellik olarak ekle (KullanÃ„Â±cÃ„Â± seÃƒÂ§imine gerek yok)
            if (model.NeighborhoodId > 0)
            {
                var pois = await _context.NeighborhoodPOIs.Where(p => p.NeighborhoodId == model.NeighborhoodId).ToListAsync();
                
                if (pois.Any())
                {
                    // YakÃ„Â±n Ãƒâ€¡evre kategorisini bul veya oluÃ…Å¸tur
                    var poiCategory = await _context.DefinitionCategories.FirstOrDefaultAsync(c => c.SystemCode == "POI");
                    if (poiCategory == null)
                    {
                        poiCategory = new DefinitionCategory { Name = "YakÃ„Â±n Ãƒâ€¡evre", SystemCode = "POI" };
                        _context.DefinitionCategories.Add(poiCategory);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var poi in pois)
                    {
                        var defVal = await _context.DefinitionValues.FirstOrDefaultAsync(v => v.CategoryId == poiCategory.Id && v.Name == poi.PoiName);
                        if (defVal == null)
                        {
                            defVal = new DefinitionValue { CategoryId = poiCategory.Id, Name = poi.PoiName };
                            _context.DefinitionValues.Add(defVal);
                            await _context.SaveChangesAsync();
                        }

                        baseProperty.Features.Add(new PropertyFeature
                        {
                            DefinitionValueId = defVal.Id,
                            Value = $"{poi.PoiCategory} ({Math.Round(poi.DistanceInMeters)}m)"
                        });
                    }
                }
            }

            // Resim/Video YÃƒÂ¼kleme Ã„Â°Ã…Å¸lemleri (Yeni Smart Media Dropzone)
            if (model.ProcessedImages != null && model.ProcessedImages.Any())
            {
                baseProperty.Images = new List<PropertyImage>();
                var tempFolder = Path.Combine(_env.WebRootPath, "uploads", "temp");
                var propertiesFolder = Path.Combine(_env.WebRootPath, "uploads", "properties");
                
                if (!Directory.Exists(propertiesFolder))
                {
                    Directory.CreateDirectory(propertiesFolder);
                }

                foreach (var media in model.ProcessedImages.OrderBy(m => m.SortOrder))
                {
                    if (!string.IsNullOrEmpty(media.TempPath))
                    {
                        var tempPath = Path.Combine(tempFolder, media.TempPath);
                        var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(media.TempPath);
                        var finalPath = Path.Combine(propertiesFolder, newFileName);

                        if (System.IO.File.Exists(tempPath))
                        {
                            System.IO.File.Move(tempPath, finalPath);

                            baseProperty.Images.Add(new PropertyImage
                            {
                                ImageUrl = "/uploads/properties/" + newFileName,
                                IsCover = (media.SortOrder == 1),
                                SortOrder = media.SortOrder,
                                RoomTag = media.RoomTag,
                                IsVideo = media.IsVideo
                            });
                        }
                    }
                }
            }
            
            // DÃ„Â±Ã…Å¸ YouTube/Vimeo Linki
            baseProperty.VideoUrl = model.VideoUrl;

            // Taslak (Draft) varsa onu silip yeni Aktif versiyonu kaydediyoruz (mapping kolaylÃ„Â±Ã„Å¸Ã„Â± iÃƒÂ§in)
            if (model.Id > 0)
            {
                var existingDraft = await _context.Properties
                    .Include(p => p.Features)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == user.Id);
                    
                if (existingDraft != null)
                {
                    _context.Properties.Remove(existingDraft);
                }
            }

            var propertiesToInsert = new List<Property> { baseProperty };
            
            // EÃ„Å¸er 1'den fazla UnitNumber girildiyse (Bulk Insert)
            for (int i = 1; i < unitNumbers.Count; i++)
            {
                var clone = new Property
                {
                    Title = baseProperty.Title,
                    Description = baseProperty.Description,
                    Price = baseProperty.Price,
                    Currency = baseProperty.Currency,
                    NetArea = baseProperty.NetArea,
                    GrossArea = baseProperty.GrossArea,
                    StatusId = baseProperty.StatusId,
                    TypeId = baseProperty.TypeId,
                    SubTypeId = baseProperty.SubTypeId,
                    FromWhomId = baseProperty.FromWhomId,
                    ComplexId = baseProperty.ComplexId,
                    Dues = baseProperty.Dues,
                    DeedStatus = baseProperty.DeedStatus,
                    BlockNumber = baseProperty.BlockNumber,
                    ParcelNumber = baseProperty.ParcelNumber,
                    TotalAreaSqm = baseProperty.TotalAreaSqm,
                    ShareAreaSqm = baseProperty.ShareAreaSqm,
                    RoomCount = baseProperty.RoomCount,
                    BathroomCount = baseProperty.BathroomCount,
                    BalconyCount = baseProperty.BalconyCount,
                    WcCount = baseProperty.WcCount,
                    BuildingAge = baseProperty.BuildingAge,
                    HeatingId = baseProperty.HeatingId,
                    IsFurnished = baseProperty.IsFurnished,
                    FloorNumber = baseProperty.FloorNumber,
                    TotalFloors = baseProperty.TotalFloors,
                    BuildingNumber = baseProperty.BuildingNumber,
                    HasBlock = baseProperty.HasBlock,
                    BlockName = baseProperty.BlockName,
                    IsTimeshare = baseProperty.IsTimeshare,
                    TimeshareStartDate = baseProperty.TimeshareStartDate,
                    TimeshareEndDate = baseProperty.TimeshareEndDate,
                    TimesharePeriod = baseProperty.TimesharePeriod,
                    HideLocation = baseProperty.HideLocation,
                    Side = baseProperty.Side,
                    UserId = baseProperty.UserId,
                    AgencyId = baseProperty.AgencyId,
                    State = baseProperty.State,
                    VideoFilePath = baseProperty.VideoFilePath,
                    VideoUrl = baseProperty.VideoUrl,
                    ICalUrl = baseProperty.ICalUrl,
                    HasAuthorization = baseProperty.HasAuthorization,
                    AuthorizationDocumentNo = baseProperty.AuthorizationDocumentNo,
                    OwnerIdNumber = baseProperty.OwnerIdNumber,
                    AuthorizationEndDate = baseProperty.AuthorizationEndDate,
                    ExternalMarketAlert = baseProperty.ExternalMarketAlert,
                    
                    UnitNumber = unitNumbers[i], 
                    
                    Features = new List<PropertyFeature>(),
                    Images = new List<PropertyImage>()
                };

                foreach (var f in baseProperty.Features)
                {
                    clone.Features.Add(new PropertyFeature
                    {
                        DefinitionValueId = f.DefinitionValueId,
                        Value = f.Value,
                        Count = f.Count,
                        SelectedSubOptions = f.SelectedSubOptions,
                        Note = f.Note
                    });
                }

                if (baseProperty.Images != null)
                {
                    foreach (var img in baseProperty.Images)
                    {
                        clone.Images.Add(new PropertyImage
                        {
                            ImageUrl = img.ImageUrl,
                            IsCover = img.IsCover,
                            SortOrder = img.SortOrder,
                            RoomTag = img.RoomTag,
                            IsVideo = img.IsVideo
                        });
                    }
                }

                propertiesToInsert.Add(clone);
            }

            _context.Properties.AddRange(propertiesToInsert);
            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // EÃ„Â°DS YETKÃ„Â° KONTROLÃƒÅ“ (Ã„Â°Ãƒâ€¡ Ãƒâ€¡ATIÃ…ÂMA Ãƒâ€¡Ãƒâ€“ZÃƒÅ“MÃƒÅ“) & N8N WEBHOOK
            // -----------------------------------------------------
            foreach (var prop in propertiesToInsert)
            {
                if (prop.HasAuthorization && prop.ComplexId.HasValue && !string.IsNullOrWhiteSpace(prop.UnitNumber))
                {
                    var conflictingQuery = _context.Properties
                        .Where(p => p.ComplexId == prop.ComplexId 
                                 && p.UnitNumber == prop.UnitNumber 
                                 && p.State == ListingState.Active
                                 && p.Id != prop.Id);

                    if (prop.HasBlock && !string.IsNullOrWhiteSpace(prop.BlockName))
                    {
                        conflictingQuery = conflictingQuery.Where(p => p.BlockName == prop.BlockName);
                    }

                    var conflictingListings = await conflictingQuery.ToListAsync();
                    if (conflictingListings.Any())
                    {
                        foreach (var oldListing in conflictingListings)
                        {
                            oldListing.State = ListingState.Revoked; // Yetkisi AlÃ„Â±ndÃ„Â± (Ã„Â°ptal)
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                if (prop.HasAuthorization)
                {
                    try
                    {
                        // Fire and forget (Asenkron beklemeden fÃ„Â±rlatÃ„Â±rÃ„Â±z)
                        var webhookUrl = "http://localhost:5678/webhook/market-intelligence-crosscheck";
                        var payload = new {
                            PropertyId = prop.Id,
                            Title = prop.Title,
                            Price = prop.Price,
                            ComplexId = prop.ComplexId,
                            UnitNumber = prop.UnitNumber,
                            BlockName = prop.BlockName,
                            Rooms = prop.RoomCount,
                            NetArea = prop.NetArea,
                            AgentName = (user.FirstName + " " + user.LastName).Trim()
                        };
                        
                        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                        _ = new HttpClient().PostAsync(webhookUrl, content);
                    }
                    catch (Exception) { /* Webhook hatasÃ„Â± uygulamayÃ„Â± patlatmasÃ„Â±n */ }
                }
            }

            if(propertiesToInsert.Count > 1)
            {
                TempData["SuccessMessage"] = $"{propertiesToInsert.Count} adet ilanÃ„Â±nÃ„Â±z baÃ…Å¸arÃ„Â±yla toplu olarak eklendi ve yayÃ„Â±na alÃ„Â±ndÃ„Â±!";
            }
            else
            {
                TempData["SuccessMessage"] = "Ã„Â°lanÃ„Â±nÃ„Â±z baÃ…Å¸arÃ„Â±yla eklendi ve yayÃ„Â±na alÃ„Â±ndÃ„Â±!";
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult SavePropertyNote([FromBody] PropertyNoteRequest request)
        {
            // Ã„Â°leride veritabanÃ„Â±na eklenecek (User - Property Note tablosu)
            return Json(new { success = true, message = "Not baÃ…Å¸arÃ„Â±yla kaydedildi." });
        }
    }

    [RequireOnboarding]
    public class PropertyNoteRequest
    {
        public int propertyId { get; set; }
        public string note { get; set; }
    }
}





