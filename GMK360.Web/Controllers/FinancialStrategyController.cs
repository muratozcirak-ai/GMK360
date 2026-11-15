using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Controllers
{
    public class StrategyBaselineDto
    {
        public decimal GovUnitCostPerSqm { get; set; } = 18500m;
        public decimal FirmUnitCostPerSqm { get; set; } = 25000m;
        public decimal TcmbPolicyRate { get; set; } = 50.0m;
        public decimal ExpectedInflationRate { get; set; } = 41.5m;
        public decimal ExpectedLaborIncreaseRate { get; set; } = 35.0m;
        public decimal TargetMinProfitMarginPercent { get; set; } = 20.0m;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class ProjectStrategyRowDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public ProjectStatus Status { get; set; }
        public string StatusText { get; set; } = "";
        public string CityName { get; set; } = "İstanbul";
        public string DistrictName { get; set; } = "";
        public string NeighborhoodName { get; set; } = "";
        public string LocationFull { get; set; } = "";
        public string Ada { get; set; } = "-";
        public string Parsel { get; set; } = "-";
        public double LandAreaSqm { get; set; }
        public int TotalFloors { get; set; }
        public int BasementFloors { get; set; }
        public int TotalCostFloors { get; set; }
        public int TotalConstSqm { get; set; }
        public int UnitsPerFloor { get; set; }
        public int UnitAvgSqm { get; set; }
        public int TotalUnits { get; set; }
        public decimal OwnerSharePercent { get; set; }
        public int OwnerUnits { get; set; }
        public int ContractorUnits { get; set; }
        public int PlannedNewShops { get; set; }
        public int OwnerGivenShops { get; set; }
        public int ContractorShops { get; set; }
        public decimal FlatSalePrice { get; set; }
        public decimal PricePerSqmRayic { get; set; }
        public decimal ShopSalePrice { get; set; }
        public decimal GovUnitCostPerSqm { get; set; }
        public decimal FirmUnitCostPerSqm { get; set; }
        public decimal RentRelocationTotal { get; set; }
        public bool IncludeRentInFeasibility { get; set; }
        public decimal EstimatedRevenue { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal EstimatedNetProfit { get; set; }
        public decimal ProfitMarginPercent { get; set; }
        public bool HasAiReport { get; set; }
        public string? AiReport { get; set; }
        public DateTime? AiUpdatedAt { get; set; }
        public int SalesPoolCount { get; set; }
    }

    public class StrategySalesUnitOptionDto
    {
        public string Label { get; set; } = "";
        public decimal Price { get; set; }
        public decimal PricePerSqm { get; set; }
        public decimal FirmCostPerSqm { get; set; }
    }

    [Authorize]
    [Route("FinancialStrategy")]
    public class FinancialStrategyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FinancialStrategyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var agencyConsultant = await _context.AgencyConsultants
                .FirstOrDefaultAsync(a => a.UserId == user.Id);

            if (agencyConsultant != null && agencyConsultant.AgencyId > 0)
                return agencyConsultant.AgencyId;

            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            var fallbackProjAgency = await _context.ConstructionProjects
                .Where(p => !p.IsDeleted && p.AgencyId > 0)
                .Select(p => (int?)p.AgencyId)
                .FirstOrDefaultAsync();

            var firstAgency = await _context.Agencies.AsNoTracking().FirstOrDefaultAsync();
            return fallbackProjAgency ?? firstAgency?.Id ?? 1;
        }

        private string GetBaselineFilePath(int agencyId)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "strategy");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"strategy_baseline_{agencyId}.json");
        }

        private async Task<StrategyBaselineDto> LoadBaselineAsync(int agencyId)
        {
            var path = GetBaselineFilePath(agencyId);
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(path);
                    var dto = JsonSerializer.Deserialize<StrategyBaselineDto>(json);
                    if (dto != null) return dto;
                }
                catch { }
            }
            return new StrategyBaselineDto();
        }

        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var baseline = await LoadBaselineAsync(agencyId.Value);

            var projects = await _context.ConstructionProjects
                .Include(p => p.Blocks).ThenInclude(b => b.Units)
                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(cb => cb.Units)
                .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var cityIds = projects.Where(p => p.CityId.HasValue).Select(p => p.CityId!.Value).Distinct().ToList();
            var distIds = projects.Where(p => p.DistrictId.HasValue).Select(p => p.DistrictId!.Value).Distinct().ToList();
            var neighIds = projects.Where(p => p.NeighborhoodId.HasValue).Select(p => p.NeighborhoodId!.Value).Distinct().ToList();

            var cities = await _context.Cities.Where(c => cityIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
            var districts = await _context.Districts.Where(d => distIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);
            var neighborhoods = await _context.Neighborhoods.Where(n => neighIds.Contains(n.Id)).ToDictionaryAsync(n => n.Id, n => n);

            var rows = new List<ProjectStrategyRowDto>();
            var unitOptions = new List<StrategySalesUnitOptionDto>();

            foreach (var p in projects)
            {
                var policy = p.RentAndRontgenPolicy;
                string cityName = p.CityId.HasValue && cities.TryGetValue(p.CityId.Value, out var cn) ? cn : "İstanbul";
                string distName = p.DistrictId.HasValue && districts.TryGetValue(p.DistrictId.Value, out var dn) ? dn : "Kadıköy";
                string neighName = "";
                string? aiReport = null;
                DateTime? aiUpdated = null;

                if (p.NeighborhoodId.HasValue && neighborhoods.TryGetValue(p.NeighborhoodId.Value, out var nObj))
                {
                    neighName = nObj.Name ?? "";
                    aiReport = nObj.AiAnalysisReport;
                    aiUpdated = nObj.AiAnalysisUpdatedAt;
                }

                int floors = p.TotalFloors.HasValue && p.TotalFloors.Value > 0 ? p.TotalFloors.Value : 15;
                int basement = policy.BasementFloors >= 0 ? policy.BasementFloors : 2;
                int totalCostFloors = floors + basement + 2; // +1 Zemin +1 Çatı
                int totalConstSqm = totalCostFloors * 271;

                int unitsPerFloor = policy.UnitsPerFloor > 0 ? policy.UnitsPerFloor : 2;
                double coreLoss = policy.CoreLossSqm > 0 ? (double)policy.CoreLossSqm : 35.0;
                int unitAvgSqm = unitsPerFloor > 0 ? (int)Math.Round(Math.Max(0, 271.0 - coreLoss) / unitsPerFloor) : 118;

                int totalUnits = floors * unitsPerFloor;
                decimal ownerShare = policy.OwnerSharePercent > 0 ? policy.OwnerSharePercent : 50m;
                int ownerUnits = (int)Math.Round(totalUnits * (ownerShare / 100m));
                int contractorUnits = Math.Max(0, totalUnits - ownerUnits);

                int plannedShops = policy.PlannedNewShops > 0 ? policy.PlannedNewShops : (p.EskiDukkanSayisi ?? 0);
                int ownerShops = policy.OwnerGivenShops > 0 ? policy.OwnerGivenShops : (p.EskiDukkanSayisi ?? 0);
                int contractorShops = Math.Max(0, plannedShops - ownerShops);

                decimal flatSalePrice = p.AverageFlatSalePrice > 0 ? p.AverageFlatSalePrice : 10000000m;
                decimal pricePerSqmRayic = unitAvgSqm > 0 ? Math.Round(flatSalePrice / unitAvgSqm) : 0m;
                decimal shopSalePrice = policy.EstimatedShopSalePrice > 0 ? policy.EstimatedShopSalePrice : 15000000m;

                decimal govUnitCost = baseline.GovUnitCostPerSqm;
                decimal firmUnitCost = policy.FirmUnitCostPerSqm > 0 ? policy.FirmUnitCostPerSqm : baseline.FirmUnitCostPerSqm;

                int oldOwners = p.EskiDaireSayisi ?? 4;
                int oldShops = p.EskiDukkanSayisi ?? 0;
                int rentMonths = policy.RentDurationMonths > 0 ? policy.RentDurationMonths : 18;
                decimal rentBurden = (oldOwners * ((policy.FlatMonthlyRent * rentMonths) + policy.FlatRelocationFee))
                                   + (oldShops * ((policy.ShopMonthlyRent * rentMonths) + policy.ShopRelocationFee));

                decimal revenue = (contractorUnits * flatSalePrice) + (contractorShops * shopSalePrice);
                decimal cost = (totalConstSqm * firmUnitCost) + (policy.IncludeRentInFeasibility ? rentBurden : 0m);
                decimal netProfit = revenue - cost;
                decimal margin = cost > 0 ? Math.Round((netProfit / cost) * 100m, 1) : 0m;

                // Count contractor units in BuildingUnits (Sales Pool)
                var allUnits = p.Blocks.SelectMany(b => b.Units)
                    .Concat(p.Blocks.SelectMany(b => b.ChildBuildings).SelectMany(cb => cb.Units))
                    .Where(u => string.IsNullOrEmpty(u.OwnerName))
                    .ToList();

                string statusLabel = p.Status switch
                {
                    ProjectStatus.Projelendirme_Teklif => "Aday / Ön Etüd",
                    ProjectStatus.Aktif_Santiye => "Aktif Şantiye",
                    ProjectStatus.Tamamlandi_Teslim => "Tamamlandı / Teslim",
                    _ => "Aday / Ön Etüd"
                };

                var row = new ProjectStrategyRowDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    Status = p.Status,
                    StatusText = statusLabel,
                    CityName = cityName,
                    DistrictName = distName,
                    NeighborhoodName = neighName,
                    LocationFull = $"{cityName} / {distName}" + (!string.IsNullOrWhiteSpace(neighName) ? $" — {neighName} Mah." : ""),
                    Ada = string.IsNullOrWhiteSpace(p.Ada) ? "-" : p.Ada,
                    Parsel = string.IsNullOrWhiteSpace(p.Parsel) ? "-" : p.Parsel,
                    LandAreaSqm = p.TotalLandArea ?? 774.30,
                    TotalFloors = floors,
                    BasementFloors = basement,
                    TotalCostFloors = totalCostFloors,
                    TotalConstSqm = totalConstSqm,
                    UnitsPerFloor = unitsPerFloor,
                    UnitAvgSqm = unitAvgSqm,
                    TotalUnits = totalUnits,
                    OwnerSharePercent = ownerShare,
                    OwnerUnits = ownerUnits,
                    ContractorUnits = contractorUnits,
                    PlannedNewShops = plannedShops,
                    OwnerGivenShops = ownerShops,
                    ContractorShops = contractorShops,
                    FlatSalePrice = flatSalePrice,
                    PricePerSqmRayic = pricePerSqmRayic,
                    ShopSalePrice = shopSalePrice,
                    GovUnitCostPerSqm = govUnitCost,
                    FirmUnitCostPerSqm = firmUnitCost,
                    RentRelocationTotal = rentBurden,
                    IncludeRentInFeasibility = policy.IncludeRentInFeasibility,
                    EstimatedRevenue = revenue,
                    EstimatedCost = cost,
                    EstimatedNetProfit = netProfit,
                    ProfitMarginPercent = margin,
                    HasAiReport = !string.IsNullOrWhiteSpace(aiReport),
                    AiReport = aiReport,
                    AiUpdatedAt = aiUpdated,
                    SalesPoolCount = allUnits.Count
                };

                rows.Add(row);

                // Add Ön Etüd flat option for simulator
                unitOptions.Add(new StrategySalesUnitOptionDto
                {
                    Label = $"[Hızlı Ön Etüd] {p.Name} ({distName}/{neighName}) — ~{unitAvgSqm} m² Daire ({flatSalePrice:N0} ₺)",
                    Price = flatSalePrice,
                    PricePerSqm = pricePerSqmRayic,
                    FirmCostPerSqm = firmUnitCost
                });

                // Add actual SalesPool units if any exist
                foreach (var u in allUnits.Take(10))
                {
                    decimal uPrice = u.ListPrice.HasValue && u.ListPrice.Value > 0 ? u.ListPrice.Value : flatSalePrice;
                    decimal uSqm = u.GrossSquareMeters.HasValue && u.GrossSquareMeters.Value > 0 ? (decimal)u.GrossSquareMeters.Value : unitAvgSqm;
                    unitOptions.Add(new StrategySalesUnitOptionDto
                    {
                        Label = $"[Satış Havuzu] {p.Name} - Daire {u.DoorNumber} ({u.RoomLayout ?? "3+1"}, {uSqm:N0} m²) — {uPrice:N0} ₺",
                        Price = uPrice,
                        PricePerSqm = uSqm > 0 ? Math.Round(uPrice / uSqm) : pricePerSqmRayic,
                        FirmCostPerSqm = firmUnitCost
                    });
                }
            }

            ViewBag.Baseline = baseline;
            ViewBag.StrategyRows = rows;
            ViewBag.UnitOptions = unitOptions;

            return View(projects);
        }

        [HttpPost("SaveBaseline")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBaseline(
            decimal govUnitCostPerSqm,
            decimal firmUnitCostPerSqm,
            decimal tcmbPolicyRate,
            decimal expectedInflationRate,
            decimal expectedLaborIncreaseRate,
            decimal targetMinProfitMarginPercent,
            bool syncAllCandidateProjects = false)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var dto = new StrategyBaselineDto
            {
                GovUnitCostPerSqm = govUnitCostPerSqm > 0 ? govUnitCostPerSqm : 18500m,
                FirmUnitCostPerSqm = firmUnitCostPerSqm > 0 ? firmUnitCostPerSqm : 25000m,
                TcmbPolicyRate = tcmbPolicyRate >= 0 ? tcmbPolicyRate : 50.0m,
                ExpectedInflationRate = expectedInflationRate >= 0 ? expectedInflationRate : 41.5m,
                ExpectedLaborIncreaseRate = expectedLaborIncreaseRate >= 0 ? expectedLaborIncreaseRate : 35.0m,
                TargetMinProfitMarginPercent = targetMinProfitMarginPercent >= 0 ? targetMinProfitMarginPercent : 20.0m,
                UpdatedAt = DateTime.Now
            };

            var path = GetBaselineFilePath(agencyId.Value);
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(path, json);

            if (syncAllCandidateProjects)
            {
                var candidateProjects = await _context.ConstructionProjects
                    .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted && p.Status == ProjectStatus.Projelendirme_Teklif)
                    .ToListAsync();

                foreach (var cp in candidateProjects)
                {
                    var pol = cp.RentAndRontgenPolicy;
                    pol.FirmUnitCostPerSqm = dto.FirmUnitCostPerSqm;
                    cp.RentPolicyJson = JsonSerializer.Serialize(pol);
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Şirket baz kriterleri kaydedildi ve {candidateProjects.Count} aday projenin Hızlı Ön Etüd birim maliyeti ({dto.FirmUnitCostPerSqm:N0} ₺/m²) ile eşitlendi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Şirket strateji baz kriterleri, devlet/firma m² birim maliyetleri ve enflasyon beklentileri güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}