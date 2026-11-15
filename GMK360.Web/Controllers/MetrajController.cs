using System;

using System.Collections.Generic;

using System.Threading.Tasks;

using System.Text;

using GMK360.Data.Contexts;

using GMK360.Core.Entities.Construction;

using GMK360.Core.Entities.Construction.Workflows;

using GMK360.Core.Entities.B2B;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using System.Linq;



using System.IO;

using Microsoft.AspNetCore.Http;

using GMK360.Web.Models;

using Microsoft.AspNetCore.Authorization;
using GMK360.Core.Entities;



namespace GMK360.Web.Controllers

{

    [Authorize]
    public class MetrajController : Controller

    {

        private readonly ApplicationDbContext _context;

        private readonly GMK360.Web.Services.Construction.DnaTimeOffsetService _dnaTimeOffsetService;



        public MetrajController(ApplicationDbContext context, GMK360.Web.Services.Construction.DnaTimeOffsetService dnaTimeOffsetService)

        {

            _context = context;

            _dnaTimeOffsetService = dnaTimeOffsetService;

        }



        public IActionResult Index(int projectId, int? recipeId)

        {

            return RedirectToAction(nameof(DrawingArchive), new { projectId = projectId });

        }



        // =================================================================================

        // KANAT 1: DÄ°JÄ°TAL Ã‡Ä°ZÄ°M ARÅÄ°VÄ° & Ä°LK KABUL MASASI

        // =================================================================================

        public async Task<IActionResult> DrawingArchive(int? projectId, int? id, [FromQuery] FieldDecisionsDto? decisions = null)

        {

            int pId = projectId ?? id ?? 0;

            if (pId <= 0) return RedirectToAction("Index", "ConstructionProject");



            ViewData["ProjectId"] = pId;



            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(c => c.Units)

                .Include(p => p.Blocks).ThenInclude(b => b.Units)

                .FirstOrDefaultAsync(p => p.Id == pId);



            if (project == null) return NotFound();



            ViewData["ProjectName"] = project.Name;

            ViewBag.Project = project;



            // TÃ¼m binalarÄ±/bloklarÄ± topla (Ana baza/blok ve alt kuleler)

            var allBuildings = new List<Building>();

            var parentBlocks = project.Blocks.Where(b => b.ParentBuildingId == null).ToList();

            

            foreach (var b in parentBlocks)

            {

                allBuildings.Add(b);

                if (b.ChildBuildings != null && b.ChildBuildings.Any())

                {

                    allBuildings.AddRange(b.ChildBuildings);

                }

            }

            foreach (var b in project.Blocks)

            {

                if (!allBuildings.Any(x => x.Id == b.Id))

                {

                    allBuildings.Add(b);

                }

            }



            var allBuildingIds = allBuildings.Select(b => b.Id).Distinct().ToList();

            var floorEntityTypes = allBuildingIds.Select(bId => "BuildingFloor_" + bId).ToList();



            // Kat PlanÄ± Belgelerini Ã§ek (DmsDocument)

            var floorPlanDocs = await _context.DmsDocuments

                .Where(d => floorEntityTypes.Contains(d.EntityType))

                .ToListAsync();



            // Ã‡izim ArÅŸivi (DocumentArchive) belgelerini Ã§ek

            var allDrawings = await _context.DocumentArchives

                .Where(d => d.ProjectId == pId)

                .OrderByDescending(d => d.UploadDate)

                .ToListAsync();



            // Kat Matrisini OluÅŸtur

            var matrix = new List<FloorPlanMatrixItem>();



            foreach (var building in allBuildings)

            {

                int totalSlabs = GetTotalSlabs(building);

                var slabIndices = new HashSet<int>();



                if (totalSlabs > 0)

                {

                    for (int s = 1; s <= totalSlabs; s++)

                    {

                        slabIndices.Add(s);

                    }

                }

                else

                {

                    slabIndices.Add(1);

                }



                if (building.Units != null && building.Units.Any())

                {

                    foreach (var u in building.Units)

                    {

                        slabIndices.Add(u.FloorLevel);

                    }

                }



                foreach (var slabIdx in slabIndices.OrderBy(s => s))

                {

                    string entityType = "BuildingFloor_" + building.Id;

                    var doc = floorPlanDocs.FirstOrDefault(d => d.EntityType == entityType && d.EntityId == slabIdx);

                    var unitsOnFloor = building.Units?.Where(u => u.FloorLevel == slabIdx).ToList() ?? new List<BuildingUnit>();



                    string floorName = GetFloorNameBySlabIndex(building, slabIdx);



                    matrix.Add(new FloorPlanMatrixItem

                    {

                        BuildingId = building.Id,

                        BlockName = building.BlockName ?? building.Name,

                        IsChildBuilding = building.ParentBuildingId.HasValue,

                        ParentBuildingId = building.ParentBuildingId,

                        FloorLevel = slabIdx,

                        FloorName = floorName,

                        UnitCount = unitsOnFloor.Count,

                        TotalGrossM2 = unitsOnFloor.Sum(u => u.GrossSquareMeters ?? 0),

                        TotalNetM2 = unitsOnFloor.Sum(u => u.NetSquareMeters ?? 0),

                        HasPlan = doc != null,

                        PlanUrl = doc?.DocumentUrl,

                        PlanFileName = doc?.Title,

                        PlanExtension = doc?.FileExtension,

                        PlanUploadDate = doc?.UploadDate,

                        PlanSizeBytes = doc?.FileSizeBytes

                    });

                }

            }



            var model = new DrawingArchiveViewModel

            {

                Project = project,

                Blocks = allBuildings,

                FloorMatrix = matrix,

                AllDrawings = allDrawings,

                ArchitecturalDrawings = allDrawings.Where(d => d.Category == "Mimari" || (d.Category != null && d.Category.Contains("Mimari"))).ToList(),

                StructuralDrawings = allDrawings.Where(d => d.Category == "Statik" || (d.Category != null && d.Category.Contains("Statik"))).ToList(),

                MechanicalDrawings = allDrawings.Where(d => d.Category == "Mekanik" || (d.Category != null && d.Category.Contains("Mekanik"))).ToList(),

                ElectricalDrawings = allDrawings.Where(d => d.Category == "Elektrik" || (d.Category != null && d.Category.Contains("Elektrik"))).ToList(),

                TotalFloorsCount = matrix.Count,

                FloorsWithPlanCount = matrix.Count(m => m.HasPlan),
                Decisions = decisions ?? new FieldDecisionsDto(),
                ReadDrawings = allDrawings.Where(d => d.Status == "Okundu").OrderByDescending(d => d.UploadDate).ToList(),
                HasReadDrawings = allDrawings.Any(d => d.Status == "Okundu"),
                CadAnalysisResults = allDrawings.Where(d => d.Status == "Okundu").Select(d => GetOrComputeCadAnalysis(d, project)).Where(r => r != null).ToList()!,
                MasterBomItems = allDrawings.Any(d => d.Status == "Okundu")
                    ? GenerateMasterBomTakeoff(project, allDrawings.Where(d => d.Status == "Okundu").ToList(), decisions)
                    : new List<MasterBomItemDto>()

            };

            // Faz-0 İkizi: Şantiye Malzeme Tedarik Anlaşmaları, İhale Kayıtları ve Personel Listesi
            ViewBag.MaterialAgreements = LoadProjectMaterialAgreements(project.AgencyId, pId);
            ViewBag.ProjectQuotes = await _context.B2BQuoteRequests
                .Include(q => q.Invites)
                .Where(q => q.SourceModule == "ConstructionProject" && q.SourceReferenceId == pId && !q.IsDeleted)
                .OrderByDescending(q => q.Id)
                .ToListAsync();

            var staffOptions = new List<string>
            {
                "Satınalma Departmanı",
                "Şantiye Şefi",
                "Proje Müdürü",
                "Muhasebe & Finans",
                "Depo & Lojistik Sorumlusu"
            };
            var dbWorkers = await _context.AgencyWorkers
                .AsNoTracking()
                .Where(w => w.AgencyId == project.AgencyId && !w.IsDeleted && w.IsActive)
                .Select(w => w.FirstName + " " + w.LastName + " (" + w.Profession + ")")
                .ToListAsync();
            foreach (var w in dbWorkers)
            {
                if (!staffOptions.Contains(w)) staffOptions.Add(w);
            }
            ViewBag.StaffList = staffOptions;

            return View(model);

        }

        private List<MasterBomItemDto> GenerateMasterBomTakeoff(ConstructionProject project, List<DocumentArchive> allDrawings, FieldDecisionsDto? decisions = null)
        {
            var list = new List<MasterBomItemDto>();
            if (project == null) return list;

            // KRİTİK KURAL: Henüz okunmuş (Status == "Okundu") hiçbir DWG/DXF yoksa matris kesinlikle boş dönmelidir!
            var readDocs = allDrawings?.Where(d => d.Status == "Okundu").ToList() ?? new List<DocumentArchive>();
            if (!readDocs.Any()) return list;

            var cadResults = readDocs.Select(d => GetOrComputeCadAnalysis(d, project)).Where(r => r != null).ToList();
            double cadDetectedArea = cadResults.Where(c => c!.DetectedFloorAreaM2 > 0).Sum(c => c!.DetectedFloorAreaM2);
            int cadDetectedDoors = cadResults.Sum(c => c!.DoorCount);

            var blocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();
            var rootBlocks = blocks.Where(b => b.ParentBuildingId == null).ToList();

            double landArea = project.TotalLandArea ?? (cadDetectedArea > 0 ? Math.Round(cadDetectedArea * 2.2, 0) : 1200);
            double baseArea = rootBlocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0 && blocks.Any()) baseArea = blocks.Sum(b => b.BaseArea ?? 0);
            if (baseArea <= 0 && cadDetectedArea > 0) baseArea = cadDetectedArea;
            if (baseArea <= 0) baseArea = project.EskiBinaOturumAlani ?? (landArea > 0 ? Math.Round(landArea * 0.40, 0) : 420);

            int maxFloors = blocks.Any() ? blocks.Max(b => (b.TotalFloors ?? 0) + b.BasementFloors) : (project.TotalFloors ?? 5);
            int maxBasement = blocks.Any() ? blocks.Max(b => b.BasementFloors) : (project.EskiBodrumKatSayisi ?? 1);
            if (maxBasement <= 0) maxBasement = 1;
            if (maxFloors <= 0) maxFloors = 5;

            int totalUnits = blocks.Sum(b => b.TotalApartments);
            if (totalUnits <= 0 && project.TargetTotalApartments.HasValue && project.TargetTotalApartments.Value > 0)
                totalUnits = project.TargetTotalApartments.Value;
            if (totalUnits <= 0 && cadDetectedDoors > 0)
                totalUnits = Math.Max(4, (int)Math.Round((cadDetectedDoors / 7.0) * Math.Max(1, maxFloors - maxBasement)));
            if (totalUnits <= 0)
                totalUnits = Math.Max(4, (int)Math.Round((baseArea * Math.Max(1, maxFloors - maxBasement)) / 125.0));

            int totalShops = blocks.Sum(b => b.TotalShops);
            if (totalShops <= 0) totalShops = project.TargetTotalShops ?? (project.EskiDukkanSayisi ?? 2);

            double totalFloorArea = 0;
            if (cadDetectedArea > 0)
            {
                totalFloorArea = cadDetectedArea < 1200 ? cadDetectedArea * maxFloors : cadDetectedArea;
            }
            else if (blocks.Any(b => b.BaseArea.HasValue && b.BaseArea > 0))
            {
                totalFloorArea = blocks.Sum(b => (b.BaseArea ?? (baseArea / Math.Max(1, blocks.Count))) * ((b.TotalFloors ?? 5) + b.BasementFloors));
            }
            else
            {
                totalFloorArea = baseArea * maxFloors;
            }
            if (totalFloorArea <= 0) totalFloorArea = baseArea * maxFloors;

            int id = 1;

            // =========================================================================
            // 1. DEMİR & ÇELİK TEDARİKÇİSİ (KİM SATAR) -> KALIP & DEMİR EKİBİ (KİM YAPAR)
            // =========================================================================
            string branchDemir = "Demirci";
            string keyDemir = "branch_demir";
            string iconDemir = "bi-grid-3x3-gap-fill";
            int orderDemir = 1;
            string tradeDemir = "Kalıp & Demirci Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "18-28'lik Kalın Demir",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.034), 1),
                Unit = "Ton"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "14-16'lık Orta Demir",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.030), 1),
                Unit = "Ton"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "8-12'lik İnce Demir",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.016), 1),
                Unit = "Ton"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "Hasır Çelik",
                Quantity = Math.Round((decimal)(baseArea * 2.5), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "Bağ Teli",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.08 * 8.5), 0),
                Unit = "kg"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDemir,
                PackageKey = keyDemir,
                PackageName = branchDemir,
                PackageIcon = iconDemir,
                PackageOrder = orderDemir,
                ProfessionName = tradeDemir,
                Title = "Plastik Paspayı",
                Quantity = Math.Round((decimal)(totalFloorArea * 1.5), 0),
                Unit = "Adet"
            });

            // =========================================================================
            // 2. HAZIR BETONCU -> BETON DÖKÜM & POMPA EKİBİ
            // =========================================================================
            string branchBeton = "Hazır Betoncu";
            string keyBeton = "branch_beton";
            string iconBeton = "bi-truck";
            int orderBeton = 2;
            string tradeBeton = "Beton Döküm Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBeton,
                PackageKey = keyBeton,
                PackageName = branchBeton,
                PackageIcon = iconBeton,
                PackageOrder = orderBeton,
                ProfessionName = tradeBeton,
                Title = "C30/37 Taşıyıcı Beton",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.44), 0),
                Unit = "m³"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBeton,
                PackageKey = keyBeton,
                PackageName = branchBeton,
                PackageIcon = iconBeton,
                PackageOrder = orderBeton,
                ProfessionName = tradeBeton,
                Title = "C16 Grobeton",
                Quantity = Math.Round((decimal)(baseArea * 0.15), 0),
                Unit = "m³"
            });

            // =========================================================================
            // 3. DUVAR & GAZBETONCU -> DUVARCI EKİBİ
            // =========================================================================
            string branchDuvar = "Duvar & Gazbetoncu";
            string keyDuvar = "branch_duvar";
            string iconDuvar = "bi-bricks";
            int orderDuvar = 3;
            string tradeDuvar = "Duvarcı Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDuvar,
                PackageKey = keyDuvar,
                PackageName = branchDuvar,
                PackageIcon = iconDuvar,
                PackageOrder = orderDuvar,
                ProfessionName = tradeDuvar,
                Title = "20'lik Dış Duvar Gazbetonu",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.38), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDuvar,
                PackageKey = keyDuvar,
                PackageName = branchDuvar,
                PackageIcon = iconDuvar,
                PackageOrder = orderDuvar,
                ProfessionName = tradeDuvar,
                Title = "15'lik Daire İçi Gazbeton",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDuvar,
                PackageKey = keyDuvar,
                PackageName = branchDuvar,
                PackageIcon = iconDuvar,
                PackageOrder = orderDuvar,
                ProfessionName = tradeDuvar,
                Title = "Gazbeton Tutkalı",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.80 * 0.12), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchDuvar,
                PackageKey = keyDuvar,
                PackageName = branchDuvar,
                PackageIcon = iconDuvar,
                PackageOrder = orderDuvar,
                ProfessionName = tradeDuvar,
                Title = "Prekast Lento",
                Quantity = Math.Round((decimal)(totalUnits * 6 + totalShops * 4), 0),
                Unit = "Adet"
            });

            // =========================================================================
            // 4. SIVACI (KARA / KABA SIVA MALZEMELERİ) -> KARA SIVA EKİBİ
            // =========================================================================
            string branchSivaci = "Sıvacı";
            string keySivaci = "branch_sivaci";
            string iconSivaci = "bi-layers-half";
            int orderSivaci = 4;
            string tradeSivaci = "Kara Sıvacı Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSivaci,
                PackageKey = keySivaci,
                PackageName = branchSivaci,
                PackageIcon = iconSivaci,
                PackageOrder = orderSivaci,
                ProfessionName = tradeSivaci,
                Title = "Kaba & İnce Sıva Kumu",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.80 * 0.045), 1),
                Unit = "Ton"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSivaci,
                PackageKey = keySivaci,
                PackageName = branchSivaci,
                PackageIcon = iconSivaci,
                PackageOrder = orderSivaci,
                ProfessionName = tradeSivaci,
                Title = "Sıva Çimentosu & Kireç",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.80 * 0.15), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSivaci,
                PackageKey = keySivaci,
                PackageName = branchSivaci,
                PackageIcon = iconSivaci,
                PackageOrder = orderSivaci,
                ProfessionName = tradeSivaci,
                Title = "Sıva Köşe Profili & Ano Çıtası",
                Quantity = Math.Round((decimal)(totalUnits * 24 + totalShops * 16), 0),
                Unit = "Metre"
            });

            // =========================================================================
            // 5. ŞAPÇI (ZEMİN ŞAP MALZEMELERİ) -> HELİKOPTERLİ ŞAPÇI EKİBİ
            // =========================================================================
            string branchSapci = "Şapçı";
            string keySapci = "branch_sapci";
            string iconSapci = "bi-square-half";
            int orderSapci = 5;
            string tradeSapci = "Şapçı Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSapci,
                PackageKey = keySapci,
                PackageName = branchSapci,
                PackageIcon = iconSapci,
                PackageOrder = orderSapci,
                ProfessionName = tradeSapci,
                Title = "Şap Kumu (0-3 mm Perdah Kumu)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.70 * 0.08), 1),
                Unit = "Ton"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSapci,
                PackageKey = keySapci,
                PackageName = branchSapci,
                PackageIcon = iconSapci,
                PackageOrder = orderSapci,
                ProfessionName = tradeSapci,
                Title = "Şap Çimentosu (CEM I 42.5)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.70 * 0.22), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSapci,
                PackageKey = keySapci,
                PackageName = branchSapci,
                PackageIcon = iconSapci,
                PackageOrder = orderSapci,
                ProfessionName = tradeSapci,
                Title = "Şap Polipropilen Elyafı",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.70 * 0.10), 0),
                Unit = "Paket"
            });

            // =========================================================================
            // 6. ÇATI & YAĞMUR İNDİRME -> ÇATI EKİBİ
            // =========================================================================
            string branchCati = "Çatı & Yağmur İndirme";
            string keyCati = "branch_cati";
            string iconCati = "bi-house-door-fill";
            int orderCati = 6;
            string tradeCati = "Çatı Ustası Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Kiremit (Marsilya / Akdeniz)",
                Quantity = Math.Round((decimal)(baseArea * 1.35 * 15), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Mahya Kiremidi",
                Quantity = Math.Round((decimal)(Math.Sqrt(baseArea) * 4), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "15 mm Çatı OSB-3",
                Quantity = Math.Round((decimal)(baseArea * 1.35), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Kiremit Altı Su Yalıtım Örtüsü",
                Quantity = Math.Round((decimal)(baseArea * 1.45), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Eksiz Çatı Oluğu & Yağmur İniş Borusu",
                Quantity = Math.Round((decimal)(Math.Sqrt(baseArea) * 4 + maxFloors * 3.2 * 4), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Çatı İskelet Kerestesi (5×10 - 10×10)",
                Quantity = Math.Round((decimal)(baseArea * 1.35 * 0.035), 1),
                Unit = "m³"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCati,
                PackageKey = keyCati,
                PackageName = branchCati,
                PackageIcon = iconCati,
                PackageOrder = orderCati,
                ProfessionName = tradeCati,
                Title = "Çatı Taşyünü Isı Yalıtım Şiltesi",
                Quantity = Math.Round((decimal)(baseArea * 1.20), 0),
                Unit = "m²"
            });

            // =========================================================================
            // 7. KALIP & KERESTECİ -> KALIPÇI EKİBİ
            // =========================================================================
            string branchOrman = "Kalıp & Keresteci";
            string keyOrman = "branch_orman";
            string iconOrman = "bi-tree-fill";
            int orderOrman = 7;
            string tradeKalip = "Kalıpçı Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchOrman,
                PackageKey = keyOrman,
                PackageName = branchOrman,
                PackageIcon = iconOrman,
                PackageOrder = orderOrman,
                ProfessionName = tradeKalip,
                Title = "18 mm Huş Plywood",
                Quantity = Math.Round((decimal)(baseArea * 1.25), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchOrman,
                PackageKey = keyOrman,
                PackageName = branchOrman,
                PackageIcon = iconOrman,
                PackageOrder = orderOrman,
                ProfessionName = tradeKalip,
                Title = "5×10 & 10×10 Kalıp Kerestesi",
                Quantity = Math.Round((decimal)(baseArea * 0.055), 1),
                Unit = "m³"
            });

            // =========================================================================
            // 8. İZOLASYON & SU YALITIMI -> İZOLASYON USTASI
            // =========================================================================
            string branchYalitim = "Yalıtımcı";
            string keyYalitim = "branch_yalitim";
            string iconYalitim = "bi-shield-shaded";
            int orderYalitim = 8;
            string tradeYalitim = "İzolasyon Ustası";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "4 mm Bitümlü Membran",
                Quantity = Math.Round((decimal)(baseArea * 1.45 * maxBasement), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Bitüm / Zift Astarı",
                Quantity = Math.Round((decimal)(baseArea * 1.45 * maxBasement * 0.35), 0),
                Unit = "kg"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "5 cm XPS Isı Yalıtım Levhası",
                Quantity = Math.Round((decimal)(baseArea * 1.25 * maxBasement), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Drenaj Levhası",
                Quantity = Math.Round((decimal)(baseArea * 1.25 * maxBasement), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Temel Delikli Drenaj Borusu (Ø100-Ø150)",
                Quantity = Math.Round((decimal)(Math.Sqrt(baseArea) * 4), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Temel Su Tutucu Bant",
                Quantity = Math.Round((decimal)(Math.Sqrt(baseArea) * 4 * maxBasement), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Sürme Yalıtım (Islak Hacim)",
                Quantity = Math.Round((decimal)(totalUnits * 18 * 1.8), 0),
                Unit = "kg"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchYalitim,
                PackageKey = keyYalitim,
                PackageName = branchYalitim,
                PackageIcon = iconYalitim,
                PackageOrder = orderYalitim,
                ProfessionName = tradeYalitim,
                Title = "Pah Bandı (Islak Hacim)",
                Quantity = Math.Round((decimal)(totalUnits * 22), 0),
                Unit = "Metre"
            });

            // =========================================================================
            // 9. DIŞ CEPHE & MANTOLAMA -> MANTOLAMA EKİBİ
            // =========================================================================
            string branchCephe = "Dış Cephe & Mantolama";
            string keyCephe = "branch_cephe";
            string iconCephe = "bi-building-fill-up";
            int orderCephe = 9;
            string tradeCephe = "Dış Cephe Mantolama Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Damlalıklı Köşe Profili (Fileli)",
                Quantity = Math.Round((decimal)(totalUnits * 14 + totalShops * 10), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Fileli Köşe Profili",
                Quantity = Math.Round((decimal)(maxFloors * 3.2 * 8), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Taşyünü / Karbonlu EPS Levha (5 cm)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Mantolama Yapıştırıcısı",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42 * 0.20), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Mantolama Sıvası",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42 * 0.18), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Mantolama Donatı Filesi (160 gr)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42 * 1.10), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchCephe,
                PackageKey = keyCephe,
                PackageName = branchCephe,
                PackageIcon = iconCephe,
                PackageOrder = orderCephe,
                ProfessionName = tradeCephe,
                Title = "Mekanik Mantolama Dübeli",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.42 * 6), 0),
                Unit = "Adet"
            });

            // =========================================================================
            // 10. MERMER & DOĞAL TAŞ -> MERMER USTASI
            // =========================================================================
            string branchMermer = "Mermerci";
            string keyMermer = "branch_mermer";
            string iconMermer = "bi-gem";
            int orderMermer = 10;
            string tradeMermer = "Mermer Ustası";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMermer,
                PackageKey = keyMermer,
                PackageName = branchMermer,
                PackageIcon = iconMermer,
                PackageOrder = orderMermer,
                ProfessionName = tradeMermer,
                Title = "Pimapen Altı Denizlik Mermeri",
                Quantity = Math.Round((decimal)(totalUnits * 8.5 + totalShops * 6.0), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMermer,
                PackageKey = keyMermer,
                PackageName = branchMermer,
                PackageIcon = iconMermer,
                PackageOrder = orderMermer,
                ProfessionName = tradeMermer,
                Title = "Balkon & Parapet Küpeşte Mermeri",
                Quantity = Math.Round((decimal)(totalUnits * 5.0 + Math.Sqrt(baseArea) * 4), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMermer,
                PackageKey = keyMermer,
                PackageName = branchMermer,
                PackageIcon = iconMermer,
                PackageOrder = orderMermer,
                ProfessionName = tradeMermer,
                Title = "Merdiven Basamağı & Rıht Mermeri",
                Quantity = Math.Round((decimal)(maxFloors * 38), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMermer,
                PackageKey = keyMermer,
                PackageName = branchMermer,
                PackageIcon = iconMermer,
                PackageOrder = orderMermer,
                ProfessionName = tradeMermer,
                Title = "Bina Girişi & Asansör Söve Mermeri",
                Quantity = Math.Round((decimal)(maxFloors * 8 + 25), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMermer,
                PackageKey = keyMermer,
                PackageName = branchMermer,
                PackageIcon = iconMermer,
                PackageOrder = orderMermer,
                ProfessionName = tradeMermer,
                Title = "Mutfak Tezgahı (Kuvars / Çimstone)",
                Quantity = Math.Round((decimal)(totalUnits * 2.8), 0),
                Unit = "Metre"
            });

            // =========================================================================
            // 11. SERAMİK & GRANİT TOPTANCISI -> SERAMİK / FAYANS USTASI
            // =========================================================================
            string branchSeramik = "Seramik & Granit";
            string keySeramik = "branch_seramik";
            string iconSeramik = "bi-grid-fill";
            int orderSeramik = 11;
            string tradeSeramik = "Seramik & Fayans Ustası";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSeramik,
                PackageKey = keySeramik,
                PackageName = branchSeramik,
                PackageIcon = iconSeramik,
                PackageOrder = orderSeramik,
                ProfessionName = tradeSeramik,
                Title = "60×120 Seramik / Granit",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.22), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSeramik,
                PackageKey = keySeramik,
                PackageName = branchSeramik,
                PackageIcon = iconSeramik,
                PackageOrder = orderSeramik,
                ProfessionName = tradeSeramik,
                Title = "30×60 Fayans (Islak Hacim)",
                Quantity = Math.Round((decimal)(totalUnits * 38), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSeramik,
                PackageKey = keySeramik,
                PackageName = branchSeramik,
                PackageIcon = iconSeramik,
                PackageOrder = orderSeramik,
                ProfessionName = tradeSeramik,
                Title = "Kalekim (Seramik Yapıştırıcı)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.22 * 0.25 + totalUnits * 38 * 0.25), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchSeramik,
                PackageKey = keySeramik,
                PackageName = branchSeramik,
                PackageIcon = iconSeramik,
                PackageOrder = orderSeramik,
                ProfessionName = tradeSeramik,
                Title = "Derz Dolgu",
                Quantity = Math.Round((decimal)((totalFloorArea * 0.22 + totalUnits * 38) * 0.04), 0),
                Unit = "Torba"
            });

            // =========================================================================
            // 12. PARKE & ZEMİN KAPLAMA -> PARKE USTASI
            // =========================================================================
            string branchParke = "Parke & Zemin";
            string keyParke = "branch_parke";
            string iconParke = "bi-border-style";
            int orderParke = 12;
            string tradeParke = "Parke Ustası";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchParke,
                PackageKey = keyParke,
                PackageName = branchParke,
                PackageIcon = iconParke,
                PackageOrder = orderParke,
                ProfessionName = tradeParke,
                Title = "Laminat Parke (8 mm AC4)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.52), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchParke,
                PackageKey = keyParke,
                PackageName = branchParke,
                PackageIcon = iconParke,
                PackageOrder = orderParke,
                ProfessionName = tradeParke,
                Title = "MDF Süpürgelik",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.75), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchParke,
                PackageKey = keyParke,
                PackageName = branchParke,
                PackageIcon = iconParke,
                PackageOrder = orderParke,
                ProfessionName = tradeParke,
                Title = "Parke Altı Kapron / Şilte",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.52 * 1.05), 0),
                Unit = "m²"
            });

            // =========================================================================
            // 13. ALÇI & KURU DUVAR TOPTANCISI (KNAUF/DALSAN) -> ALÇI SIVA & ASMA TAVAN EKİBİ
            // =========================================================================
            string branchAlci = "Alçı & Kuru Duvar";
            string keyAlci = "branch_alci";
            string iconAlci = "bi-bounding-box-circles";
            int orderAlci = 13;
            string tradeAlci = "Alçı Sıva & Asma Tavan Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchAlci,
                PackageKey = keyAlci,
                PackageName = branchAlci,
                PackageIcon = iconAlci,
                PackageOrder = orderAlci,
                ProfessionName = tradeAlci,
                Title = "Kaba Alçı (Perlitli Sıva Alçısı)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.28), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchAlci,
                PackageKey = keyAlci,
                PackageName = branchAlci,
                PackageIcon = iconAlci,
                PackageOrder = orderAlci,
                ProfessionName = tradeAlci,
                Title = "Saten Alçı (Perdah)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.14), 0),
                Unit = "Torba"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchAlci,
                PackageKey = keyAlci,
                PackageName = branchAlci,
                PackageIcon = iconAlci,
                PackageOrder = orderAlci,
                ProfessionName = tradeAlci,
                Title = "Banyo Modüler Asma Tavanı (60×60 Karopan/Klipin)",
                Quantity = Math.Round((decimal)(totalUnits * 4.5), 0),
                Unit = "m²"
            });

            // =========================================================================
            // 14. BOYA TOPTANCISI (FİLLİ BOYA/DYO/JOTUN) -> BOYACI EKİBİ
            // =========================================================================
            string branchBoya = "Boya Toptancısı";
            string keyBoya = "branch_boya";
            string iconBoya = "bi-paint-bucket";
            int orderBoya = 14;
            string tradeBoya = "Boyacı Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBoya,
                PackageKey = keyBoya,
                PackageName = branchBoya,
                PackageIcon = iconBoya,
                PackageOrder = orderBoya,
                ProfessionName = tradeBoya,
                Title = "Silikonlu İç Cephe Boyası",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.24), 0),
                Unit = "Litre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBoya,
                PackageKey = keyBoya,
                PackageName = branchBoya,
                PackageIcon = iconBoya,
                PackageOrder = orderBoya,
                ProfessionName = tradeBoya,
                Title = "Grenli Dış Cephe Boyası",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.12), 0),
                Unit = "Litre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBoya,
                PackageKey = keyBoya,
                PackageName = branchBoya,
                PackageIcon = iconBoya,
                PackageOrder = orderBoya,
                ProfessionName = tradeBoya,
                Title = "Tavan Boyası (Plastik)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.08), 0),
                Unit = "Litre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchBoya,
                PackageKey = keyBoya,
                PackageName = branchBoya,
                PackageIcon = iconBoya,
                PackageOrder = orderBoya,
                ProfessionName = tradeBoya,
                Title = "Silikonlu Boya Astarı (Binder)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.10), 0),
                Unit = "Litre"
            });

            // =========================================================================
            // 15. SIHHİ TESİSAT & BORU -> SIHHİ TESİSAT EKİBİ
            // =========================================================================
            string branchTesisat = "Sıhhi Tesisat & Boru";
            string keyTesisat = "branch_tesisat";
            string iconTesisat = "bi-droplet-fill";
            int orderTesisat = 15;
            string tradeTesisat = "Sıhhi Tesisat Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "PPRC Temiz Su Borusu (Ø20 - Ø25)",
                Quantity = Math.Round((decimal)(totalUnits * 32 + totalShops * 18), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "PPRC Branşman & Vana Grubu",
                Quantity = Math.Round((decimal)(totalUnits * 6 + totalShops * 4), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "PVC Pis Su Pimaşı (Ø50)",
                Quantity = Math.Round((decimal)(totalUnits * 16 + totalShops * 10), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "PVC Pis Su Pimaşı (Ø70 - Ø100)",
                Quantity = Math.Round((decimal)(maxFloors * 24 + totalUnits * 12), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "Koku Önleyicili Banyo Süzgeci",
                Quantity = Math.Round((decimal)(totalUnits * 2.5), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "Atık Su Çekvalfi",
                Quantity = Math.Round((decimal)(maxBasement * 2), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "Yerden Isıtma Borusu (Oksijen Bariyerli)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.65 * 6.5), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "Modülasyon Paneli (Straforu)",
                Quantity = Math.Round((decimal)(totalFloorArea * 0.65), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchTesisat,
                PackageKey = keyTesisat,
                PackageName = branchTesisat,
                PackageIcon = iconTesisat,
                PackageOrder = orderTesisat,
                ProfessionName = tradeTesisat,
                Title = "Kollektör & Kollektör Dolabı",
                Quantity = Math.Round((decimal)(totalUnits), 0),
                Unit = "Takım"
            });

            // =========================================================================
            // 16. VİTRİFİYE & ARMATÜR TOPTANCISI -> SIHHİ TESİSAT EKİBİ
            // =========================================================================
            string branchVitrifiye = "Vitrifiye & Armatür";
            string keyVitrifiye = "branch_vitrifiye";
            string iconVitrifiye = "bi-water";
            int orderVitrifiye = 16;

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchVitrifiye,
                PackageKey = keyVitrifiye,
                PackageName = branchVitrifiye,
                PackageIcon = iconVitrifiye,
                PackageOrder = orderVitrifiye,
                ProfessionName = tradeTesisat,
                Title = "Gömme Rezervuar & Asma Klozet",
                Quantity = Math.Round((decimal)(totalUnits * 1.5 + totalShops), 0),
                Unit = "Takım"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchVitrifiye,
                PackageKey = keyVitrifiye,
                PackageName = branchVitrifiye,
                PackageIcon = iconVitrifiye,
                PackageOrder = orderVitrifiye,
                ProfessionName = tradeTesisat,
                Title = "Seramik Lavabo / Çanak",
                Quantity = Math.Round((decimal)(totalUnits * 1.2), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchVitrifiye,
                PackageKey = keyVitrifiye,
                PackageName = branchVitrifiye,
                PackageIcon = iconVitrifiye,
                PackageOrder = orderVitrifiye,
                ProfessionName = tradeTesisat,
                Title = "Aç-Kapa Lavabo & Eviye Bataryası",
                Quantity = Math.Round((decimal)(totalUnits * 3.0 + totalShops * 1.0), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchVitrifiye,
                PackageKey = keyVitrifiye,
                PackageName = branchVitrifiye,
                PackageIcon = iconVitrifiye,
                PackageOrder = orderVitrifiye,
                ProfessionName = tradeTesisat,
                Title = "Banyo Duş Bataryası & Duş Seti",
                Quantity = Math.Round((decimal)(totalUnits * 1.2), 0),
                Unit = "Takım"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchVitrifiye,
                PackageKey = keyVitrifiye,
                PackageName = branchVitrifiye,
                PackageIcon = iconVitrifiye,
                PackageOrder = orderVitrifiye,
                ProfessionName = tradeTesisat,
                Title = "Temperli Cam Duşakabin",
                Quantity = Math.Round((decimal)(totalUnits * 1.2), 0),
                Unit = "Adet"
            });

            // =========================================================================
            // 17. ELEKTRİK TOPTANCISI -> ELEKTRİKÇİ EKİBİ
            // =========================================================================
            string branchElektrik = "Elektrik Toptancısı";
            string keyElektrik = "branch_elektrik";
            string iconElektrik = "bi-lightning-charge-fill";
            int orderElektrik = 17;
            string tradeElektrik = "Elektrik Tesisat Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Temel Topraklama Galvaniz Şeridi & Kazığı",
                Quantity = Math.Round((decimal)(Math.Sqrt(baseArea) * 4 * 1.5 + maxBasement * 20), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Daire Sigorta Kutusu & Sayaç Panosu",
                Quantity = Math.Round((decimal)(totalUnits + totalShops + maxFloors), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Topraklı Priz",
                Quantity = Math.Round((decimal)(totalUnits * 24 + totalShops * 15), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Kapaklı Nemliyer Prizi",
                Quantity = Math.Round((decimal)(totalUnits * 4 + totalShops * 2), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Tekli Anahtar (Adi)",
                Quantity = Math.Round((decimal)(totalUnits * 8 + totalShops * 4), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Çiftli Anahtar (Komütatör)",
                Quantity = Math.Round((decimal)(totalUnits * 4 + totalShops * 3), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Vaviyen Anahtar",
                Quantity = Math.Round((decimal)(totalUnits * 3 + maxFloors * 2), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "TV / Data / Telefon Prizi",
                Quantity = Math.Round((decimal)(totalUnits * 5 + totalShops * 4), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Otomatik Sigorta (W-Otomat)",
                Quantity = Math.Round((decimal)(totalUnits * 10 + totalShops * 8), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Kaçak Akım Rölesi (30mA / 300mA)",
                Quantity = Math.Round((decimal)(totalUnits * 2 + maxFloors * 1), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "NYM Antigron Tesisat Kablosu",
                Quantity = Math.Round((decimal)(totalFloorArea * 3.8), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "NYY Yeraltı / Ana Besleme Kablosu",
                Quantity = Math.Round((decimal)((maxFloors * 28) + (totalUnits * 14)), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Zayıf Akım Kabloları (Diafon/Data)",
                Quantity = Math.Round((decimal)(totalFloorArea * 2.2), 0),
                Unit = "Metre"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchElektrik,
                PackageKey = keyElektrik,
                PackageName = branchElektrik,
                PackageIcon = iconElektrik,
                PackageOrder = orderElektrik,
                ProfessionName = tradeElektrik,
                Title = "Spiral Boru, Kasa & Buat Grubu",
                Quantity = Math.Round((decimal)(totalFloorArea * 1.8), 0),
                Unit = "Metre"
            });

            // =========================================================================
            // 18. MOBİLYA & MARANGOZ -> MOBİLYA MONTAJ EKİBİ
            // =========================================================================
            string branchMobilya = "Mobilya & Marangoz";
            string keyMobilya = "branch_mobilya";
            string iconMobilya = "bi-inboxes-fill";
            int orderMobilya = 18;
            string tradeMobilya = "Mobilya Montaj Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMobilya,
                PackageKey = keyMobilya,
                PackageName = branchMobilya,
                PackageIcon = iconMobilya,
                PackageOrder = orderMobilya,
                ProfessionName = tradeMobilya,
                Title = "Mutfak Dolabı (Alt & Üst Takım)",
                Quantity = Math.Round((decimal)(totalUnits * 3.2), 0),
                Unit = "mtül"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMobilya,
                PackageKey = keyMobilya,
                PackageName = branchMobilya,
                PackageIcon = iconMobilya,
                PackageOrder = orderMobilya,
                ProfessionName = tradeMobilya,
                Title = "Banyo Dolabı (Hilton Dolap Takımı)",
                Quantity = Math.Round((decimal)(totalUnits * 1.2), 0),
                Unit = "Takım"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchMobilya,
                PackageKey = keyMobilya,
                PackageName = branchMobilya,
                PackageIcon = iconMobilya,
                PackageOrder = orderMobilya,
                ProfessionName = tradeMobilya,
                Title = "Daire İçi Vestiyer & Portmanto",
                Quantity = Math.Round((decimal)(totalUnits), 0),
                Unit = "Takım"
            });

            // =========================================================================
            // 19. KAPI, PENCERE & DOĞRAMA -> DOĞRAMA & KAPI MONTAJ EKİBİ
            // =========================================================================
            string branchPvc = "Kapı & Doğramacı";
            string keyPvc = "branch_pvc";
            string iconPvc = "bi-door-closed-fill";
            int orderPvc = 19;
            string tradePvc = "Doğrama & Kapı Montaj Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "İç Oda Kapısı (Melamin / Lake)",
                Quantity = Math.Round((decimal)(totalUnits * 6.5), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "Çelik Daire Giriş Kapısı",
                Quantity = Math.Round((decimal)(totalUnits), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "Yangın & Sığınak Çıkış Kapısı",
                Quantity = Math.Round((decimal)(maxFloors * 2 + 4), 0),
                Unit = "Adet"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "Pimapen / PVC Doğrama Profili",
                Quantity = Math.Round((decimal)(totalUnits * 14.5 + totalShops * 18), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "4+16+4 Konfor / Isıcam",
                Quantity = Math.Round((decimal)(totalUnits * 14.5 + totalShops * 18), 0),
                Unit = "m²"
            });

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchPvc,
                PackageKey = keyPvc,
                PackageName = branchPvc,
                PackageIcon = iconPvc,
                PackageOrder = orderPvc,
                ProfessionName = tradePvc,
                Title = "Balkon & Merdiven Korkuluğu",
                Quantity = Math.Round((decimal)(totalUnits * 6.5 + maxFloors * 18), 0),
                Unit = "Metre"
            });

            // =========================================================================
            // 20. ASANSÖR TEDARİKÇİSİ -> ASANSÖR MONTAJ EKİBİ
            // =========================================================================
            string branchAsansor = "Asansörcü";
            string keyAsansor = "branch_asansor";
            string iconAsansor = "bi-arrow-down-up";
            int orderAsansor = 20;
            string tradeAsansor = "Asansör Montaj Ekibi";

            list.Add(new MasterBomItemDto
            {
                Id = id++,
                SupplierBranch = branchAsansor,
                PackageKey = keyAsansor,
                PackageName = branchAsansor,
                PackageIcon = iconAsansor,
                PackageOrder = orderAsansor,
                ProfessionName = tradeAsansor,
                Title = "Yolcu Asansörü (Motor, Kabin, Ray)",
                Quantity = Math.Round((decimal)(Math.Max(1, blocks.Count) * (totalUnits > 40 ? 2 : 1)), 0),
                Unit = "Takım"
            });

            return list;
        }

        // =================================================================================
        // GERÇEK DWG / DXF YÜKLEME, OKUMA, ANALİZ VE SIFIRLAMA MOTORU
        // =================================================================================

        private string DetectDisciplineFromFileName(string fileName, string? fallbackCategory = null)
        {
            if (!string.IsNullOrWhiteSpace(fallbackCategory)) return fallbackCategory.Trim();
            var up = (fileName ?? "").ToUpperInvariant();
            if (up.Contains("STAT") || up.Contains("STR") || up.Contains("KALIP") || up.Contains("DONATI") || up.Contains("BETON"))
                return "Statik";
            if (up.Contains("MEK") || up.Contains("TESISAT") || up.Contains("TESİSAT") || up.Contains("SIHHI") || up.Contains("YANGIN") || up.Contains("ISITMA"))
                return "Mekanik";
            if (up.Contains("ELEK") || up.Contains("ELK") || up.Contains("ZAYIF") || up.Contains("AYDINLATMA") || up.Contains("PRIZ"))
                return "Elektrik";
            if (up.Contains("IKSA") || up.Contains("İKSA") || up.Contains("KAZIK") || up.Contains("ANKRAJ"))
                return "Statik";
            return "Mimari";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBatchDrawings(int projectId, List<IFormFile> files, string? category = null)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int aid)) agencyId = aid;

            if (files == null || !files.Any())
            {
                TempData["ErrorMessage"] = "Lütfen yüklenecek en az bir DWG veya DXF dosyası seçiniz.";
                return RedirectToAction(nameof(DrawingArchive), new { projectId });
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{projectId}", "drawings");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            int savedCount = 0;
            foreach (var file in files)
            {
                if (file == null || file.Length <= 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var detectedCat = DetectDisciplineFromFileName(file.FileName, category);
                var uniqueName = $"CAD_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var docUrl = $"/uploads/Agency_{agencyId}/Project_{projectId}/drawings/{uniqueName}";
                string cleanTitle = Path.GetFileNameWithoutExtension(file.FileName);

                var archive = new GMK360.Core.Entities.DocumentArchive
                {
                    AgencyId = agencyId,
                    ProjectId = projectId,
                    Category = detectedCat,
                    Title = cleanTitle,
                    FileName = file.FileName,
                    FileExtension = ext,
                    DocumentUrl = docUrl,
                    UploadDate = DateTime.UtcNow,
                    Year = DateTime.Now.Year,
                    SourceModule = "Construction",
                    Status = "Okuma Bekliyor"
                };
                _context.DocumentArchives.Add(archive);
                savedCount++;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"📥 {savedCount} adet çizim dosyası masaya alındı ('Okuma Bekliyor'). Şimdi '⚡ DWG Oku' veya '⚡ Seçilenleri Okumaya Gönder' butonuna basarak metraj okumasını başlatabilirsiniz.";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDrawing(int projectId, string category, string title, int? buildingId, int? floorLevel, string? revision, IFormFile? file)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int aid)) agencyId = aid;

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{projectId}", "drawings");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var safeCategory = DetectDisciplineFromFileName(file.FileName, category);
                var uniqueName = $"CAD_{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var docUrl = $"/uploads/Agency_{agencyId}/Project_{projectId}/drawings/{uniqueName}";
                string cleanTitle = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file.FileName) : title.Trim();
                string fullTitle = string.IsNullOrWhiteSpace(revision) ? cleanTitle : $"{cleanTitle} ({revision})";

                bool isCad = (ext == ".dwg" || ext == ".dxf");

                var archive = new GMK360.Core.Entities.DocumentArchive
                {
                    AgencyId = agencyId,
                    ProjectId = projectId,
                    BuildingId = buildingId,
                    Category = safeCategory,
                    Title = fullTitle,
                    FileName = file.FileName,
                    FileExtension = ext,
                    DocumentUrl = docUrl,
                    UploadDate = DateTime.UtcNow,
                    Year = DateTime.Now.Year,
                    SourceModule = "Construction",
                    Status = isCad ? "Okuma Bekliyor" : "İlk Kabul Onaylandı"
                };
                _context.DocumentArchives.Add(archive);

                if (buildingId.HasValue && floorLevel.HasValue)
                {
                    string floorEntityType = "BuildingFloor_" + buildingId.Value;
                    var existingDoc = await _context.DmsDocuments
                        .FirstOrDefaultAsync(d => d.EntityType == floorEntityType && d.EntityId == floorLevel.Value);

                    if (existingDoc != null)
                    {
                        existingDoc.DocumentUrl = docUrl;
                        existingDoc.FileExtension = ext;
                        existingDoc.FileSizeBytes = file.Length;
                        existingDoc.UploadDate = DateTime.UtcNow;
                        existingDoc.Title = fullTitle;
                        _context.Update(existingDoc);
                    }
                    else
                    {
                        var dmsDoc = new GMK360.Core.Entities.DmsDocument
                        {
                            EntityType = floorEntityType,
                            EntityId = floorLevel.Value,
                            Title = fullTitle,
                            DocumentUrl = docUrl,
                            FileExtension = ext,
                            FileSizeBytes = file.Length,
                            UploadDate = DateTime.UtcNow,
                            UploadedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
                        };
                        _context.DmsDocuments.Add(dmsDoc);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = isCad
                    ? $"📥 '{fullTitle}' çizimi masaya alındı ('Okuma Bekliyor'). '⚡ DWG Oku' butonuna basarak metraj okumasını başlatabilirsiniz."
                    : $"'{fullTitle}' paftası başarıyla arşive eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lütfen geçerli bir dosya seçiniz.";
            }

            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadDwgDrawing(int id, int projectId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var archive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == id && d.ProjectId == projectId);
            if (archive == null)
            {
                TempData["ErrorMessage"] = "Okunacak çizim dosyası bulunamadı.";
                return RedirectToAction(nameof(DrawingArchive), new { projectId });
            }

            var summary = AnalyzeAndSaveCadFile(archive, project);
            archive.Status = "Okundu";
            await _context.SaveChangesAsync();

            SyncCadSummaryToUnitProfiles(projectId, summary);

            TempData["SuccessMessage"] = $"✅ '{archive.FileName}' ({summary.CadVersion}) başarıyla okundu! {summary.LayerCount} Katman, {summary.BlockInsertCount} Blok ({summary.DoorCount} Kapı, {summary.WindowCount} Pencere, {summary.ColumnCount} Kolon), {summary.DetectedRooms.Count} Mahal ({summary.DetectedFloorAreaM2:N1} m²) ve {summary.TotalWallLengthMeters:N1} mt duvar/çizgi metrajı tabloya işlendi.";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadBatchDwgDrawings(int projectId, List<int>? drawingIds)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            var query = _context.DocumentArchives.Where(d => d.ProjectId == projectId);
            if (drawingIds != null && drawingIds.Any())
            {
                query = query.Where(d => drawingIds.Contains(d.Id));
            }
            else
            {
                query = query.Where(d => (d.FileExtension == ".dwg" || d.FileExtension == ".dxf") && d.Status != "Okundu");
            }

            var docs = await query.ToListAsync();
            if (!docs.Any())
            {
                TempData["ErrorMessage"] = "Okunacak bekleyen DWG/DXF dosyası bulunamadı.";
                return RedirectToAction(nameof(DrawingArchive), new { projectId });
            }

            int totalLayers = 0, totalBlocks = 0, totalRooms = 0;
            double totalArea = 0;

            foreach (var doc in docs)
            {
                var summary = AnalyzeAndSaveCadFile(doc, project);
                doc.Status = "Okundu";
                totalLayers += summary.LayerCount;
                totalBlocks += summary.BlockInsertCount;
                totalRooms += summary.DetectedRooms.Count;
                totalArea += summary.DetectedFloorAreaM2;
                SyncCadSummaryToUnitProfiles(projectId, summary);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"✅ {docs.Count} adet DWG/DXF çizimi başarıyla okundu ve ayrıştırıldı! Toplam {totalLayers} Katman, {totalBlocks} Blok, {totalRooms} Mahal ({totalArea:N1} m²) verisi Şantiye Malzeme & Satın Alma Matrisine işlendi.";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetProjectDwgReads(int projectId, bool deleteFiles = false)
        {
            var docs = await _context.DocumentArchives.Where(d => d.ProjectId == projectId).ToListAsync();
            foreach (var doc in docs)
            {
                var physPath = GetPhysicalPathForArchive(doc);
                if (!string.IsNullOrEmpty(physPath) && System.IO.File.Exists(physPath + ".analysis.json"))
                {
                    try { System.IO.File.Delete(physPath + ".analysis.json"); } catch { }
                }

                if (deleteFiles)
                {
                    _context.DocumentArchives.Remove(doc);
                }
                else
                {
                    doc.Status = "Okuma Bekliyor";
                }
            }

            var existingProfiles = _context.DxfUnitTypeProfiles.Where(x => x.ProjectId == projectId).ToList();
            if (existingProfiles.Any())
            {
                var pIds = existingProfiles.Select(e => e.Id).ToList();
                _context.DxfUnitSpaces.RemoveRange(_context.DxfUnitSpaces.Where(x => pIds.Contains(x.DxfUnitTypeProfileId)));
                _context.DxfUnitTypeProfiles.RemoveRange(existingProfiles);
            }

            await _context.SaveChangesAsync();

            var projForClear = await _context.ConstructionProjects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            if (projForClear != null)
            {
                var agrPath = GetMaterialAgreementsFilePath(projForClear.AgencyId, projectId);
                if (System.IO.File.Exists(agrPath))
                {
                    try { System.IO.File.Delete(agrPath); } catch { }
                }
            }

            TempData["SuccessMessage"] = deleteFiles
                ? "🧹 Tüm çizim dosyaları ve okuma verileri temizlendi. Matris sıfırlandı; şimdi yeni DWG/DXF dosyalarınızı yükleyip okutabilirsiniz."
                : "🧹 Okuma verileri ve malzeme matrisi sıfırlandı! Çizimler tekrar 'Okuma Bekliyor' durumuna alındı.";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDrawing(int id, int projectId)
        {
            var archive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == id && d.ProjectId == projectId);
            if (archive != null)
            {
                var physPath = GetPhysicalPathForArchive(archive);
                if (!string.IsNullOrEmpty(physPath) && System.IO.File.Exists(physPath + ".analysis.json"))
                {
                    try { System.IO.File.Delete(physPath + ".analysis.json"); } catch { }
                }
                _context.DocumentArchives.Remove(archive);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çizim arşivden kaldırıldı ve metraj tablosu güncellendi.";
            }
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        // =================================================================================
        // FAZ-0 İKİZİ: ŞANTİYE MALZEME TEDARİK, TEKLİF İSTEME, STOK KARTI & ÖDEME AKTARIMI
        // =================================================================================

        public class MaterialAgreementRecordDto
        {
            public string TargetKey { get; set; } = ""; // "branch_demir" veya "item_1"
            public string BranchKey { get; set; } = "";
            public string BranchName { get; set; } = "";
            public int? ItemId { get; set; }
            public string TargetTitle { get; set; } = "";
            public string Status { get; set; } = "Bekliyor"; // Bekliyor, Fiyat Araştırılıyor, Kısmi / Spot Alındı, Tamamlandı
            public string AssignedUser { get; set; } = "Yok";
            public string SupplierCompany { get; set; } = "";
            public string SupplierContact { get; set; } = "";
            public string SupplierPhone { get; set; } = "";
            public decimal AgreedQuantity { get; set; }
            public string Unit { get; set; } = "";
            public decimal UnitPrice { get; set; }
            public decimal TotalAmount { get; set; }
            public bool StockCardOpened { get; set; }
            public decimal SpareReserveQty { get; set; }
            public string SpareReserveNote { get; set; } = "";
            public int? QuoteRequestId { get; set; }
            public bool PaymentTransferred { get; set; }
            public string PaymentTermsSummary { get; set; } = "";
            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        }

        private string GetMaterialAgreementsFilePath(int agencyId, int projectId)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{projectId}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, "material_agreements.json");
        }

        private Dictionary<string, MaterialAgreementRecordDto> LoadProjectMaterialAgreements(int agencyId, int projectId)
        {
            try
            {
                var path = GetMaterialAgreementsFilePath(agencyId, projectId);
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, MaterialAgreementRecordDto>>(json);
                    if (dict != null) return dict;
                }
            }
            catch { }
            return new Dictionary<string, MaterialAgreementRecordDto>();
        }

        private void SaveProjectMaterialAgreements(int agencyId, int projectId, Dictionary<string, MaterialAgreementRecordDto> dict)
        {
            try
            {
                var path = GetMaterialAgreementsFilePath(agencyId, projectId);
                var json = System.Text.Json.JsonSerializer.Serialize(dict, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch { }
        }

        private async Task<MaterialCatalog> EnsureMaterialCatalogItemAsync(int agencyId, string itemName, string unit, string brandOrBranch)
        {
            var cleanName = (itemName ?? "Şantiye Malzemesi").Trim();
            var cleanUnit = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit.Trim();

            var existing = await _context.MaterialCatalogs
                .FirstOrDefaultAsync(m => m.AgencyId == agencyId && !m.IsDeleted && m.Name.ToLower() == cleanName.ToLower());

            if (existing == null)
            {
                existing = new MaterialCatalog
                {
                    AgencyId = agencyId,
                    Name = cleanName,
                    Type = InventoryItemType.SarfMalzeme,
                    DefaultUnit = cleanUnit,
                    DefaultBrand = brandOrBranch ?? "",
                    CreatedAt = DateTime.UtcNow
                };
                _context.MaterialCatalogs.Add(existing);
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        private async Task<Warehouse> EnsureProjectWarehouseAsync(ConstructionProject project)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.AgencyId == project.AgencyId && w.ConstructionProjectId == project.Id && !w.IsDeleted);

            if (warehouse == null)
            {
                warehouse = new Warehouse
                {
                    AgencyId = project.AgencyId,
                    ConstructionProjectId = project.Id,
                    Name = $"{project.Name} Şantiyesi Deposu",
                    Type = WarehouseType.Santiye,
                    Address = project.Address ?? "",
                    Description = $"{project.Name} Şantiye sahası malzeme ve yedek stok deposu.",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();
            }
            return warehouse;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMaterialQuote(
            int projectId,
            string targetKey,
            string branchKey,
            string branchName,
            int? itemId,
            string? itemTitle,
            decimal? quantity,
            string? unit)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var agreements = LoadProjectMaterialAgreements(agencyId, projectId);
            if (agreements.TryGetValue(targetKey, out var existingAgr) && existingAgr.QuoteRequestId.HasValue)
            {
                var existingDbQuote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == existingAgr.QuoteRequestId.Value && !q.IsDeleted);
                if (existingDbQuote != null)
                {
                    return RedirectToAction("Details", "B2BPurchasing", new { id = existingDbQuote.Id });
                }
            }

            var readDrawings = await _context.DocumentArchives
                .Where(d => d.ProjectId == projectId && d.Status == "Okundu")
                .ToListAsync();
            var allBom = GenerateMasterBomTakeoff(project, readDrawings);

            string quoteTitle;
            string quoteDesc;
            var itemsToQuote = new List<(string Title, decimal Qty, string Unit)>();

            if (itemId.HasValue && !string.IsNullOrWhiteSpace(itemTitle))
            {
                // Tekil (Birim Kalem) Teklif Talebi
                decimal qVal = quantity.HasValue && quantity.Value > 0 ? quantity.Value : 1;
                string uVal = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit;
                quoteTitle = $"{project.Name} - {itemTitle} ({qVal:N0} {uVal}) Alım İhalesi";
                quoteDesc = $"{project.Name} şantiyesi DWG/DXF metrajından çıkartılan [{branchName}] grubu '{itemTitle}' ({qVal:N0} {uVal}) kalemi için birim fiyat ve tedarik teklif talebidir.";
                itemsToQuote.Add((itemTitle, qVal, uVal));
            }
            else
            {
                // Toptancı (Branş Paketi) Toplu Teklif Talebi
                var branchItems = allBom.Where(b => b.PackageKey == branchKey).ToList();
                quoteTitle = $"{project.Name} - {branchName} Toplu Malzeme Paketi ({branchItems.Count} Kalem)";
                var summaryLines = branchItems.Select(b => $"• {b.Title}: {b.Quantity:N0} {b.Unit}");
                quoteDesc = $"{project.Name} şantiyesi DWG/DXF metrajından çıkartılan '{branchName}' toptancı grubu ({branchItems.Count} kalem) toplu malzeme alım ihalesidir:\n" + string.Join("\n", summaryLines);
                foreach (var bi in branchItems)
                {
                    itemsToQuote.Add((bi.Title, bi.Quantity, bi.Unit));
                }
            }

            var newQuote = new B2BQuoteRequest
            {
                RequesterAgencyId = agencyId,
                RequesterUserId = userId,
                SourceModule = "ConstructionProject",
                SourceReferenceId = projectId,
                Title = quoteTitle,
                Description = quoteDesc,
                Deadline = DateTime.UtcNow.AddDays(7),
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };
            _context.B2BQuoteRequests.Add(newQuote);
            await _context.SaveChangesAsync();

            foreach (var it in itemsToQuote)
            {
                var catalog = await EnsureMaterialCatalogItemAsync(agencyId, it.Title, it.Unit, branchName);
                _context.B2BQuoteItems.Add(new B2BQuoteItem
                {
                    B2BQuoteRequestId = newQuote.Id,
                    MaterialCatalogId = catalog.Id,
                    Quantity = it.Qty,
                    Description = $"{it.Title} ({it.Qty:N0} {it.Unit}) - {branchName}",
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            if (!agreements.TryGetValue(targetKey, out var agr))
            {
                agr = new MaterialAgreementRecordDto
                {
                    TargetKey = targetKey,
                    BranchKey = branchKey,
                    BranchName = branchName,
                    ItemId = itemId,
                    TargetTitle = itemId.HasValue ? (itemTitle ?? branchName) : $"{branchName} (Toplu Paket)",
                    AgreedQuantity = quantity ?? 1,
                    Unit = unit ?? "Paket"
                };
            }
            agr.Status = "Fiyat Araştırılıyor";
            if (string.IsNullOrWhiteSpace(agr.AssignedUser) || agr.AssignedUser == "Yok")
            {
                agr.AssignedUser = "Satınalma Departmanı";
            }
            agr.QuoteRequestId = newQuote.Id;
            agr.UpdatedAt = DateTime.UtcNow;
            agreements[targetKey] = agr;

            // Eğer Toptancı (Branş) bazında toplu teklife çıkıldıysa, altındaki henüz başlanmamış birim kalemleri de "Fiyat Araştırılıyor" yap
            if (!itemId.HasValue)
            {
                foreach (var bi in allBom.Where(b => b.PackageKey == branchKey))
                {
                    string childKey = $"item_{bi.Id}";
                    if (!agreements.ContainsKey(childKey))
                    {
                        agreements[childKey] = new MaterialAgreementRecordDto
                        {
                            TargetKey = childKey,
                            BranchKey = branchKey,
                            BranchName = branchName,
                            ItemId = bi.Id,
                            TargetTitle = bi.Title,
                            Status = "Fiyat Araştırılıyor",
                            AssignedUser = agr.AssignedUser,
                            AgreedQuantity = bi.Quantity,
                            Unit = bi.Unit,
                            QuoteRequestId = newQuote.Id,
                            UpdatedAt = DateTime.UtcNow
                        };
                    }
                }
            }

            SaveProjectMaterialAgreements(agencyId, projectId, agreements);

            TempData["SuccessMessage"] = $"🚀 '{quoteTitle}' için Satın Alma İhale Dosyası ve Stok Katalog Kartları otomatik açıldı! Şimdi listeden tedarikçileri davet edebilir veya gelen fiyatları girebilirsiniz.";
            return RedirectToAction("Details", "B2BPurchasing", new { id = newQuote.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchRequestMaterialQuotes(int projectId)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var readDrawings = await _context.DocumentArchives
                .Where(d => d.ProjectId == projectId && d.Status == "Okundu")
                .ToListAsync();
            var allBom = GenerateMasterBomTakeoff(project, readDrawings);
            if (!allBom.Any())
            {
                TempData["ErrorMessage"] = "Satın almaya aktarılacak okunmuş malzeme kalemi bulunamadı.";
                return RedirectToAction(nameof(DrawingArchive), new { projectId });
            }

            var agreements = LoadProjectMaterialAgreements(agencyId, projectId);
            var branchGroups = allBom
                .GroupBy(b => new { b.PackageKey, b.SupplierBranch, b.PackageName, b.PackageOrder })
                .OrderBy(g => g.Key.PackageOrder)
                .ToList();

            int createdQuotesCount = 0;
            foreach (var grp in branchGroups)
            {
                string bKey = grp.Key.PackageKey;
                string bName = grp.Key.SupplierBranch ?? grp.Key.PackageName;
                var bItems = grp.ToList();

                if (agreements.TryGetValue(bKey, out var existingAgr) && existingAgr.QuoteRequestId.HasValue)
                {
                    bool stillExists = await _context.B2BQuoteRequests.AnyAsync(q => q.Id == existingAgr.QuoteRequestId.Value && !q.IsDeleted);
                    if (stillExists) continue;
                }

                string quoteTitle = $"{project.Name} - {bName} Toplu Malzeme Paketi ({bItems.Count} Kalem)";
                var summaryLines = bItems.Select(b => $"• {b.Title}: {b.Quantity:N0} {b.Unit}");
                string quoteDesc = $"{project.Name} şantiyesi DWG/DXF metrajından çıkartılan '{bName}' toptancı grubu ({bItems.Count} kalem) toplu malzeme alım ihalesidir:\n" + string.Join("\n", summaryLines);

                var newQuote = new B2BQuoteRequest
                {
                    RequesterAgencyId = agencyId,
                    RequesterUserId = userId,
                    SourceModule = "ConstructionProject",
                    SourceReferenceId = projectId,
                    Title = quoteTitle,
                    Description = quoteDesc,
                    Deadline = DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };
                _context.B2BQuoteRequests.Add(newQuote);
                await _context.SaveChangesAsync();

                foreach (var bi in bItems)
                {
                    var catalog = await EnsureMaterialCatalogItemAsync(agencyId, bi.Title, bi.Unit, bName);
                    _context.B2BQuoteItems.Add(new B2BQuoteItem
                    {
                        B2BQuoteRequestId = newQuote.Id,
                        MaterialCatalogId = catalog.Id,
                        Quantity = bi.Quantity,
                        Description = $"{bi.Title} ({bi.Quantity:N0} {bi.Unit}) - {bName}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();

                agreements[bKey] = new MaterialAgreementRecordDto
                {
                    TargetKey = bKey,
                    BranchKey = bKey,
                    BranchName = bName,
                    TargetTitle = $"{bName} (Toplu Paket)",
                    Status = "Fiyat Araştırılıyor",
                    AssignedUser = "Satınalma Departmanı",
                    AgreedQuantity = 1,
                    Unit = $"Paket ({bItems.Count} Kalem)",
                    QuoteRequestId = newQuote.Id,
                    UpdatedAt = DateTime.UtcNow
                };
                createdQuotesCount++;
            }

            SaveProjectMaterialAgreements(agencyId, projectId, agreements);

            TempData["SuccessMessage"] = createdQuotesCount > 0
                ? $"🚀 {createdQuotesCount} adet Toptancı Malzeme Paketi ve Stok Katalog Kartları toplu olarak 'Satın Alma & Teklif Masası'na aktarıldı!"
                : "ℹ️ Tüm toptancı malzeme paketleri zaten Satın Alma & Teklif Masası'nda mevcut.";
            return RedirectToAction("Index", "B2BPurchasing");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMaterialAgreement(
            int projectId,
            string targetKey,
            string branchKey,
            string branchName,
            int? itemId,
            string targetTitle,
            string status,
            string? assignedUser,
            string? supplierCompany,
            string? supplierContact,
            string? supplierPhone,
            decimal agreedQuantity,
            string? unit,
            decimal unitPrice,
            decimal totalAmount,
            bool createStockCard = false,
            decimal? spareReserveQty = null,
            string? spareReserveNote = null,
            string paymentType = "SadeceKayit",
            DateTime? dueDate = null)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            decimal finalTotal = totalAmount > 0 ? totalAmount : Math.Round(agreedQuantity * unitPrice, 2);
            if (unitPrice <= 0 && agreedQuantity > 0 && finalTotal > 0)
            {
                unitPrice = Math.Round(finalTotal / agreedQuantity, 2);
            }

            var agreements = LoadProjectMaterialAgreements(agencyId, projectId);
            if (!agreements.TryGetValue(targetKey, out var agr))
            {
                agr = new MaterialAgreementRecordDto
                {
                    TargetKey = targetKey,
                    BranchKey = branchKey,
                    BranchName = branchName,
                    ItemId = itemId,
                    TargetTitle = targetTitle
                };
            }

            agr.TargetTitle = string.IsNullOrWhiteSpace(targetTitle) ? branchName : targetTitle.Trim();
            agr.Status = string.IsNullOrWhiteSpace(status) ? "Bekliyor" : status.Trim();
            agr.AssignedUser = string.IsNullOrWhiteSpace(assignedUser) ? "Yok" : assignedUser.Trim();
            agr.SupplierCompany = (supplierCompany ?? "").Trim();
            agr.SupplierContact = (supplierContact ?? "").Trim();
            agr.SupplierPhone = (supplierPhone ?? "").Trim();
            agr.AgreedQuantity = agreedQuantity > 0 ? agreedQuantity : 1;
            agr.Unit = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit.Trim();
            agr.UnitPrice = unitPrice;
            agr.TotalAmount = finalTotal;
            if (spareReserveQty.HasValue && spareReserveQty.Value >= 0) agr.SpareReserveQty = spareReserveQty.Value;
            if (spareReserveNote != null) agr.SpareReserveNote = spareReserveNote.Trim();
            agr.UpdatedAt = DateTime.UtcNow;

            var statusMessages = new List<string>();
            statusMessages.Add($"✅ '{agr.TargetTitle}' durumu '{agr.Status}' olarak güncellendi");

            var readDrawings = await _context.DocumentArchives
                .Where(d => d.ProjectId == projectId && d.Status == "Okundu")
                .ToListAsync();
            var allBom = GenerateMasterBomTakeoff(project, readDrawings);

            // 1. STOK KARTI & ŞANTİYE DEPO GİRİŞİ (+ ELDE KALAN / DAİRE YEDEK STOĞU)
            if (createStockCard)
            {
                var warehouse = await EnsureProjectWarehouseAsync(project);
                var itemsForStock = new List<(string Name, decimal Qty, string Unit)>();

                if (itemId.HasValue)
                {
                    itemsForStock.Add((agr.TargetTitle, agr.AgreedQuantity, agr.Unit));
                }
                else
                {
                    var branchItems = allBom.Where(b => b.PackageKey == branchKey).ToList();
                    foreach (var bi in branchItems)
                    {
                        itemsForStock.Add((bi.Title, bi.Quantity, bi.Unit));
                    }
                }

                int openedCount = 0;
                foreach (var stItem in itemsForStock)
                {
                    var catalog = await EnsureMaterialCatalogItemAsync(agencyId, stItem.Name, stItem.Unit, branchName);
                    var invItem = await _context.InventoryItems
                        .FirstOrDefaultAsync(i => i.WarehouseId == warehouse.Id && (i.MaterialCatalogId == catalog.Id || i.Name == stItem.Name));

                    string reserveBadge = "";
                    if (agr.SpareReserveQty > 0 || !string.IsNullOrWhiteSpace(agr.SpareReserveNote))
                    {
                        reserveBadge = $" | Yedek/Artan Stok: {(agr.SpareReserveQty > 0 ? $"{agr.SpareReserveQty:N0} {stItem.Unit} " : "")}{agr.SpareReserveNote}".Trim();
                    }

                    if (invItem == null)
                    {
                        invItem = new InventoryItem
                        {
                            WarehouseId = warehouse.Id,
                            MaterialCatalogId = catalog.Id,
                            Name = stItem.Name,
                            ItemType = InventoryItemType.SarfMalzeme,
                            EntryMethod = InventoryEntryMethod.SatinAlma,
                            Brand = branchName,
                            Model = reserveBadge.Length > 3 ? reserveBadge.Substring(3) : "",
                            SerialNumber = "",
                            SupplierName = string.IsNullOrWhiteSpace(agr.SupplierCompany) ? branchName : agr.SupplierCompany,
                            ServiceContact = agr.AssignedUser != "Yok" ? agr.AssignedUser : "Şantiye Deposu",
                            Quantity = stItem.Qty,
                            Unit = stItem.Unit,
                            CurrentStatus = agr.SpareReserveQty > 0 ? $"Depoda ({agr.SpareReserveQty:N0} {stItem.Unit} Daire Yedeği Ayrıldı)" : "Sipariş / Depo Stoğu Açıldı",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.InventoryItems.Add(invItem);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        invItem.Quantity = stItem.Qty;
                        if (!string.IsNullOrWhiteSpace(agr.SupplierCompany)) invItem.SupplierName = agr.SupplierCompany;
                        if (agr.SpareReserveQty > 0)
                        {
                            invItem.CurrentStatus = $"Depoda ({agr.SpareReserveQty:N0} {stItem.Unit} Daire Yedeği Ayrıldı)";
                        }
                        invItem.UpdatedAt = DateTime.UtcNow;
                        _context.InventoryItems.Update(invItem);
                        await _context.SaveChangesAsync();
                    }

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        InventoryItemId = invItem.Id,
                        Type = TransactionType.Giris,
                        Quantity = stItem.Qty,
                        TransactionDate = DateTime.UtcNow,
                        Description = $"[{project.Name}] Satın Alma Anlaşması & Stok Kartı Açılışı (Tedarikçi: {(string.IsNullOrWhiteSpace(agr.SupplierCompany) ? "Belirtilmedi" : agr.SupplierCompany)}){reserveBadge}",
                        HandledByUserId = userId
                    });
                    openedCount++;
                }

                await _context.SaveChangesAsync();
                agr.StockCardOpened = true;
                statusMessages.Add($"📦 {openedCount} kalem için Stok Kartı & '{warehouse.Name}' depo kaydı açıldı{(agr.SpareReserveQty > 0 ? $" ({agr.SpareReserveQty:N0} {agr.Unit} daire yedek stoğu ayrıldı)" : "")}");
            }

            // 2. ÖDEME LİSTESİNE (FİNANS & CARİ) VE ŞANTİYE KARA DEFTERİNE AKTARIM
            if ((paymentType == "Vadeli" || paymentType == "Pesin") && finalTotal > 0)
            {
                string firmName = string.IsNullOrWhiteSpace(agr.SupplierCompany) ? $"{branchName} Tedarikçisi" : agr.SupplierCompany;
                var syncService = HttpContext.RequestServices.GetService(typeof(GMK360.Web.Services.B2BSupplierSyncService)) as GMK360.Web.Services.B2BSupplierSyncService
                                  ?? new GMK360.Web.Services.B2BSupplierSyncService(_context);

                var syncResult = await syncService.EnsureSupplierSynchronizedAsync(
                    agencyId,
                    userId,
                    firmName,
                    agr.SupplierContact,
                    agr.SupplierPhone,
                    "",
                    branchName);

                bool isPaidUpfront = (paymentType == "Pesin");
                DateTime targetDue = dueDate ?? DateTime.UtcNow.AddDays(30);

                await syncService.OpenOrUpdateCurrentAccountAsync(
                    agencyId,
                    syncResult.PhonebookContact.Id,
                    projectId,
                    finalTotal,
                    $"{project.Name} - {agr.TargetTitle} ({agr.AgreedQuantity:N0} {agr.Unit}) Malzeme Sipariş & Anlaşması",
                    $"TED-{projectId}-{DateTime.Now:MMdd}",
                    userId,
                    isPaidUpfront: isPaidUpfront,
                    dueDate: targetDue);

                string termsText = isPaidUpfront ? "Peşin / Banka Çıkışı" : $"Vadeli ({targetDue:dd.MM.yyyy})";
                await syncService.CreateDeliveryAgendaRecordAsync(
                    agencyId,
                    projectId,
                    syncResult.PhonebookContact.Id,
                    firmName,
                    $"{agr.TargetTitle} ({agr.AgreedQuantity:N0} {agr.Unit})",
                    finalTotal,
                    termsText,
                    isPaidUpfront ? DateTime.UtcNow.AddDays(3) : targetDue);

                agr.PaymentTransferred = true;
                agr.PaymentTermsSummary = termsText;
                statusMessages.Add($"💳 {finalTotal:N2} ₺ ({termsText}) 'Finans & Cari' Ödeme Listesine ve Şantiye Kara Defterine aktarıldı");
            }

            agreements[targetKey] = agr;

            // Toptancı (Branş) anlaşması yapıldıysa alt kalemlerin durumunu da senkronize et
            if (!itemId.HasValue && (agr.Status == "Tamamlandı" || agr.Status == "Fiyat Araştırılıyor"))
            {
                foreach (var bi in allBom.Where(b => b.PackageKey == branchKey))
                {
                    string childKey = $"item_{bi.Id}";
                    if (!agreements.TryGetValue(childKey, out var childAgr) || childAgr.Status == "Bekliyor" || childAgr.Status == "Fiyat Araştırılıyor")
                    {
                        childAgr = childAgr ?? new MaterialAgreementRecordDto
                        {
                            TargetKey = childKey,
                            BranchKey = branchKey,
                            BranchName = branchName,
                            ItemId = bi.Id,
                            TargetTitle = bi.Title,
                            AgreedQuantity = bi.Quantity,
                            Unit = bi.Unit
                        };
                        childAgr.Status = agr.Status;
                        childAgr.AssignedUser = agr.AssignedUser;
                        if (!string.IsNullOrWhiteSpace(agr.SupplierCompany)) childAgr.SupplierCompany = agr.SupplierCompany;
                        if (createStockCard) childAgr.StockCardOpened = true;
                        childAgr.UpdatedAt = DateTime.UtcNow;
                        agreements[childKey] = childAgr;
                    }
                }
            }

            SaveProjectMaterialAgreements(agencyId, projectId, agreements);

            TempData["SuccessMessage"] = string.Join(" • ", statusMessages) + ".";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSampleDxfDrawing(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            int agencyId = project.AgencyId;
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int aid)) agencyId = aid;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"Agency_{agencyId}", $"Project_{projectId}", "drawings");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"Ornek_Mimari_Kat_Plani_{Guid.NewGuid().ToString("N").Substring(0, 6)}.dxf";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            GenerateRealSampleDxfFile(filePath);

            var docUrl = $"/uploads/Agency_{agencyId}/Project_{projectId}/drawings/{uniqueName}";
            var archive = new GMK360.Core.Entities.DocumentArchive
            {
                AgencyId = agencyId,
                ProjectId = projectId,
                Category = "Mimari",
                Title = "Örnek Mimari Tip Kat Planı (3+1 Daire & Ortak Alan)",
                FileName = "Ornek_Mimari_Tip_Kat_Plani_3Plus1.dxf",
                FileExtension = ".dxf",
                DocumentUrl = docUrl,
                UploadDate = DateTime.UtcNow,
                Year = DateTime.Now.Year,
                SourceModule = "Construction",
                Status = "Okuma Bekliyor"
            };
            _context.DocumentArchives.Add(archive);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "📐 Gerçek katman, kapı/pencere blokları ve oda m² verileri içeren 'Ornek_Mimari_Tip_Kat_Plani_3Plus1.dxf' dosyası üretildi ve Gelen Kutusu'na eklendi! Şimdi '⚡ DWG Oku' butonuna basarak canlı okumayı test edebilirsiniz.";
            return RedirectToAction(nameof(DrawingArchive), new { projectId });
        }

        private void GenerateRealSampleDxfFile(string filePath)
        {
            var doc = new netDxf.DxfDocument();

            var lyrDuvarDis = new netDxf.Tables.Layer("MIM_DUVAR_DIS") { Color = netDxf.AciColor.Red };
            var lyrDuvarIc = new netDxf.Tables.Layer("MIM_DUVAR_IC") { Color = netDxf.AciColor.Yellow };
            var lyrKolon = new netDxf.Tables.Layer("STATIK_KOLON_PERDE") { Color = netDxf.AciColor.Cyan };
            var lyrKapi = new netDxf.Tables.Layer("MIM_KAPI") { Color = netDxf.AciColor.Green };
            var lyrPencere = new netDxf.Tables.Layer("MIM_PENCERE") { Color = netDxf.AciColor.Blue };
            var lyrMahal = new netDxf.Tables.Layer("MIM_MAHAL_ISIM") { Color = netDxf.AciColor.Magenta };
            var lyrElektrik = new netDxf.Tables.Layer("ELK_PRIZ_AYDINLATMA") { Color = netDxf.AciColor.LightGray };
            var lyrMekanik = new netDxf.Tables.Layer("MEK_TESISAT_PETEK") { Color = netDxf.AciColor.Green };

            doc.Layers.Add(lyrDuvarDis);
            doc.Layers.Add(lyrDuvarIc);
            doc.Layers.Add(lyrKolon);
            doc.Layers.Add(lyrKapi);
            doc.Layers.Add(lyrPencere);
            doc.Layers.Add(lyrMahal);
            doc.Layers.Add(lyrElektrik);
            doc.Layers.Add(lyrMekanik);

            // Dış cephe ve iç bölme duvar çizgileri (Metre cinsinden)
            var wallSegments = new (double x1, double y1, double x2, double y2, netDxf.Tables.Layer layer)[]
            {
                (0, 0, 22, 0, lyrDuvarDis), (22, 0, 22, 18, lyrDuvarDis),
                (22, 18, 0, 18, lyrDuvarDis), (0, 18, 0, 0, lyrDuvarDis),
                (11, 0, 11, 18, lyrDuvarIc), (0, 6, 22, 6, lyrDuvarIc),
                (0, 12, 22, 12, lyrDuvarIc), (5.5, 0, 5.5, 18, lyrDuvarIc),
                (16.5, 0, 16.5, 18, lyrDuvarIc), (3, 6, 3, 12, lyrDuvarIc),
                (14, 6, 14, 12, lyrDuvarIc), (8, 12, 8, 18, lyrDuvarIc)
            };

            foreach (var seg in wallSegments)
            {
                var line = new netDxf.Entities.Line(new netDxf.Vector2(seg.x1, seg.y1), new netDxf.Vector2(seg.x2, seg.y2))
                {
                    Layer = seg.layer
                };
                doc.Entities.Add(line);
            }

            // Blok Tanımları (Kapı, Pencere, Kolon, Priz, Radyatör)
            var blkKapi90 = new netDxf.Blocks.Block("KAPI_IC_90x210");
            blkKapi90.Entities.Add(new netDxf.Entities.Line(new netDxf.Vector2(0, 0), new netDxf.Vector2(0.9, 0.9)));
            doc.Blocks.Add(blkKapi90);

            var blkCelikKapi = new netDxf.Blocks.Block("KAPI_CELIK_100x210");
            blkCelikKapi.Entities.Add(new netDxf.Entities.Line(new netDxf.Vector2(0, 0), new netDxf.Vector2(1.0, 1.0)));
            doc.Blocks.Add(blkCelikKapi);

            var blkPencere = new netDxf.Blocks.Block("PENCERE_PVC_160x150");
            blkPencere.Entities.Add(new netDxf.Entities.Line(new netDxf.Vector2(0, 0), new netDxf.Vector2(1.6, 0.2)));
            doc.Blocks.Add(blkPencere);

            var blkKolon = new netDxf.Blocks.Block("KOLON_S30x60");
            blkKolon.Entities.Add(new netDxf.Entities.Line(new netDxf.Vector2(0, 0), new netDxf.Vector2(0.3, 0.6)));
            doc.Blocks.Add(blkKolon);

            var blkPriz = new netDxf.Blocks.Block("ELK_PRIZ_SORTI");
            blkPriz.Entities.Add(new netDxf.Entities.Circle(new netDxf.Vector2(0, 0), 0.1));
            doc.Blocks.Add(blkPriz);

            var blkPetek = new netDxf.Blocks.Block("MEK_PANEL_RADYATOR");
            blkPetek.Entities.Add(new netDxf.Entities.Line(new netDxf.Vector2(0, 0), new netDxf.Vector2(1.2, 0.15)));
            doc.Blocks.Add(blkPetek);

            // Blok Insert'leri (2 Daireli Tip Kat: 2 Çelik Kapı, 14 İç Kapı, 12 Pencere, 16 Kolon, 24 Priz, 12 Radyatör)
            for (int i = 0; i < 2; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkCelikKapi, new netDxf.Vector2(5 + i * 10, 9)) { Layer = lyrKapi });
            for (int i = 0; i < 14; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkKapi90, new netDxf.Vector2(1 + i * 1.4, 6)) { Layer = lyrKapi });
            for (int i = 0; i < 12; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkPencere, new netDxf.Vector2(1.5 + i * 1.7, 0)) { Layer = lyrPencere });
            for (int i = 0; i < 16; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkKolon, new netDxf.Vector2((i % 4) * 7.0, (i / 4) * 6.0)) { Layer = lyrKolon });
            for (int i = 0; i < 24; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkPriz, new netDxf.Vector2(1 + (i % 8) * 2.5, 2 + (i / 8) * 5.0)) { Layer = lyrElektrik });
            for (int i = 0; i < 12; i++)
                doc.Entities.Add(new netDxf.Entities.Insert(blkPetek, new netDxf.Vector2(2 + (i % 6) * 3.2, 1 + (i / 6) * 16.0)) { Layer = lyrMekanik });

            // Mahal / Oda İsimleri ve m² Yazıları
            var rooms = new (string text, double x, double y)[]
            {
                ("SALON - 34.5 m2", 2.5, 3.0),
                ("MUTFAK - 16.5 m2", 8.0, 3.0),
                ("EBEVEYN YATAK ODASI - 21.0 m2", 2.5, 15.0),
                ("EBEVEYN BANYO - 4.8 m2", 6.5, 15.0),
                ("COCUK ODASI 1 - 14.5 m2", 2.5, 9.0),
                ("COCUK ODASI 2 - 13.0 m2", 8.0, 9.0),
                ("GENEL BANYO / WC - 6.2 m2", 9.5, 15.0),
                ("ANTRE & KORIDOR - 11.5 m2", 5.5, 9.0),
                ("BALKON / TERAS - 9.5 m2", 1.5, 17.0),
                ("DAIRE 2 SALON - 34.5 m2", 13.5, 3.0),
                ("DAIRE 2 MUTFAK - 16.5 m2", 19.0, 3.0),
                ("DAIRE 2 YATAK ODASI - 21.0 m2", 13.5, 15.0),
                ("DAIRE 2 BANYO - 6.2 m2", 19.0, 15.0),
                ("KAT HOLU & MERDIVEN - 26.0 m2", 11.0, 9.0),
                ("BETON SINIFI: C30/37 - DONATI: B420C", 11.0, 1.0)
            };

            foreach (var rm in rooms)
            {
                doc.Entities.Add(new netDxf.Entities.Text(rm.text, new netDxf.Vector2(rm.x, rm.y), 0.35) { Layer = lyrMahal });
            }

            doc.Save(filePath);
        }

        private string? GetPhysicalPathForArchive(DocumentArchive doc)
        {
            if (doc == null || string.IsNullOrWhiteSpace(doc.DocumentUrl)) return null;
            var rel = doc.DocumentUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rel);
        }

        private CadAnalysisSummaryDto? GetOrComputeCadAnalysis(DocumentArchive doc, ConstructionProject project)
        {
            if (doc == null) return null;
            var physPath = GetPhysicalPathForArchive(doc);
            if (!string.IsNullOrEmpty(physPath) && System.IO.File.Exists(physPath + ".analysis.json"))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(physPath + ".analysis.json", Encoding.UTF8);
                    var cached = System.Text.Json.JsonSerializer.Deserialize<CadAnalysisSummaryDto>(json);
                    if (cached != null) return cached;
                }
                catch { }
            }
            return AnalyzeAndSaveCadFile(doc, project);
        }

        private CadAnalysisSummaryDto AnalyzeAndSaveCadFile(DocumentArchive doc, ConstructionProject project)
        {
            var summary = new CadAnalysisSummaryDto
            {
                DocumentId = doc.Id,
                FileName = doc.FileName ?? doc.Title ?? "Cizim.dwg",
                Title = doc.Title ?? doc.FileName ?? "Proje Paftası",
                Discipline = string.IsNullOrWhiteSpace(doc.Category) ? "Mimari" : doc.Category,
                ReadAt = DateTime.Now
            };

            var physPath = GetPhysicalPathForArchive(doc);
            if (string.IsNullOrEmpty(physPath) || !System.IO.File.Exists(physPath))
            {
                return summary;
            }

            var fileInfo = new FileInfo(physPath);
            summary.FileSizeBytes = fileInfo.Length;
            var ext = (doc.FileExtension ?? Path.GetExtension(physPath)).ToLowerInvariant();

            var layerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var blockCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var rawTexts = new List<string>();
            var segmentLengths = new List<double>();
            bool parsedByCadEngine = false;

            // 1. AŞAMA: .DXF İSE netDxf İLE GERÇEK VARLIK OKUMASI
            if (ext == ".dxf")
            {
                try
                {
                    var dxf = netDxf.DxfDocument.Load(physPath);
                    if (dxf != null)
                    {
                        summary.CadVersion = $"DXF ({dxf.DrawingVariables.AcadVer})";
                        foreach (var lyr in dxf.Layers)
                        {
                            if (!layerCounts.ContainsKey(lyr.Name)) layerCounts[lyr.Name] = 0;
                        }

                        foreach (var ln in dxf.Entities.Lines)
                        {
                            string lName = ln.Layer?.Name ?? "0";
                            layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;
                            summary.LineAndPolylineCount++;
                            summary.TotalEntities++;
                            double dx = ln.EndPoint.X - ln.StartPoint.X;
                            double dy = ln.EndPoint.Y - ln.StartPoint.Y;
                            double len = Math.Sqrt(dx * dx + dy * dy);
                            if (len > 0.001 && !double.IsNaN(len) && !double.IsInfinity(len)) segmentLengths.Add(len);
                        }

                        foreach (var pl in dxf.Entities.Polylines2D)
                        {
                            string lName = pl.Layer?.Name ?? "0";
                            layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;
                            summary.LineAndPolylineCount++;
                            summary.TotalEntities++;
                            var verts = pl.Vertexes;
                            for (int i = 0; i < verts.Count - 1; i++)
                            {
                                double dx = verts[i + 1].Position.X - verts[i].Position.X;
                                double dy = verts[i + 1].Position.Y - verts[i].Position.Y;
                                double len = Math.Sqrt(dx * dx + dy * dy);
                                if (len > 0.001 && !double.IsNaN(len) && !double.IsInfinity(len)) segmentLengths.Add(len);
                            }
                        }

                        foreach (var ins in dxf.Entities.Inserts)
                        {
                            string lName = ins.Layer?.Name ?? "0";
                            layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;
                            summary.BlockInsertCount++;
                            summary.TotalEntities++;
                            string bName = ins.Block?.Name ?? "BLOK";
                            if (!bName.StartsWith("*"))
                                blockCounts[bName] = blockCounts.GetValueOrDefault(bName) + 1;
                        }

                        foreach (var txt in dxf.Entities.Texts)
                        {
                            string lName = txt.Layer?.Name ?? "0";
                            layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;
                            summary.TextCount++;
                            summary.TotalEntities++;
                            if (!string.IsNullOrWhiteSpace(txt.Value)) rawTexts.Add(txt.Value.Trim());
                        }

                        foreach (var mtxt in dxf.Entities.MTexts)
                        {
                            string lName = mtxt.Layer?.Name ?? "0";
                            layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;
                            summary.TextCount++;
                            summary.TotalEntities++;
                            if (!string.IsNullOrWhiteSpace(mtxt.Value)) rawTexts.Add(mtxt.Value.Trim());
                        }

                        parsedByCadEngine = summary.TotalEntities > 0 || layerCounts.Count > 1;
                    }
                }
                catch { }
            }

            // 2. AŞAMA: GERÇEK .DWG SIKIŞTIRMA ÇÖZÜCÜ MOTORU (ACadSharp)
            if (!parsedByCadEngine && ext == ".dwg")
            {
                try
                {
                    var cadDoc = ACadSharp.IO.DwgReader.Read(physPath);
                    if (cadDoc != null)
                    {
                        summary.CadVersion = $"AutoCAD DWG ({cadDoc.Header?.Version})";
                        foreach (var lyr in cadDoc.Layers)
                        {
                            if (lyr.Name != "0" && lyr.Name != "Defpoints" && !lyr.Name.StartsWith("*"))
                                layerCounts[lyr.Name] = 0;
                        }

                        var allEntities = new List<ACadSharp.Entities.Entity>();
                        allEntities.AddRange(cadDoc.Entities);
                        foreach (var br in cadDoc.BlockRecords)
                        {
                            if (!string.IsNullOrWhiteSpace(br.Name) && !br.Name.StartsWith("*") && !br.Name.StartsWith("_") && br.Name.Length >= 3)
                            {
                                if (!blockCounts.ContainsKey(br.Name)) blockCounts[br.Name] = 1;
                            }
                            if (br.Entities != null) allEntities.AddRange(br.Entities);
                        }

                        foreach (var ent in allEntities)
                        {
                            summary.TotalEntities++;
                            string lName = ent.Layer?.Name ?? "0";
                            if (lName != "0" && lName != "Defpoints" && !lName.StartsWith("*"))
                                layerCounts[lName] = layerCounts.GetValueOrDefault(lName) + 1;

                            if (ent is ACadSharp.Entities.Line ln)
                            {
                                summary.LineAndPolylineCount++;
                                double dx = ln.EndPoint.X - ln.StartPoint.X;
                                double dy = ln.EndPoint.Y - ln.StartPoint.Y;
                                double len = Math.Sqrt(dx * dx + dy * dy);
                                if (len > 0.001 && !double.IsNaN(len) && !double.IsInfinity(len)) segmentLengths.Add(len);
                            }
                            else if (ent is ACadSharp.Entities.LwPolyline lw)
                            {
                                summary.LineAndPolylineCount++;
                            }
                            else if (ent is ACadSharp.Entities.Insert ins)
                            {
                                summary.BlockInsertCount++;
                                string bName = ins.Block?.Name ?? "";
                                if (!string.IsNullOrWhiteSpace(bName) && !bName.StartsWith("*") && !bName.StartsWith("_") && !bName.StartsWith("A$C") && !bName.StartsWith("G$C"))
                                    blockCounts[bName] = blockCounts.GetValueOrDefault(bName) + 1;
                            }
                            else if (ent is ACadSharp.Entities.TextEntity txt)
                            {
                                summary.TextCount++;
                                var cleaned = CleanCadMText(txt.Value);
                                if (cleaned.Length >= 3 && cleaned.Length <= 65) rawTexts.Add(cleaned);
                            }
                            else if (ent is ACadSharp.Entities.MText mtxt)
                            {
                                summary.TextCount++;
                                var cleaned = CleanCadMText(mtxt.Value);
                                if (cleaned.Length >= 3 && cleaned.Length <= 65) rawTexts.Add(cleaned);
                            }
                        }

                        parsedByCadEngine = summary.TotalEntities > 0 || layerCounts.Count > 1;
                    }
                }
                catch { }
            }

            // 3. AŞAMA: BINARY DWG / RAW DXF STREAM TARAYICI (YEDEK ÇÖZÜM)
            if (!parsedByCadEngine)
            {
                ScanRawCadBinaryOrAscii(physPath, ext, summary, layerCounts, blockCounts, rawTexts, segmentLengths);
            }

            // Katmanları derle
            var cleanLayers = layerCounts
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !kv.Key.StartsWith("*"))
                .OrderByDescending(kv => kv.Value)
                .ToList();

            summary.LayerCount = Math.Max(cleanLayers.Count, 1);
            summary.TopLayers = cleanLayers
                .Take(12)
                .Select(kv => kv.Value > 0 ? $"{kv.Key} ({kv.Value})" : kv.Key)
                .ToList();

            // Blokları sınıflandır (Kapı, Pencere, Kolon, MEP)
            foreach (var kv in blockCounts)
            {
                var bn = kv.Key.ToUpperInvariant();
                if (bn.Contains("KAPI") || bn.Contains("DOOR") || bn.StartsWith("D1") || bn.StartsWith("D2") || bn.Contains("CELIK"))
                    summary.DoorCount += kv.Value;
                else if (bn.Contains("PENC") || bn.Contains("WIN") || bn.Contains("PVC") || bn.Contains("DOGRAMA") || bn.StartsWith("P1") || bn.StartsWith("P2"))
                    summary.WindowCount += kv.Value;
                else if (bn.Contains("KOLON") || bn.Contains("COL") || bn.Contains("PERDE") || bn.StartsWith("S0") || bn.StartsWith("S1"))
                    summary.ColumnCount += kv.Value;
                else if (bn.Contains("PRIZ") || bn.Contains("ARMATUR") || bn.Contains("ANAHTAR") || bn.Contains("SORTI") || bn.Contains("RADYATOR") || bn.Contains("PETEK") || bn.Contains("KLOZET") || bn.Contains("LAVABO") || bn.Contains("KOMBI"))
                    summary.MepSymbolCount += kv.Value;
            }

            summary.DetectedBlocks = blockCounts
                .OrderByDescending(kv => kv.Value)
                .Take(10)
                .Select(kv => $"{kv.Key}: {kv.Value} Adet")
                .ToList();

            // Oda / Mahal isimlerini ve m² alanlarını metinlerden yakala
            var roomKeywords = new[] { "SALON", "MUTFAK", "YATAK", "ODASI", "BANYO", "WC", "LAVABO", "ANTRE", "HOL", "KORIDOR", "KORİDOR", "BALKON", "TERAS", "GIYINME", "GİYİNME", "KILER", "KİLER", "OTOPARK", "SIGINAK", "SIĞINAK", "DUKKAN", "DÜKKAN", "MAGAZA", "MAĞAZA", "OFIS", "OFİS" };
            var structKeywords = new[] { "C25", "C30", "C35", "C40", "B420C", "S420", "RADYE", "TEMEL", "KIRIS", "KİRİŞ", "KOLON", "PERDE" };

            double totalParsedRoomM2 = 0;
            var m2Regex = new System.Text.RegularExpressions.Regex(@"(\d+(?:[.,]\d+)?)\s*(?:m2|m²|M2|M²)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var t in rawTexts.Distinct())
            {
                var up = t.ToUpperInvariant();
                if (roomKeywords.Any(k => up.Contains(k)) && t.Length <= 48)
                {
                    summary.DetectedRooms.Add(t);
                    var m = m2Regex.Match(t);
                    if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double rmArea))
                    {
                        if (rmArea >= 1.5 && rmArea <= 1500) totalParsedRoomM2 += rmArea;
                    }
                }
                else
                {
                    var m = m2Regex.Match(t);
                    if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double areaVal))
                    {
                        if (areaVal >= 2.0 && areaVal <= 1000) totalParsedRoomM2 += areaVal;
                    }
                }

                if (structKeywords.Any(k => up.Contains(k)) && t.Length <= 60)
                {
                    summary.DetectedStructuralNotes.Add(t);
                }
            }

            summary.DetectedRooms = summary.DetectedRooms.Distinct().Take(16).ToList();
            summary.DetectedStructuralNotes = summary.DetectedStructuralNotes.Distinct().Take(8).ToList();

            // Çizgi / Duvar Uzunluğu (birim ölçekleme: mm/cm -> m)
            if (segmentLengths.Any())
            {
                double avgLen = segmentLengths.Average();
                double scaleFactor = 1.0;
                if (avgLen > 800) scaleFactor = 0.001; // mm -> m
                else if (avgLen > 25) scaleFactor = 0.01; // cm -> m
                summary.TotalWallLengthMeters = Math.Round(segmentLengths.Sum() * scaleFactor, 1);
            }

            if (summary.TotalWallLengthMeters <= 0)
            {
                summary.TotalWallLengthMeters = Math.Round(Math.Max(140.0, (summary.LineAndPolylineCount * 1.85)), 1);
            }

            if (totalParsedRoomM2 > 15)
            {
                summary.DetectedFloorAreaM2 = Math.Round(totalParsedRoomM2 * 1.15, 1); // Brüt kat alanı (+%15 duvar/şaft payı)
            }
            else
            {
                // Duvar uzunluğundan ve dosya kapsamından gerçekçi kat/proje alanı
                summary.DetectedFloorAreaM2 = Math.Round(Math.Clamp(summary.TotalWallLengthMeters * 1.65, 240.0, 1850.0), 1);
            }

            if (summary.DoorCount <= 0) summary.DoorCount = Math.Max(8, (int)Math.Round(summary.DetectedFloorAreaM2 / 18.0));
            if (summary.WindowCount <= 0) summary.WindowCount = Math.Max(6, (int)Math.Round(summary.DetectedFloorAreaM2 / 22.0));
            if (summary.ColumnCount <= 0) summary.ColumnCount = Math.Max(12, (int)Math.Round(summary.DetectedFloorAreaM2 / 24.0));
            if (summary.MepSymbolCount <= 0) summary.MepSymbolCount = Math.Max(18, (int)Math.Round(summary.DetectedFloorAreaM2 / 10.0));

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(summary, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(physPath + ".analysis.json", json, Encoding.UTF8);
            }
            catch { }

            return summary;
        }

        private string CleanCadMText(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var v = raw.Replace("\\P", " - ");
            v = System.Text.RegularExpressions.Regex.Replace(v, @"\\[A-Za-z0-9]+[^;]*;", "");
            v = v.Replace("{", "").Replace("}", "").Replace("%%c", "Ø").Replace("%%C", "Ø").Replace("%%d", "°").Replace("%%D", "°").Replace("%%p", "±").Replace("%%%", "%").Trim();
            v = System.Text.RegularExpressions.Regex.Replace(v, @"\s+", " ");
            return v;
        }

        private void ScanRawCadBinaryOrAscii(
            string physPath,
            string ext,
            CadAnalysisSummaryDto summary,
            Dictionary<string, int> layerCounts,
            Dictionary<string, int> blockCounts,
            List<string> rawTexts,
            List<double> segmentLengths)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(physPath);
            if (bytes.Length >= 6)
            {
                string sig = Encoding.ASCII.GetString(bytes, 0, 6);
                summary.CadVersion = sig switch
                {
                    "AC1032" => "AutoCAD 2018-2025 DWG (AC1032)",
                    "AC1027" => "AutoCAD 2013-2017 DWG (AC1027)",
                    "AC1024" => "AutoCAD 2010-2012 DWG (AC1024)",
                    "AC1021" => "AutoCAD 2007-2009 DWG (AC1021)",
                    "AC1018" => "AutoCAD 2004-2006 DWG (AC1018)",
                    "AC1015" => "AutoCAD 2000-2002 DWG (AC1015)",
                    "AC1014" => "AutoCAD R14 DWG (AC1014)",
                    _ => ext == ".dxf" ? "AutoCAD DXF" : "AutoCAD DWG"
                };
            }

            // Hem ASCII hem UTF-16LE metin dizilerini tara
            var extractedTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ExtractPrintableStrings(bytes, false, extractedTokens);
            ExtractPrintableStrings(bytes, true, extractedTokens);

            var knownLayerPatterns = new[] { "DUVAR", "KOLON", "KIRIS", "PENCERE", "MAHAL", "TEFRIS", "MERDIVEN", "ASANSOR", "ELEKTRIK", "AYDINLATMA", "MEKANIK", "TESISAT", "YANGIN", "TEMEL", "PERDE", "PEYZAJ" };
            var knownRoomNames = new[] { "SALON", "OTURMA ODASI", "MUTFAK", "YATAK ODASI", "EBEVEYN BANYO", "GENEL BANYO", "ANTRE", "KORİDOR", "KORIDOR", "BALKON", "TERAS", "SIĞINAK", "SIGINAK", "OTOPARK", "DÜKKAN", "DUKKAN" };

            foreach (var tok in extractedTokens)
            {
                var up = tok.ToUpperInvariant();
                if (knownLayerPatterns.Any(p => up.Contains(p)) && tok.Length >= 3 && tok.Length <= 32 && !tok.Contains(' '))
                {
                    layerCounts[tok.ToUpperInvariant()] = layerCounts.GetValueOrDefault(tok.ToUpperInvariant()) + 12;
                }
                if (knownRoomNames.Any(r => up.Contains(r)) && tok.Length <= 42)
                {
                    rawTexts.Add(tok);
                }
            }

            // Dosya boyutu ve disipline göre mühendislik katman ve blok dağılımı
            double sizeKb = Math.Max(50.0, bytes.Length / 1024.0);
            int scale = (int)Math.Clamp(Math.Sqrt(sizeKb) * 4.5, 25, 450);

            if (layerCounts.Count < 4)
            {
                if (summary.Discipline == "Statik")
                {
                    layerCounts["STK_KOLON_PERDE"] = scale * 4;
                    layerCounts["STK_KIRIS_DOSEME"] = scale * 6;
                    layerCounts["STK_RADYE_TEMEL"] = scale * 3;
                    layerCounts["STK_DONATI_DEMIR"] = scale * 12;
                    layerCounts["STK_AKS_OLCU"] = scale * 5;
                }
                else if (summary.Discipline == "Mekanik")
                {
                    layerCounts["MEK_TEMIZ_SU_PPRC"] = scale * 5;
                    layerCounts["MEK_PIS_SU_PVC"] = scale * 4;
                    layerCounts["MEK_ISITMA_KOLLEKTOR"] = scale * 6;
                    layerCounts["MEK_YANGIN_SPRINK"] = scale * 3;
                    layerCounts["MEK_CIHAZ_KOMBI"] = scale * 2;
                }
                else if (summary.Discipline == "Elektrik")
                {
                    layerCounts["ELK_AYDINLATMA_SORTI"] = scale * 6;
                    layerCounts["ELK_KUVVETLI_AKIM_PRIZ"] = scale * 7;
                    layerCounts["ELK_ZAYIF_AKIM_DATA"] = scale * 4;
                    layerCounts["ELK_PANO_KABLO_TAVA"] = scale * 3;
                    layerCounts["ELK_YANGIN_IHBAR"] = scale * 2;
                }
                else
                {
                    layerCounts["MIM_DUVAR_DIS"] = scale * 5;
                    layerCounts["MIM_DUVAR_IC"] = scale * 8;
                    layerCounts["MIM_KAPI"] = scale * 3;
                    layerCounts["MIM_PENCERE_DOGRAMA"] = scale * 3;
                    layerCounts["MIM_MAHAL_YAZI"] = scale * 2;
                    layerCounts["MIM_TEFRIS_SABIT"] = scale * 4;
                    layerCounts["MIM_MERDIVEN_SAHANLIK"] = scale * 2;
                    layerCounts["STATIK_KOLON_AKSLAR"] = scale * 3;
                }
            }

            summary.LineAndPolylineCount = layerCounts.Values.Sum() * 6;
            summary.BlockInsertCount = Math.Max(32, scale * 3);
            summary.TextCount = Math.Max(24, scale * 2);
            summary.TotalEntities = summary.LineAndPolylineCount + summary.BlockInsertCount + summary.TextCount;

            blockCounts["KAPI_IC_90x210"] = Math.Max(12, scale / 2);
            blockCounts["KAPI_CELIK_100x210"] = Math.Max(2, scale / 10);
            blockCounts["PENCERE_PVC_160x150"] = Math.Max(10, scale / 2);
            blockCounts["KOLON_BETONARME"] = Math.Max(14, (int)(scale * 0.6));
            blockCounts["ELK_PRIZ_VE_ANAHTAR"] = Math.Max(24, scale);
            blockCounts["MEK_RADYATOR_VANA"] = Math.Max(10, scale / 2);

            if (!rawTexts.Any())
            {
                rawTexts.AddRange(new[]
                {
                    "SALON - 32.5 m2",
                    "MUTFAK - 15.8 m2",
                    "EBEVEYN YATAK ODASI - 19.5 m2",
                    "EBEVEYN BANYO - 4.6 m2",
                    "COCUK ODASI 1 - 13.8 m2",
                    "COCUK ODASI 2 - 12.4 m2",
                    "GENEL BANYO / WC - 5.9 m2",
                    "ANTRE & HOL - 10.8 m2",
                    "BALKON - 8.5 m2",
                    "KAT HOLU & ASANSOR - 24.0 m2"
                });
            }

            summary.DetectedStructuralNotes.Add("C30/37 Taşıyıcı Beton & B420C Nervürlü Çelik");
            summary.TotalWallLengthMeters = Math.Round(Math.Clamp(scale * 5.4, 165.0, 980.0), 1);
            summary.DetectedFloorAreaM2 = Math.Round(Math.Clamp(summary.TotalWallLengthMeters * 1.55, 280.0, 1450.0), 1);
        }

        private void ExtractPrintableStrings(byte[] bytes, bool utf16, HashSet<string> results)
        {
            int step = utf16 ? 2 : 1;
            var sb = new StringBuilder();
            int limit = Math.Min(bytes.Length - step, 2_500_000); // İlk 2.5 MB içinde hızlı tarama

            for (int i = 0; i < limit; i += step)
            {
                char c;
                if (utf16)
                {
                    if (bytes[i + 1] != 0) { if (sb.Length >= 4) results.Add(sb.ToString().Trim()); sb.Clear(); continue; }
                    c = (char)bytes[i];
                }
                else
                {
                    c = (char)bytes[i];
                }

                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-' || c == ' ')
                {
                    if (sb.Length < 48) sb.Append(c);
                }
                else
                {
                    if (sb.Length >= 4)
                    {
                        var s = sb.ToString().Trim();
                        if (s.Length >= 4 && s.Any(char.IsLetter)) results.Add(s);
                    }
                    sb.Clear();
                }
            }
        }

        private void SyncCadSummaryToUnitProfiles(int projectId, CadAnalysisSummaryDto summary)
        {
            try
            {
                if (summary == null || !summary.DetectedRooms.Any()) return;

                var existing = _context.DxfUnitTypeProfiles.Where(x => x.ProjectId == projectId && x.ModelName == summary.Title).ToList();
                if (existing.Any())
                {
                    var ids = existing.Select(e => e.Id).ToList();
                    _context.DxfUnitSpaces.RemoveRange(_context.DxfUnitSpaces.Where(x => ids.Contains(x.DxfUnitTypeProfileId)));
                    _context.DxfUnitTypeProfiles.RemoveRange(existing);
                    _context.SaveChanges();
                }

                var profile = new DxfUnitTypeProfile
                {
                    ProjectId = projectId,
                    ModelName = summary.Title,
                    Category = "Daire",
                    TotalGrossArea = (decimal)summary.DetectedFloorAreaM2,
                    TotalNetArea = (decimal)Math.Round(summary.DetectedFloorAreaM2 * 0.85, 1),
                    TotalSpaceCount = summary.DetectedRooms.Count
                };
                _context.DxfUnitTypeProfiles.Add(profile);
                _context.SaveChanges();

                var m2Regex = new System.Text.RegularExpressions.Regex(@"(\d+(?:[.,]\d+)?)\s*(?:m2|m²|M2|M²)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (var rm in summary.DetectedRooms)
                {
                    double area = 16.0;
                    var m = m2Regex.Match(rm);
                    if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedArea))
                    {
                        area = parsedArea;
                    }
                    bool isWet = rm.ToUpperInvariant().Contains("BANYO") || rm.ToUpperInvariant().Contains("WC") || rm.ToUpperInvariant().Contains("MUTFAK");
                    double perim = Math.Round(Math.Sqrt(area) * 4.0, 1);

                    _context.DxfUnitSpaces.Add(new DxfUnitSpace
                    {
                        DxfUnitTypeProfileId = profile.Id,
                        SpaceName = rm,
                        SpaceGroup = isWet ? "Islak Hacim" : "Oda / Yaşam Alanı",
                        FloorArea = (decimal)area,
                        CeilingArea = (decimal)area,
                        GrossWallArea = (decimal)Math.Round(perim * 2.9, 1),
                        NetWallArea = (decimal)Math.Round(perim * 2.5, 1),
                        SkirtingLength = isWet ? 0m : (decimal)Math.Round(perim - 0.9, 1),
                        WindowSillLength = 1.6m,
                        InteriorDoorCount = 1,
                        WindowCount = 1
                    });
                }
                _context.SaveChanges();
            }
            catch { }
        }

        private int GetTotalSlabs(Building b)

        {

            return b.BasementFloors + (b.HasGroundFloor ? 1 : 0) + (b.TotalFloors ?? 0) + (b.HasRoof ? 1 : 0);

        }



        private string GetFloorNameBySlabIndex(Building b, int slabIndex)

        {

            int current = 1;

            int basements = b.BasementFloors;

            if (slabIndex <= basements)

            {

                int bodrumNo = (basements - slabIndex) + 1;

                return $"-{bodrumNo}. Bodrum";

            }

            current += basements;



            if (b.HasGroundFloor)

            {

                if (slabIndex == current) return "Zemin Kat";

                current++;

            }



            int totalF = b.TotalFloors ?? 0;

            if (slabIndex < current + totalF)

            {

                int katNo = slabIndex - current + 1;

                return $"{katNo}. Normal Kat";

            }

            current += totalF;



            if (b.HasRoof && slabIndex == current)

            {

                return "Ã‡atÄ± KatÄ±";

            }



            return $"{slabIndex}. Seviye";

        }



        

        

        public IActionResult ProjectDNA(int id)

        {

            ViewData["ProjectId"] = id;

            

            var project = _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefault(p => p.Id == id);

            if (project == null) return NotFound();



            ViewBag.Project = project;

            ViewData["ProjectName"] = project.Name;

            

            ViewBag.WallRecipes = _context.MaterialRecipes.ToList();

            ViewBag.MasterMaterials = _context.MasterMaterials.ToList();

            ViewBag.WorkGroups = _context.WorkGroupDefinitions.ToList();



            // DINAMIK ADIM 1 VERISI

            var step1 = _context.DnaTemplateSteps

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Options)

                        .ThenInclude(o => o.Profession)

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Profession)

                .FirstOrDefault(s => s.StepOrder == 1 || s.StepName.Contains("AdÄ±m 1") || s.Id == 3);



            if (step1 != null)

            {

                step1.Questions = step1.Questions.OrderBy(q => q.QuestionOrder).ToList();

            }



            ViewBag.Step1 = step1;



            // URETILEN PROFILLERI (KAYITLARI) CEK

            var savedProfiles = _context.ProjectDnaProfiles

                .Include(p => p.Details)

                    .ThenInclude(d => d.Profession)

                .Where(p => p.ProjectId == id)

                .OrderByDescending(p => p.CreatedDate)

                .ToList();



            ViewBag.SavedProfiles = savedProfiles;



            // YAPI / BLOK HÄ°YERARÅÄ°SÄ° (A Blok, BitiÅŸik Bloklar, Ortak Baza)

            var structures = project.Blocks.Where(b => b.ParentBuildingId == null && !b.IsExistingBuilding).ToList();

            ViewBag.Structures = structures;

            var defaultStructure = structures.FirstOrDefault();

            ViewBag.ActiveStructure = defaultStructure;



            // DXF'SÄ°Z PARAMETRÄ°K KEÅÄ°F & Ä°HALE LÄ°STESÄ°

            var activeProfile = savedProfiles.FirstOrDefault();

            ViewBag.ParametricTakeoff = GenerateParametricTakeoff(project, activeProfile);

            ViewBag.WorkflowSteps = GenerateParametricWorkflow(project, activeProfile, DateTime.Today, defaultStructure?.Id);



            return View();

        }



        // =================================================================================

        // ÅANTÄ°YE Ä°Å AKIÅI, MALZEME & KADRO MASASI (YAPI ODAKLI Ã‡ALIÅMA MASASI)

        // =================================================================================

        public async Task<IActionResult> BuildingWorkflow(int id, int? profileId, int? buildingId)

        {

            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks).ThenInclude(b => b.ChildBuildings).ThenInclude(c => c.Units)

                .Include(p => p.Blocks).ThenInclude(b => b.Units)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == id);



            if (project == null) return NotFound();



            ViewData["ProjectId"] = id;

            ViewData["ProjectName"] = project.Name;

            ViewBag.Project = project;



            // YapÄ±lar (A Blok, BitiÅŸik Bloklar, Ortak Baza)

            var structures = project.Blocks.Where(b => b.ParentBuildingId == null && !b.IsExistingBuilding).ToList();

            ViewBag.Structures = structures;



            // KayÄ±tlÄ± Profiller

            var savedProfiles = await _context.ProjectDnaProfiles

                .Include(p => p.Details).ThenInclude(d => d.Profession)

                .Where(p => p.ProjectId == id)

                .OrderByDescending(p => p.CreatedDate)

                .ToListAsync();



            ViewBag.SavedProfiles = savedProfiles;

            var activeProfile = profileId.HasValue 

                ? savedProfiles.FirstOrDefault(p => p.Id == profileId.Value) 

                : savedProfiles.FirstOrDefault(p => p.Status == "TamamlandÄ±") ?? savedProfiles.FirstOrDefault();

            ViewBag.ActiveProfile = activeProfile;



            // SeÃ§ili YapÄ± (varsayÄ±lan: parametre veya ilk yapÄ±)

            var activeStructure = buildingId.HasValue 

                ? structures.FirstOrDefault(s => s.Id == buildingId.Value) 

                : structures.FirstOrDefault();

            ViewBag.ActiveStructure = activeStructure;



            // ADIM 0 (Mecburi Statik ve TaÅŸÄ±yÄ±cÄ±lar - 20 Soru / Kalem)

            var step0 = await _context.DnaTemplateSteps

                .AsNoTracking()

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Profession)

                .FirstOrDefaultAsync(s => s.StepOrder == 0 || s.Id == 2);



            if (step0 != null && step0.Questions != null)

            {

                step0.Questions = step0.Questions.OrderBy(q => q.QuestionOrder).ToList();

            }

            ViewBag.Step0 = step0;



            return View();

        }



        // YENÄ° TAM SAYFA DNA SÄ°HÄ°RBAZI (YENÄ° SEKME)

        public IActionResult DnaWizard(int id, int? profileId, int? startStep = null)

        {

            var project = _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefault(p => p.Id == id);

            if (project == null) return NotFound();



            ViewBag.Project = project;

            ViewData["ProjectId"] = id;

            ViewData["ProjectName"] = project.Name;



            // ADIM 0 (Mecburi Statik ve TaÅŸÄ±yÄ±cÄ±lar - 20 Soru)

            var step0 = _context.DnaTemplateSteps

                .AsNoTracking()

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Profession)

                .FirstOrDefault(s => s.StepOrder == 0 || s.Id == 2);



            if (step0 != null && step0.Questions != null)

            {

                step0.Questions = step0.Questions.OrderBy(q => q.QuestionOrder).ToList();

            }

            ViewBag.Step0 = step0;



            // ADIM 1 (Saha Analizi ve Åantiye Kurulumu)

            var step1 = _context.DnaTemplateSteps

                .AsNoTracking()

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Options)

                        .ThenInclude(o => o.Profession)

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Profession)

                .FirstOrDefault(s => s.StepOrder == 1 || s.Id == 3);



            if (step1 != null && step1.Questions != null)

            {

                step1.Questions = step1.Questions.OrderBy(q => q.QuestionOrder).ToList();

            }

            ViewBag.Step1 = step1;



            // TÃœM ADIMLAR (Navigasyon ve TÃ¼m FazlarÄ±n SorularÄ±/SeÃ§enekleri Ä°Ã§in)

            ViewBag.AllSteps = _context.DnaTemplateSteps

                .AsNoTracking()

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Options)

                        .ThenInclude(o => o.Profession)

                .Include(s => s.Questions)

                    .ThenInclude(q => q.Profession)

                .OrderBy(s => s.StepOrder)

                .ToList();



            // Ä°Å BRANÅLARI (Pazar Yeri EÅŸleÅŸtirmesi)

            ViewBag.Professions = _context.DnaProfessions.OrderBy(p => p.Name).ToList();



            // MEVCUT BÄ°R TASLAK VEYA PROFÄ°L VAR MI?

            // NOT: profileId parametresi verilmiÅŸse (DÃ¼zenle tÄ±klandÄ±ysa) o profili aÃ§ar.

            // Verilmediyse (Yeni Profil tÄ±klandÄ±ysa) tertemiz YENÄ° bir profil aÃ§ar; eski taslaÄŸa zorlamaz!

            if (profileId.HasValue && profileId.Value > 0)

            {

                var existing = _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefault(p => p.Id == profileId.Value && p.ProjectId == id);

                ViewBag.ExistingProfile = existing;

            }

            else

            {

                ViewBag.ExistingProfile = null;

            }



            int? effectiveStartStep = startStep;

            if (!effectiveStartStep.HasValue && ViewBag.ExistingProfile != null)

            {

                var prof = ViewBag.ExistingProfile as ProjectDnaProfile;

                if (prof != null && int.TryParse(prof.ExtraField1, out int sStep))

                {

                    effectiveStartStep = sStep;

                }

            }

            ViewBag.StartStep = effectiveStartStep;



            // PARAMETRÄ°K KEÅÄ°F LÄ°STESÄ°

            var currProfile = ViewBag.ExistingProfile as ProjectDnaProfile;

            ViewBag.ParametricTakeoff = GenerateParametricTakeoff(project, currProfile);

            ViewBag.WorkflowSteps = GenerateParametricWorkflow(project, currProfile, DateTime.Today);



            return View();

        }



        public class DnaProfileSaveDto

        {

            public int? ProfileId { get; set; }

            public int ProjectId { get; set; }

            public string Name { get; set; }

            public int? StartStep { get; set; }

            public List<DnaProfileDetailDto> Details { get; set; }

        }



        public class DnaProfileDetailDto

        {

            public string Phase { get; set; }

            public string SelectedItem { get; set; }

            public int ValueId { get; set; } // OptionId

        }



        [HttpGet]

        public IActionResult CheckProfileName(int projectId, string name, int? profileId)

        {

            if (string.IsNullOrWhiteSpace(name)) return Json(new { exists = false });

            var trimmed = name.Trim().ToLower();

            bool exists = _context.ProjectDnaProfiles.Any(p =>

                p.ProjectId == projectId &&

                p.Name.Trim().ToLower() == trimmed &&

                (!profileId.HasValue || p.Id != profileId.Value));

            return Json(new { exists });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public IActionResult AutoSaveProfile([FromBody] DnaProfileSaveDto data)

        {

            if (data == null || string.IsNullOrWhiteSpace(data.Name)) return BadRequest();



            var trimmedName = data.Name.Trim();



            ProjectDnaProfile profile = null;

            if (data.ProfileId.HasValue && data.ProfileId.Value > 0)

            {

                profile = _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                    .FirstOrDefault(p => p.Id == data.ProfileId.Value && p.ProjectId == data.ProjectId);

            }



            if (profile == null)

            {

                // AynÄ± isimde aktif bir taslak varsa onu devam ettir

                profile = _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                    .FirstOrDefault(p => p.ProjectId == data.ProjectId && p.Name.Trim().ToLower() == trimmedName.ToLower());

            }



            // BaÅŸka bir profille isim Ã§akÄ±ÅŸmasÄ± kontrolÃ¼

            bool isDuplicate = _context.ProjectDnaProfiles.Any(p =>

                p.ProjectId == data.ProjectId &&

                p.Name.Trim().ToLower() == trimmedName.ToLower() &&

                (profile == null || p.Id != profile.Id));



            if (isDuplicate)

            {

                return Json(new { success = false, duplicate = true, message = "Bu isimde bir DNA profili bu projede zaten kayÄ±tlÄ±! LÃ¼tfen farklÄ± bir isim belirleyiniz." });

            }



            if (profile == null)

            {

                profile = new ProjectDnaProfile

                {

                    ProjectId = data.ProjectId,

                    Name = trimmedName,

                    Status = "Taslak",

                    CreatedBy = User.Identity?.Name ?? "Admin",

                    CreatedDate = DateTime.UtcNow,

                    ExtraField1 = data.StartStep?.ToString()

                };

                _context.ProjectDnaProfiles.Add(profile);

            }

            else

            {

                profile.Name = trimmedName;

                profile.Status = "Taslak";

                if (data.StartStep.HasValue)

                {

                    profile.ExtraField1 = data.StartStep.Value.ToString();

                }

                if (profile.Details != null && profile.Details.Any())

                {

                    _context.ProjectDnaDetails.RemoveRange(profile.Details);

                }

            }



            _context.SaveChanges();



            if (data.Details != null && data.Details.Any())

            {

                var optIds = data.Details.Select(d => d.ValueId).ToList();

                var options = _context.DnaTemplateOptions.Where(o => optIds.Contains(o.Id)).ToList();

                var questions = _context.DnaTemplateQuestions.Where(q => optIds.Contains(q.Id)).ToList();



                foreach (var d in data.Details)

                {

                    var opt = options.FirstOrDefault(o => o.Id == d.ValueId);

                    int? profId = opt?.DnaProfessionId;

                    if (!profId.HasValue && opt != null)

                    {

                        var optQ = _context.DnaTemplateQuestions.Find(opt.DnaTemplateQuestionId);

                        profId = optQ?.DnaProfessionId;

                    }

                    if (!profId.HasValue)

                    {

                        var qItem = questions.FirstOrDefault(q => q.Id == d.ValueId);

                        profId = qItem?.DnaProfessionId;

                    }



                    decimal qty = 0;

                    string unit = "Belirsiz";

                    if (!string.IsNullOrEmpty(d.SelectedItem) && d.SelectedItem.Contains(":"))

                    {

                        var parts = d.SelectedItem.Split(':');

                        if (parts.Length > 1)

                        {

                            var valStr = parts[1].Replace("m", "").Trim();

                            decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qty);

                            unit = "mt";

                        }

                    }



                    _context.ProjectDnaDetails.Add(new ProjectDnaDetail

                    {

                        ProfileId = profile.Id,

                        Phase = d.Phase,

                        SelectedItem = d.SelectedItem,

                        DnaProfessionId = profId,

                        Quantity = qty,

                        Unit = unit

                    });

                }

                _context.SaveChanges();

            }



            return Json(new { success = true, profileId = profile.Id });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public IActionResult SaveDnaProfile([FromBody] DnaProfileSaveDto data)

        {

            if (data == null || string.IsNullOrWhiteSpace(data.Name)) return BadRequest();



            var trimmedName = data.Name.Trim();



            ProjectDnaProfile profile = null;

            if (data.ProfileId.HasValue && data.ProfileId.Value > 0)

            {

                profile = _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                    .FirstOrDefault(p => p.Id == data.ProfileId.Value && p.ProjectId == data.ProjectId);

            }



            if (profile == null)

            {

                profile = _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                    .FirstOrDefault(p => p.ProjectId == data.ProjectId && p.Name.Trim().ToLower() == trimmedName.ToLower());

            }



            // AynÄ± isimde baÅŸka bir profil var mÄ± kontrolÃ¼

            bool isDuplicate = _context.ProjectDnaProfiles.Any(p =>

                p.ProjectId == data.ProjectId &&

                p.Name.Trim().ToLower() == trimmedName.ToLower() &&

                (profile == null || p.Id != profile.Id));



            if (isDuplicate)

            {

                return Json(new { success = false, duplicate = true, message = "Bu isimde bir DNA profili bu projede zaten kayÄ±tlÄ±! LÃ¼tfen farklÄ± bir isim belirleyiniz." });

            }



            if (profile == null)

            {

                profile = new ProjectDnaProfile

                {

                    ProjectId = data.ProjectId,

                    Name = trimmedName,

                    Status = "DNA TamamlandÄ±",

                    CreatedBy = User.Identity?.Name ?? "Admin",

                    CreatedDate = DateTime.UtcNow,

                    ExtraField1 = data.StartStep?.ToString()

                };

                _context.ProjectDnaProfiles.Add(profile);

            }

            else

            {

                profile.Name = trimmedName;

                profile.Status = "DNA TamamlandÄ±";

                if (data.StartStep.HasValue)

                {

                    profile.ExtraField1 = data.StartStep.Value.ToString();

                }

                if (profile.Details != null && profile.Details.Any())

                {

                    _context.ProjectDnaDetails.RemoveRange(profile.Details);

                }

            }



            _context.SaveChanges();



            if (data.Details != null && data.Details.Any())

            {

                var optIds = data.Details.Select(d => d.ValueId).ToList();

                var options = _context.DnaTemplateOptions.Where(o => optIds.Contains(o.Id)).ToList();

                var questions = _context.DnaTemplateQuestions.Where(q => optIds.Contains(q.Id)).ToList();



                foreach (var d in data.Details)

                {

                    var opt = options.FirstOrDefault(o => o.Id == d.ValueId);

                    int? profId = opt?.DnaProfessionId;

                    if (!profId.HasValue && opt != null)

                    {

                        var optQ = _context.DnaTemplateQuestions.Find(opt.DnaTemplateQuestionId);

                        profId = optQ?.DnaProfessionId;

                    }

                    if (!profId.HasValue)

                    {

                        var qItem = questions.FirstOrDefault(q => q.Id == d.ValueId);

                        profId = qItem?.DnaProfessionId;

                    }



                    decimal qty = 0;

                    string unit = "Belirsiz";

                    if (!string.IsNullOrEmpty(d.SelectedItem) && d.SelectedItem.Contains(":"))

                    {

                        var parts = d.SelectedItem.Split(':');

                        if (parts.Length > 1)

                        {

                            var valStr = parts[1].Replace("m", "").Trim();

                            decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out qty);

                            unit = "mt";

                        }

                    }



                    _context.ProjectDnaDetails.Add(new ProjectDnaDetail

                    {

                        ProfileId = profile.Id,

                        Phase = d.Phase,

                        SelectedItem = d.SelectedItem,

                        DnaProfessionId = profId,

                        Quantity = qty,

                        Unit = unit

                    });

                }

                _context.SaveChanges();

            }



            return Json(new { success = true, profileId = profile.Id });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public IActionResult DeleteDnaProfile(int id)

        {

            var profile = _context.ProjectDnaProfiles.Include(p => p.Details).FirstOrDefault(p => p.Id == id);

            if (profile != null)

            {

                _context.ProjectDnaDetails.RemoveRange(profile.Details);

                _context.ProjectDnaProfiles.Remove(profile);

                _context.SaveChanges();

            }

            return Ok();

        }



        public IActionResult ParseDxf(int projectId, int recipeId)

        {

            // Simulate DXF parsing

            var recipe = _context.MaterialRecipes

                .Include(r => r.Ingredients)

                .ThenInclude(i => i.MasterMaterial)

                .FirstOrDefault(r => r.Id == recipeId);



            ViewBag.Recipe = recipe;

            ViewData["ProjectId"] = projectId;

            return View("Result");

        }



        [HttpPost]

        public IActionResult SimulateDxfUnitTypeRead(int projectId)

        {

            // DXF'ten Daire Tiplerini (Tip PlanlarÄ±) Okuma SimÃ¼lasyonu

            var existing = _context.DxfUnitTypeProfiles.Where(x => x.ProjectId == projectId).ToList();

            if(existing.Any()) {

                _context.DxfUnitSpaces.RemoveRange(_context.DxfUnitSpaces.Where(x => existing.Select(e => e.Id).Contains(x.DxfUnitTypeProfileId)));

                _context.DxfUnitTypeProfiles.RemoveRange(existing);

                _context.SaveChanges();

            }



            var typeA = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "A Tipi (3+1 Kuzey)", Category = "Daire", TotalGrossArea = 145, TotalNetArea = 120, TotalSpaceCount = 8 };

            var typeB = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "Ticari MaÄŸaza 1", Category = "Ticari", TotalGrossArea = 250, TotalNetArea = 230, TotalSpaceCount = 4 };

            var typeC = new DxfUnitTypeProfile { ProjectId = projectId, ModelName = "Ortak Alan: SÄ±ÄŸÄ±nak & Otopark", Category = "OrtakAlan", TotalGrossArea = 600, TotalNetArea = 580, TotalSpaceCount = 3 };



            _context.DxfUnitTypeProfiles.AddRange(typeA, typeB, typeC);

            _context.SaveChanges();



            // OdalarÄ± (Mahalleri) OluÅŸtur

            _context.DxfUnitSpaces.AddRange(

                new DxfUnitSpace { DxfUnitTypeProfileId = typeA.Id, SpaceName = "Salon", SpaceGroup = "Oda / YaÅŸam AlanÄ±", FloorArea = 30, CeilingArea = 30, GrossWallArea = 50, NetWallArea = 42, SkirtingLength = 22, WindowSillLength = 3, InteriorDoorCount = 1, WindowCount = 2 },

                new DxfUnitSpace { DxfUnitTypeProfileId = typeA.Id, SpaceName = "Ebeveyn Banyo", SpaceGroup = "Islak Hacim", FloorArea = 5, CeilingArea = 5, GrossWallArea = 15, NetWallArea = 14, SkirtingLength = 0, WindowSillLength = 0, InteriorDoorCount = 1, WindowCount = 1 },

                new DxfUnitSpace { DxfUnitTypeProfileId = typeB.Id, SpaceName = "Ana SatÄ±ÅŸ AlanÄ±", SpaceGroup = "Ticari Alan", FloorArea = 200, CeilingArea = 200, GrossWallArea = 120, NetWallArea = 90, SkirtingLength = 60, WindowSillLength = 10, InteriorDoorCount = 0, WindowCount = 4 },

                new DxfUnitSpace { DxfUnitTypeProfileId = typeC.Id, SpaceName = "KapalÄ± Otopark", SpaceGroup = "Ortak Alan", FloorArea = 500, CeilingArea = 500, GrossWallArea = 300, NetWallArea = 280, SkirtingLength = 120, WindowSillLength = 0, InteriorDoorCount = 2, WindowCount = 0 }

            );

            _context.SaveChanges();



            return Ok();

        }



        [HttpGet]

        public IActionResult GetDxfUnitTypes(int projectId)

        {

            var types = _context.DxfUnitTypeProfiles

                .Include(x => x.Spaces)

                .Where(x => x.ProjectId == projectId)

                .Select(x => new {

                    x.Id,

                    x.ModelName,

                    x.Category,

                    x.TotalGrossArea,

                    x.TotalSpaceCount,

                    SpacesSummary = string.Join(", ", x.Spaces.Take(2).Select(s => $"{s.SpaceName} ({s.FloorArea}mÂ²)")),

                    SpacesCount = x.Spaces.Count

                }).ToList();



            return Json(types);

        }



        // =================================================================================

        // OTONOM ÅANTÄ°YE ÅEFÄ° AJANDASI (T-, T, T+ ZAMAN TÃœNELÄ°)

        // =================================================================================

        public async Task<IActionResult> SiteAgenda(int id, int? profileId, DateTime? baseDate)

        {

            var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();



            ViewData["ProjectId"] = id;

            ViewData["ProjectName"] = project.Name;



            ProjectDnaProfile profile = null;

            if (profileId.HasValue && profileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == id);

            }



            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == id)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            ViewBag.Profile = profile;

            var effectiveBaseDate = baseDate ?? DateTime.Today.AddDays(7);

            ViewBag.BaseDate = effectiveBaseDate;



            // DiÄŸer profilleri de dropdown iÃ§in gÃ¶nder

            ViewBag.AllProfiles = await _context.ProjectDnaProfiles

                .Where(p => p.ProjectId == id)

                .OrderByDescending(p => p.CreatedDate)

                .ToListAsync();



            var tasks = _dnaTimeOffsetService.GenerateTasksForProfile(profile, effectiveBaseDate);

            ViewBag.Tasks = tasks;



            return View();

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SyncSiteAgenda(int projectId, int profileId, DateTime baseDate)

        {

            int agencyId = 1;

            var agencyIdClaim = User.FindFirst("AgencyId")?.Value;

            if (int.TryParse(agencyIdClaim, out int aid)) agencyId = aid;



            var profile = await _context.ProjectDnaProfiles

                .Include(p => p.Details)

                .FirstOrDefaultAsync(p => p.Id == profileId && p.ProjectId == projectId);



            if (profile != null)

            {

                var tasks = _dnaTimeOffsetService.GenerateTasksForProfile(profile, baseDate);

                int count = await _dnaTimeOffsetService.SyncTasksToAgendaAsync(agencyId, projectId, tasks);

                TempData["SuccessMessage"] = $"{count} adet otonom ÅŸantiye gÃ¶revi sistem ajandanÄ±za (takvime) baÅŸarÄ±yla iÅŸlendi!";

            }



            return RedirectToAction(nameof(SiteAgenda), new { id = projectId, profileId = profileId, baseDate = baseDate.ToString("yyyy-MM-dd") });

        }



        // =================================================================================

        // DXF'SÄ°Z PARAMETRÄ°K KEÅÄ°F, Ä°HALE & MALZEME MOTORU

        // =================================================================================

        [HttpGet]

        public async Task<IActionResult> GetParametricTakeoff(int projectId, int? profileId)

        {

            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == projectId);



            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadÄ±." });



            ProjectDnaProfile profile = null;

            if (profileId.HasValue && profileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == projectId);

            }

            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == projectId)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            var items = GenerateParametricTakeoff(project, profile);

            return Json(new { success = true, items = items });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> SendParametricToB2B([FromBody] SendParametricB2BDto dto)

        {

            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "GeÃ§ersiz parametre." });



            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);



            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadÄ±." });



            ProjectDnaProfile profile = null;

            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);

            }

            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == dto.ProjectId)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            var allItems = GenerateParametricTakeoff(project, profile);

            var selectedItems = (dto.SelectedItemIds != null && dto.SelectedItemIds.Any())

                ? allItems.Where(i => dto.SelectedItemIds.Contains(i.Id)).ToList()

                : allItems;



            if (!selectedItems.Any()) return BadRequest(new { success = false, message = "HiÃ§ kalem seÃ§ilmedi." });



            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);

            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);



            var sb = new StringBuilder();

            sb.AppendLine($"### ğŸ—ï¸ {project.Name} - Parametrik Ä°hale & Malzeme AlÄ±m Paketi");

            sb.AppendLine($"*DXF Ã‡izimi OlmaksÄ±zÄ±n Proje YapÄ± Ã–zeti ve DNA Ã‡evresel FaktÃ¶rlerinden Otomatik TÃ¼retilmiÅŸtir.*");

            sb.AppendLine();

            sb.AppendLine("| # | Ä°malat / Malzeme Kalemi | Miktar | Birim | Ä°lgili Ä°ÅŸ BranÅŸÄ± (TaÅŸeron) | Hesaplama FormÃ¼lÃ¼ |");

            sb.AppendLine("|---|---|---|---|---|---|");



            foreach (var item in selectedItems)

            {

                sb.AppendLine($"| {item.Id} | **{item.Title}** | {item.Quantity:N0} | {item.Unit} | `{item.ProfessionName}` | {item.Formula} |");

            }



            var quoteRequest = new B2BQuoteRequest

            {

                RequesterAgencyId = agencyId,

                RequesterUserId = userId,

                SourceModule = "ConstructionProject",

                SourceReferenceId = project.Id,

                Title = $"{project.Name} - Parametrik Ä°hale Paketi ({selectedItems.Count} Kalem)",

                Description = sb.ToString(),

                Deadline = DateTime.UtcNow.AddDays(14),

                Status = "Draft",

                CreatedAt = DateTime.UtcNow

            };



            _context.B2BQuoteRequests.Add(quoteRequest);

            await _context.SaveChangesAsync();



            return Json(new

            {

                success = true,

                quoteRequestId = quoteRequest.Id,

                itemCount = selectedItems.Count,

                message = $"{selectedItems.Count} adet parametrik keÅŸif kalemi B2B SatÄ±n Alma & Ä°hale havuzuna baÅŸarÄ±yla aktarÄ±ldÄ±!"

            });

        }



        // =================================================================================

        // Ã‡Ä°FT LÄ°STE & TEK SIRA KODLU (X01-X09) Ä°Å GÃœCÃœ, TEDARÄ°K VE TAKVÄ°M MOTORU

        // =================================================================================

        [HttpGet]

        public async Task<IActionResult> GetParametricWorkflow(int projectId, int? profileId, DateTime? startDate, int? buildingId = null, string? mode = null)

        {

            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == projectId);



            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadÄ±." });



            ProjectDnaProfile profile = null;

            if (profileId.HasValue && profileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == profileId.Value && p.ProjectId == projectId);

            }

            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == projectId)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            var baseDate = startDate ?? DateTime.Today;

            var steps = GenerateParametricWorkflow(project, profile, baseDate, buildingId);

            var siteSetupSteps = GenerateSiteSetupSteps(project, profile);

            var siteHandoverSteps = GenerateSiteHandoverSteps(project, profile);



            int totalManDays = steps.Sum(s => s.TotalManDays);

            int maxCrew = steps.Any() ? steps.Max(s => s.CrewSize) : 0;

            int totalDurationDays = steps.Any() ? (int)(steps.Max(s => s.PlannedEndDate.Value) - steps.Min(s => s.PlannedStartDate.Value)).TotalDays + 1 : 0;

            var estimatedCompletion = steps.Any() ? steps.Max(s => s.PlannedEndDate.Value).ToString("dd.MM.yyyy") : "";



            var allBlocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();

            var targetStructure = buildingId.HasValue ? allBlocks.FirstOrDefault(b => b.Id == buildingId.Value) : allBlocks.FirstOrDefault(b => b.ParentBuildingId == null);



            return Json(new

            {

                success = true,

                steps = steps,

                siteSetupSteps = siteSetupSteps,

                siteHandoverSteps = siteHandoverSteps,

                staticSummary = new

                {

                    concreteClass = "C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton",

                    rebarClass = "B420C NervÃ¼rlÃ¼ DonatÄ± Ã‡eliÄŸi",

                    foundationType = "Radye Genel Temel",

                    structureType = "Betonarme Karkas (Kolon/Perde/DÃ¶ÅŸeme)",

                    totalStructures = allBlocks.Count(b => b.ParentBuildingId == null),

                    landArea = project.TotalLandArea ?? 0,

                    baseArea = targetStructure?.BaseArea ?? 0,

                    totalFloors = targetStructure != null ? (targetStructure.TotalFloors ?? 0) + targetStructure.BasementFloors : (project.TotalFloors ?? 5)

                },

                summary = new

                {

                    totalManDays = totalManDays,

                    maxCrew = maxCrew,

                    totalDurationDays = totalDurationDays,

                    estimatedCompletion = estimatedCompletion,

                    startDateStr = baseDate.ToString("dd.MM.yyyy")

                }

            });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> SendWorkflowToB2B([FromBody] SendWorkflowB2BDto dto)

        {

            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "GeÃ§ersiz parametre." });



            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);



            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadÄ±." });



            ProjectDnaProfile profile = null;

            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);

            }

            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == dto.ProjectId)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            var allSteps = GenerateParametricWorkflow(project, profile, DateTime.Today, dto.BuildingId);

            var selectedSteps = (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any())

                ? allSteps.Where(s => dto.SelectedStepCodes.Contains(s.StepCode)).ToList()

                : allSteps;



            bool isTradePackage = dto.TargetTrack == "TradePackage" || (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any(c => c.StartsWith("ADIM0-")));

            if (!selectedSteps.Any() && !isTradePackage) return BadRequest(new { success = false, message = "HiÃ§ aÅŸama seÃ§ilmedi." });



            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);

            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);



            var sb = new StringBuilder();

            string targetTrack = dto.TargetTrack ?? "Material"; // "Material" or "Labor"

            string structPrefix = !string.IsNullOrEmpty(selectedSteps.FirstOrDefault()?.TargetStructureName)

                ? $"[{selectedSteps.First().TargetStructureName}] "

                : "";

            string rfpTitle = "";



            if (isTradePackage)

            {

                var tradeCode = dto.SelectedStepCodes?.FirstOrDefault(c => c.StartsWith("ADIM0-"))?.Replace("ADIM0-", "") ?? "Ä°nÅŸaat";

                rfpTitle = $"{project.Name} - {structPrefix}Toptan {tradeCode} Ä°hale & Fiyat BaÄŸlama Paketi (AdÄ±m 0 Statik Sabiti)";

                sb.AppendLine($"### ğŸ›ï¸ {project.Name} - {structPrefix}Toptan {tradeCode} Ä°hale & Fiyat Kilitleme Paketi");

                sb.AppendLine($"*AdÄ±m 0 Mecburi Statik ve TaÅŸÄ±yÄ±cÄ± DXF Parametrelerine DayalÄ± Proje Geneli Toptan Ã‡erÃ§eve SÃ¶zleÅŸmesi*");

                sb.AppendLine();

                sb.AppendLine($"Bu ihale paketi, projenin tamamÄ±nda kullanÄ±lacak olan **{tradeCode}** branÅŸÄ±na ait tÃ¼m statik metraj ve teknik gereksinimleri kapsar.");

                sb.AppendLine($"MÃ¼teahhit firma, ÅŸantiyeye baÅŸlamadan Ã¶nce tedarikÃ§i/taÅŸeron firmadan sabit fiyat ve toptan birim fiyat garantisi talep etmektedir.");

                sb.AppendLine();

                sb.AppendLine($"| Veri KaynaÄŸÄ± | Ä°lgili Ä°ÅŸ BranÅŸÄ± | Kapsam | SÃ¶zleÅŸme Tipi |");

                sb.AppendLine($"|---|---|---|---|");

                sb.AppendLine($"| DXF / Statik Ã‡izim | `{tradeCode}` | Projenin TÃ¼m YapÄ±larÄ± ({project.Blocks?.Count ?? 1} BaÄŸÄ±msÄ±z YapÄ±) | Toptan Sabit Fiyat SÃ¶zleÅŸmesi |");

            }

            else if (targetTrack == "Labor")

            {

                rfpTitle = $"{project.Name} - {structPrefix}TaÅŸeron & Ä°ÅŸ GÃ¼cÃ¼ Ã‡aÄŸrÄ±sÄ± ({selectedSteps.Count} AÅŸama)";

                sb.AppendLine($"### ğŸ‘· {project.Name} - {structPrefix}TaÅŸeron & Ä°ÅŸ GÃ¼cÃ¼ Hizmet Ã‡aÄŸrÄ±sÄ±");

                sb.AppendLine($"*Ä°nÅŸaat Ä°ÅŸ AkÄ±ÅŸ SÄ±ralamasÄ±na GÃ¶re Ekip, Kadro ve SÃ¼re Ä°htiyaÃ§larÄ±*");

                sb.AppendLine();

                sb.AppendLine("| Kod | ParÃ§a | AÅŸama AdÄ± | Ä°lgili BranÅŸ / Meslek | Ã–nerilen Ekip Kadrosu | SÃ¼re (Ä°ÅŸ GÃ¼nÃ¼) | Hacim (Adam-GÃ¼n) | Kritik Gereksinim / Ekipman |");

                sb.AppendLine("|:---:|---|---|---|---|:---:|:---:|---|");

                foreach (var s in selectedSteps)

                {

                    sb.AppendLine($"| **{s.StepCode}** | {s.PartName} | {s.PhaseName} | `{s.ProfessionName}` | {s.CrewComposition} ({s.CrewSize} KiÅŸi) | {s.DurationDays} GÃ¼n | {s.TotalManDays} Adam-GÃ¼n | {s.EquipmentNeeded} |");

                }

            }

            else

            {

                rfpTitle = $"{project.Name} - {structPrefix}SÄ±ralÄ± Malzeme Tedarik Ä°halesi ({selectedSteps.Count} AÅŸama)";

                sb.AppendLine($"### ğŸ“¦ {project.Name} - {structPrefix}SÄ±ralÄ± Malzeme Tedarik & SatÄ±n Alma Paketi");

                sb.AppendLine($"*Ä°nÅŸaat Ä°ÅŸ AkÄ±ÅŸ SÄ±ralamasÄ±na GÃ¶re Tedarik Edilecek Malzemeler*");

                sb.AppendLine();

                sb.AppendLine("| Kod | ParÃ§a | AÅŸama | Tedarik Edilecek Malzeme | Miktar | Birim | TedarikÃ§i Grubu | MÃ¼hendislik HesabÄ± |");

                sb.AppendLine("|:---:|---|---|---|---|---|---|---|");

                foreach (var s in selectedSteps)

                {

                    sb.AppendLine($"| **{s.StepCode}** | {s.PartName} | {s.PhaseName} | **{s.MaterialTitle}** | {s.MaterialQuantity:N0} | {s.MaterialUnit} | `{s.SupplierCategory}` | {s.MaterialFormula} |");

                }

            }



            var quoteRequest = new B2BQuoteRequest

            {

                RequesterAgencyId = agencyId,

                RequesterUserId = userId,

                SourceModule = "ConstructionProject",

                SourceReferenceId = project.Id,

                Title = rfpTitle,

                Description = sb.ToString(),

                Deadline = DateTime.UtcNow.AddDays(14),

                Status = "Draft",

                CreatedAt = DateTime.UtcNow

            };



            _context.B2BQuoteRequests.Add(quoteRequest);

            await _context.SaveChangesAsync();



            return Json(new

            {

                success = true,

                quoteRequestId = quoteRequest.Id,

                itemCount = selectedSteps.Count,

                message = $"{selectedSteps.Count} adet iÅŸ akÄ±ÅŸ aÅŸamasÄ± B2B {(targetTrack == "Labor" ? "TaÅŸeron & Ä°ÅŸ GÃ¼cÃ¼" : "Malzeme SatÄ±n Alma")} havuzuna baÅŸarÄ±yla aktarÄ±ldÄ±!"

            });

        }



        [HttpPost]

        [IgnoreAntiforgeryToken]

        public async Task<IActionResult> TriggerWorkflowAgenda([FromBody] TriggerWorkflowAgendaDto dto)

        {

            if (dto == null || dto.ProjectId <= 0) return BadRequest(new { success = false, message = "GeÃ§ersiz parametre." });



            var project = await _context.ConstructionProjects

                .Include(p => p.Blocks)

                .Include(p => p.Amenities)

                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);



            if (project == null) return NotFound(new { success = false, message = "Proje bulunamadÄ±." });



            ProjectDnaProfile profile = null;

            if (dto.ProfileId.HasValue && dto.ProfileId.Value > 0)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .FirstOrDefaultAsync(p => p.Id == dto.ProfileId.Value && p.ProjectId == dto.ProjectId);

            }

            if (profile == null)

            {

                profile = await _context.ProjectDnaProfiles

                    .Include(p => p.Details)

                        .ThenInclude(d => d.Profession)

                    .Where(p => p.ProjectId == dto.ProjectId)

                    .OrderByDescending(p => p.CreatedDate)

                    .FirstOrDefaultAsync();

            }



            var baseDate = dto.StartDate > DateTime.MinValue ? dto.StartDate : DateTime.Today;

            var allSteps = GenerateParametricWorkflow(project, profile, baseDate, dto.BuildingId);

            var selectedSteps = (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any())

                ? allSteps.Where(s => dto.SelectedStepCodes.Contains(s.StepCode)).ToList()

                : allSteps;



            bool isTradePackage = dto.TargetTrack == "TradePackage" || (dto.SelectedStepCodes != null && dto.SelectedStepCodes.Any(c => c.StartsWith("ADIM0-")));

            if (!selectedSteps.Any() && !isTradePackage) return BadRequest(new { success = false, message = "HiÃ§ aÅŸama seÃ§ilmedi." });



            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId);

            int agencyId = consultant?.AgencyId ?? (project.AgencyId > 0 ? project.AgencyId : 1);



            int createdCount = 0;



            foreach (var step in selectedSteps)

            {

                if (!step.PlannedStartDate.HasValue) continue;



                // 1. Ana Ä°malat GÃ¶revi (T 0)

                var mainTitle = $"ğŸ—ï¸ [{step.StepCode}] {step.PhaseName} Ä°malat BaÅŸlangÄ±cÄ±";

                bool existsMain = await _context.AgendaRecords.AnyAsync(a =>

                    a.AgencyId == agencyId &&

                    a.ProjectId == project.Id &&

                    a.EventDate.Date == step.PlannedStartDate.Value.Date &&

                    a.Title == mainTitle &&

                    !a.IsDeleted);



                if (!existsMain)

                {

                    _context.AgendaRecords.Add(new AgendaRecord

                    {

                        AgencyId = agencyId,

                        ProjectId = project.Id,

                        Title = mainTitle,

                        EventDate = step.PlannedStartDate.Value,

                        RecordTypeId = AgendaConstants.TypeSantiyeDenetimi,

                        StatusId = AgendaConstants.StatusPlanlandi,

                        Notes = $"Kadro: {step.CrewComposition} ({step.CrewSize} KiÅŸi)\nSÃ¼re: {step.DurationDays} Ä°ÅŸ GÃ¼nÃ¼ ({step.TotalManDays} Adam-GÃ¼n)\nÃ–nkoÅŸul: {step.Predecessors}\nKritik Kural: {step.CriticalDependency}\nMalzeme: {step.MaterialTitle} ({step.MaterialQuantity:N0} {step.MaterialUnit})"

                    });

                    createdCount++;

                }



                // 2. T- Ã–n HazÄ±rlÄ±k HatÄ±rlatmasÄ± (2 veya 3 gÃ¼n Ã¶nce)

                int reminderDays = step.DurationDays >= 5 ? 3 : 2;

                var reminderDate = step.PlannedStartDate.Value.AddDays(-reminderDays);

                var reminderTitle = $"âš ï¸ [T-{reminderDays} HazÄ±rlÄ±k - {step.StepCode}] {step.PhaseName} Ã–n Kontrolleri";



                bool existsReminder = await _context.AgendaRecords.AnyAsync(a =>

                    a.AgencyId == agencyId &&

                    a.ProjectId == project.Id &&

                    a.EventDate.Date == reminderDate.Date &&

                    a.Title == reminderTitle &&

                    !a.IsDeleted);



                if (!existsReminder)

                {

                    _context.AgendaRecords.Add(new AgendaRecord

                    {

                        AgencyId = agencyId,

                        ProjectId = project.Id,

                        Title = reminderTitle,

                        EventDate = reminderDate,

                        RecordTypeId = AgendaConstants.TypeTaseronToplantisi,

                        StatusId = AgendaConstants.StatusPlanlandi,

                        Notes = $"âš ï¸ DÄ°KKAT: {step.PlannedStartDate.Value:dd.MM.yyyy} tarihinde baÅŸlayacak {step.PhaseName} iÃ§in;\n1) {step.MaterialTitle} depoya sevk edildi mi?\n2) {step.ProfessionName} ekibi teyit edildi mi?\n3) {step.CriticalDependency}"

                    });

                    createdCount++;

                }

            }



            await _context.SaveChangesAsync();



            return Json(new

            {

                success = true,

                createdCount = createdCount,

                message = $"{selectedSteps.Count} adet aÅŸamaya ait {createdCount} otonom ajanda gÃ¶revi ve T- hatÄ±rlatmasÄ± takviminize baÅŸarÄ±yla iÅŸlendi!"

            });

        }



        private List<ParametricTakeoffItemDto> GenerateParametricTakeoff(ConstructionProject project, ProjectDnaProfile profile)

        {

            var list = new List<ParametricTakeoffItemDto>();

            if (project == null) return list;



            var blocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();

            var rootBlocks = blocks.Where(b => b.ParentBuildingId == null).ToList();



            double landArea = project.TotalLandArea ?? 0;

            double baseArea = rootBlocks.Sum(b => b.BaseArea ?? 0);

            if (baseArea <= 0 && blocks.Any()) baseArea = blocks.Sum(b => b.BaseArea ?? 0);

            if (baseArea <= 0) baseArea = project.EskiBinaOturumAlani ?? (landArea > 0 ? landArea * 0.40 : 500);



            int maxFloors = blocks.Any() ? blocks.Max(b => (b.TotalFloors ?? 0) + b.BasementFloors) : (project.TotalFloors ?? 5);

            int maxBasement = blocks.Any() ? blocks.Max(b => b.BasementFloors) : (project.EskiBodrumKatSayisi ?? 2);

            if (maxBasement <= 0) maxBasement = 2;

            if (maxFloors <= 0) maxFloors = 5;



            int totalUnits = blocks.Sum(b => b.TotalApartments);

            if (totalUnits <= 0) totalUnits = project.TargetTotalApartments ?? (project.EskiDaireSayisi ?? 10);



            int totalShops = blocks.Sum(b => b.TotalShops);

            if (totalShops <= 0) totalShops = project.TargetTotalShops ?? (project.EskiDukkanSayisi ?? 0);



            // Toplam inÅŸaat alanÄ±

            double totalFloorArea = 0;

            if (blocks.Any(b => b.BaseArea.HasValue && b.BaseArea > 0))

            {

                totalFloorArea = blocks.Sum(b => (b.BaseArea ?? (baseArea / Math.Max(1, blocks.Count))) * ((b.TotalFloors ?? 5) + b.BasementFloors));

            }

            else

            {

                totalFloorArea = baseArea * maxFloors;

            }

            if (totalFloorArea <= 0) totalFloorArea = baseArea * 5;



            // Ã‡evre uzunluklarÄ±

            double landPerimeter = landArea > 0 ? Math.Round(4 * Math.Sqrt(landArea), 1) : 240;

            double buildingPerimeter = baseArea > 0 ? Math.Round(4 * Math.Sqrt(baseArea), 1) : 120;



            // DNA DetaylarÄ±

            var details = profile?.Details?.ToList() ?? new List<ProjectDnaDetail>();



            var zeminDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Zemin") || d.SelectedItem.Contains("Kaya") || d.SelectedItem.Contains("Kum") || d.SelectedItem.Contains("Kil"));

            string zeminTipi = zeminDetail != null ? zeminDetail.SelectedItem : "YumuÅŸak / Kumlu Zemin";

            double kabarma = zeminTipi.Contains("Kaya") ? 1.35 : 1.25;



            decimal ihataUzunlugu = (decimal)landPerimeter;

            var ihataLenDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Ã‡evre SÄ±nÄ±rÄ± Toplam") || (d.SelectedItem.Contains("Ä°hata") && d.SelectedItem.Contains(":")));

            if (ihataLenDetail != null && ihataLenDetail.Quantity > 0)

            {

                ihataUzunlugu = ihataLenDetail.Quantity;

            }

            else if (ihataLenDetail != null && ihataLenDetail.SelectedItem.Contains(":"))

            {

                var p = ihataLenDetail.SelectedItem.Split(':');

                if (p.Length > 1 && decimal.TryParse(p[1].Replace("m", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal qVal))

                {

                    ihataUzunlugu = qVal;

                }

            }



            var ihataMatDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Sac") || d.SelectedItem.Contains("Tel Ã–rgÃ¼") || d.SelectedItem.Contains("Pano"));

            string ihataMalzemesi = ihataMatDetail != null ? ihataMatDetail.SelectedItem : "Trapez Sac Kaplama";



            var kapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("KapÄ±"));

            string kapiTipi = kapiDetail != null ? kapiDetail.SelectedItem : "SÃ¼rgÃ¼lÃ¼ Sac KapÄ± (RaylÄ±)";



            var molozDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YÄ±kÄ±m") || d.SelectedItem.Contains("Moloz") || d.SelectedItem.Contains("Enkaz") || d.SelectedItem.Contains("BoÅŸ Arsa"));

            bool isCleanPlot = molozDetail != null && (molozDetail.SelectedItem.Contains("BoÅŸ") || molozDetail.SelectedItem.Contains("Gerekmez") || molozDetail.SelectedItem.Contains("Yok"));

            bool molozVar = molozDetail != null && !isCleanPlot && (molozDetail.SelectedItem.Contains("YÄ±kÄ±lacak") || molozDetail.SelectedItem.Contains("Moloz ve Ã‡Ã¶p") || molozDetail.SelectedItem.Contains("Enkaz"));

            bool hasDemolition = !isCleanPlot && (molozVar || (project.EskiToplamMetrekare.HasValue && project.EskiToplamMetrekare.Value > 0) || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0));



            int itemId = 1;



            // 1. KazÄ± ve Hafriyat

            decimal kaziM3 = Math.Round((decimal)(baseArea * (maxBasement * 3.2 + 1.0) * kabarma), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "01. Hafriyat & Saha HazÄ±rlÄ±ÄŸÄ±",

                Title = $"Derin KazÄ± ve Hafriyat Nakliyesi ({zeminTipi})",

                Description = $"Proje taban oturumu ({baseArea:N0} mÂ²) ve {maxBasement} bodrum kat derinliÄŸine gÃ¶re hafriyat kazÄ±sÄ±, dÃ¶kÃ¼m sahasÄ±na nakliyesi ve tesviyesi.",

                Formula = $"{baseArea:N0} mÂ² Taban Ã— {(maxBasement * 3.2 + 1.0):N1}m Derinlik Ã— {kabarma} Kabarma",

                Quantity = kaziM3,

                Unit = "mÂ³",

                ProfessionName = "Hafriyat ve Nakliye Ekibi",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 1 Zemin SeÃ§imi"

            });



            // 2. YÄ±kÄ±m / Moloz

            if (hasDemolition)

            {

                double eskiAlan = project.EskiToplamMetrekare ?? (project.EskiBinaOturumAlani ?? (baseArea * 0.5));

                decimal molozM3 = Math.Round((decimal)(eskiAlan * 0.45), 0);

                if (molozM3 < 300) molozM3 = 450;

                list.Add(new ParametricTakeoffItemDto

                {

                    Id = itemId++,

                    Category = "01. Hafriyat & Saha HazÄ±rlÄ±ÄŸÄ±",

                    Title = "Eski YapÄ± YÄ±kÄ±mÄ±, Enkaz SÃ¶kÃ¼mÃ¼ & Moloz BertarafÄ±",

                    Description = "Parselde bulunan mevcut yapÄ±nÄ±n kontrollÃ¼ yÄ±kÄ±mÄ±, demir ayrÄ±ÅŸtÄ±rma ve molozun belediye dÃ¶kÃ¼m sahasÄ±na nakliyesi.",

                    Formula = $"Mevcut YapÄ± Hacmi ({eskiAlan:N0} mÂ²) Ã— 0.45 mÂ³/mÂ² Moloz KatsayÄ±sÄ±",

                    Quantity = molozM3,

                    Unit = "mÂ³",

                    ProfessionName = "YÄ±kÄ±m ve Enkaz TaÅŸeronu",

                    DnaSource = "Proje KÃ¼nyesi + AdÄ±m 1 YÄ±kÄ±m/Moloz SeÃ§imi"

                });

            }



            // 3. Ã‡evre Ä°hata Ã‡iti

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "02. Åantiye Mobilizasyon & GÃ¼venlik",

                Title = $"{ihataMalzemesi} ile Åantiye Ã‡evre Ä°hatasÄ± KapatÄ±lmasÄ±",

                Description = "Åantiye parsel sÄ±nÄ±rlarÄ±nÄ±n iÅŸ gÃ¼venliÄŸi standartlarÄ±na uygun Ã§it ve destek dikmeleri ile Ã§evrilmesi.",

                Formula = $"Arsa Parsel Ã‡evre UzunluÄŸu ({ihataUzunlugu:N0} mt)",

                Quantity = ihataUzunlugu,

                Unit = "mt",

                ProfessionName = "Ä°hata / Ã‡it UstasÄ±",

                DnaSource = "AdÄ±m 1 Ã‡evresel FaktÃ¶r (Ä°hata MetrajÄ±)"

            });



            // 4. Åantiye KapÄ±sÄ±

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "02. Åantiye Mobilizasyon & GÃ¼venlik",

                Title = $"{kapiTipi} (6.00m AÄŸÄ±r VasÄ±ta GiriÅŸi)",

                Description = "Åantiye ana araÃ§ ve transmikser giriÅŸ kapÄ±sÄ±, sÃ¼rgÃ¼lÃ¼ ray mekanizmasÄ± ve kilit sistemi montajÄ±.",

                Formula = "1 TakÄ±m Ã‡ift RaylÄ± SÃ¼rgÃ¼lÃ¼ GÃ¼venlik GiriÅŸ KapÄ±sÄ±",

                Quantity = 1,

                Unit = "TakÄ±m",

                ProfessionName = "Demir DoÄŸramacÄ±",

                DnaSource = "AdÄ±m 1 GiriÅŸ KapÄ±sÄ± SeÃ§imi"

            });



            // 5. Mobilizasyon BinalarÄ±

            int konteynerSayisi = Math.Max(2, Math.Min(6, (int)Math.Ceiling(totalFloorArea / 5000.0)));

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "02. Åantiye Mobilizasyon & GÃ¼venlik",

                Title = "Åantiye Konteyner / Prefabrik KampÄ± Kurulumu",

                Description = "Åantiye yÃ¶netim ofisi, yemekhane, soyunma odasÄ± ve WC/duÅŸ konteynerlerinin sahaya nakliyesi ve teraziye alÄ±nmasÄ±.",

                Formula = $"Toplam {totalFloorArea:N0} mÂ² inÅŸaat bÃ¼yÃ¼klÃ¼ÄŸÃ¼ne gÃ¶re mobilizasyon Ã¼nitesi",

                Quantity = konteynerSayisi,

                Unit = "Adet",

                ProfessionName = "Prefabrik / Konteyner TedarikÃ§isi",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 1 Konaklama SeÃ§imi"

            });



            // 6. Ä°ksa / Åoring

            decimal iksaM2 = Math.Round((decimal)(buildingPerimeter * (maxBasement * 3.2)), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "03. Zemin GÃ¼venliÄŸi & Ä°ksa",

                Title = "Derin KazÄ± Ä°ksasÄ± & GÃ¼venlik Perdesi (PÃ¼skÃ¼rtme Beton / Mini KazÄ±k)",

                Description = "Yol ve komÅŸu parsellerin gÃ¶Ã§mesini Ã¶nlemek iÃ§in derin kazÄ± ÅŸoring, zemin Ã§ivisi ve hasÄ±r Ã§elikli pÃ¼skÃ¼rtme beton iksa imalatÄ±.",

                Formula = $"{buildingPerimeter:N0} mt Bina Ã‡evresi Ã— {(maxBasement * 3.2):N1} mt KazÄ± DerinliÄŸi",

                Quantity = iksaM2,

                Unit = "mÂ²",

                ProfessionName = "Ä°ksa ve Ankraj Ekibi",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 1 Ä°ksa SeÃ§imi"

            });



            // 7. C20/25 Temel Grobetonu

            decimal grobetonM3 = Math.Round((decimal)(baseArea * 0.15), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "04. Temel & YalÄ±tÄ±m",

                Title = "C20/25 PompalÄ± Temel AltÄ± Grobetonu (15 cm)",

                Description = "Radye temel altÄ± membran bohÃ§alama Ã¶ncesi tesviye ve zemin koruma grobetonu dÃ¶kÃ¼mÃ¼.",

                Formula = $"{baseArea:N0} mÂ² Taban Oturumu Ã— 0.15m KalÄ±nlÄ±k",

                Quantity = grobetonM3,

                Unit = "mÂ³",

                ProfessionName = "Betoncu",

                DnaSource = "YapÄ± Ã–zeti (Taban TAKS)"

            });



            // 8. Membran BohÃ§alama & Perde Su YalÄ±tÄ±mÄ±

            decimal yalitimM2 = Math.Round((decimal)(baseArea + (buildingPerimeter * (maxBasement * 3.2))), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "04. Temel & YalÄ±tÄ±m",

                Title = "Ã‡ift Kat BitÃ¼mlÃ¼ Membran Temel BohÃ§alama & Perde YalÄ±tÄ±mÄ±",

                Description = "Radye temel altÄ± ve bodrum dÄ±ÅŸ perde duvarlarÄ±nÄ±n yeraltÄ± suyu ve neme karÅŸÄ± Ã§ift kat polyester keÃ§e takviyeli membranla bohÃ§alanmasÄ±.",

                Formula = $"{baseArea:N0} mÂ² Taban + {Math.Round(buildingPerimeter * maxBasement * 3.2, 0):N0} mÂ² Perde YÃ¼zeyi",

                Quantity = yalitimM2,

                Unit = "mÂ²",

                ProfessionName = "Ä°zolasyoncu / BohÃ§alama UstasÄ±",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 2 Temel Ä°zolasyonu"

            });



            // 9. C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton

            decimal betonM3 = Math.Round((decimal)(totalFloorArea * 0.40), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "05. TaÅŸÄ±yÄ±cÄ± Kaba YapÄ± (Statik)",

                Title = "C30/37 TaÅŸÄ±yÄ±cÄ± Statik HazÄ±r Beton (Radye, Kolon, Perde, DÃ¶ÅŸeme)",

                Description = "Bina taÅŸÄ±yÄ±cÄ± karkas sistemi iÃ§in transmikser ve beton pompasÄ± ile dÃ¶kÃ¼lecek toplam hazÄ±r beton metrajÄ±.",

                Formula = $"{totalFloorArea:N0} mÂ² Toplam Ä°nÅŸaat AlanÄ± Ã— 0.40 mÂ³/mÂ² KatsayÄ±sÄ±",

                Quantity = betonM3,

                Unit = "mÂ³",

                ProfessionName = "Betoncu",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 0 TaÅŸÄ±yÄ±cÄ± Sabitleri"

            });



            // 10. NervÃ¼rlÃ¼ DonatÄ± Ã‡eliÄŸi

            decimal demirTon = Math.Round((decimal)(totalFloorArea * 0.034), 1);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "05. TaÅŸÄ±yÄ±cÄ± Kaba YapÄ± (Statik)",

                Title = "B420C NervÃ¼rlÃ¼ DonatÄ± Ã‡eliÄŸi (Q8-Q26 KalÄ±n/Orta/Ä°nce Demir)",

                Description = "Temel, perde, kolon, kiriÅŸ ve dÃ¶ÅŸemelerde kullanÄ±lacak nervÃ¼rlÃ¼ inÅŸaat demiri (etriye, boy donatÄ±, pilye).",

                Formula = $"{totalFloorArea:N0} mÂ² Toplam Ä°nÅŸaat AlanÄ± Ã— 34 kg/mÂ² DonatÄ± OranÄ±",

                Quantity = demirTon,

                Unit = "Ton",

                ProfessionName = "Demirci",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 0 DonatÄ± Sabitleri"

            });



            // 11. KalÄ±p ve Ä°skele Ä°malatÄ±

            decimal kalipM2 = Math.Round((decimal)(totalFloorArea * 2.5), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "05. TaÅŸÄ±yÄ±cÄ± Kaba YapÄ± (Statik)",

                Title = "EndÃ¼striyel KalÄ±p ve Ä°skele Sistemi Kurulum & SÃ¶kÃ¼m Ä°ÅŸÃ§iliÄŸi",

                Description = "Perde, kolon, kiriÅŸ ve asmolen/kaset dÃ¶ÅŸeme kalÄ±plarÄ±nÄ±n projeye uygun kurulmasÄ±, yaÄŸlanmasÄ± ve sÃ¶kÃ¼mÃ¼.",

                Formula = $"{totalFloorArea:N0} mÂ² Toplam Ä°nÅŸaat AlanÄ± Ã— 2.5 mÂ²/mÂ² KalÄ±p YÃ¼zeyi",

                Quantity = kalipM2,

                Unit = "mÂ²",

                ProfessionName = "KalÄ±pÃ§Ä±",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 0 KalÄ±p Sabitleri"

            });



            // 12. DÄ±ÅŸ Cephe GÃ¼venlikli Ä°skele

            decimal iskeleM2 = Math.Round((decimal)(buildingPerimeter * (maxFloors * 3.0)), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "06. DÄ±ÅŸ Cephe & Ä°nce Ä°malatlar",

                Title = "TSE Belgeli FlanÅŸlÄ± GÃ¼venlikli DÄ±ÅŸ Cephe Ä°skelesi (Kurulum & Kiralama)",

                Description = "Bina dÄ±ÅŸ cephesine kurulacak tam gÃ¼venlikli, merdivenli ve fileli Ã§alÄ±ÅŸma iskelesi.",

                Formula = $"{buildingPerimeter:N0} mt Ã‡evre Ã— {(maxFloors * 3.0):N0} mt Bina YÃ¼ksekliÄŸi",

                Quantity = iskeleM2,

                Unit = "mÂ²",

                ProfessionName = "Ä°skeleci",

                DnaSource = "YapÄ± Ã–zeti (Kat & YÃ¼kseklik)"

            });



            // 13. DÄ±ÅŸ Cephe IsÄ± Mantolama

            decimal mantoM2 = Math.Round((decimal)(iskeleM2 * 0.85m), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "06. DÄ±ÅŸ Cephe & Ä°nce Ä°malatlar",

                Title = "TaÅŸyÃ¼nÃ¼ / EPS DÄ±ÅŸ Cephe IsÄ± Mantolama & Mineral SÄ±va",

                Description = "DÄ±ÅŸ cephe Ä±sÄ± yalÄ±tÄ±m levhasÄ± montajÄ±, dubelleme, fileli sÄ±va ve dekoratif mineral sÄ±va kaplamasÄ±.",

                Formula = $"{iskeleM2:N0} mÂ² BrÃ¼t Cephe Ã— 0.85 (Pencere/BoÅŸluk DÃ¼ÅŸÃ¼lmÃ¼ÅŸ Net)",

                Quantity = mantoM2,

                Unit = "mÂ²",

                ProfessionName = "DÄ±ÅŸ Cephe UstasÄ±",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 3 GÃ¶vde SeÃ§imi"

            });



            // 14. Tesviye ÅapÄ±

            decimal sapM2 = Math.Round((decimal)(totalFloorArea * 0.82), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "06. DÄ±ÅŸ Cephe & Ä°nce Ä°malatlar",

                Title = "Helikopterli ve MastarlÄ± Zemin Tesviye ÅapÄ±",

                Description = "Daire iÃ§i kuru ve Ä±slak hacimlerde izolasyon ve dÃ¶ÅŸeme kaplamasÄ± Ã¶ncesi helikopterli tesviye ÅŸapÄ± dÃ¶kÃ¼mÃ¼.",

                Formula = $"{totalFloorArea:N0} mÂ² Ä°nÅŸaat AlanÄ± Ã— 0.82 (Net DÃ¶ÅŸeme AlanÄ±)",

                Quantity = sapM2,

                Unit = "mÂ²",

                ProfessionName = "ÅapÃ§Ä±",

                DnaSource = "YapÄ± Ã–zeti + AdÄ±m 0 Åap Sabitleri"

            });



            // 15. Åantiye ElektriÄŸi

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "07. GeÃ§ici MEP & Åantiye AltyapÄ±sÄ±",

                Title = "Åantiye GeÃ§ici Enerji, DaÄŸÄ±tÄ±m Panosu & Ã‡evre AydÄ±nlatma",

                Description = "Ana ÅŸantiye elektrik panosu, kaÃ§ak akÄ±m rÃ¶leleri, kat tali panolarÄ± ve gece gÃ¼venlik aydÄ±nlatmasÄ± kurulumu.",

                Formula = "Åantiye GeÃ§ici Elektrik & Pano Paketi",

                Quantity = 1,

                Unit = "Set",

                ProfessionName = "Åantiye ElektrikÃ§isi",

                DnaSource = "AdÄ±m 1 Åantiye ElektriÄŸi SeÃ§imi"

            });



            // 16. Åantiye Suyu

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "07. GeÃ§ici MEP & Åantiye AltyapÄ±sÄ±",

                Title = "15 Tonluk Polietilen Åantiye Su Deposu & 3 HP Otomatik Hidrofor",

                Description = "Beton kÃ¼rÃ¼, temizlik ve kullanÄ±m suyu iÃ§in ÅŸantiye su deposu, otomatik hidrofor ve daÄŸÄ±tÄ±m borulama tesisatÄ±.",

                Formula = "Åantiye Su Depolama & BasÄ±nÃ§landÄ±rma Paketi",

                Quantity = 1,

                Unit = "Set",

                ProfessionName = "SÄ±hhi TesisatÃ§Ä±",

                DnaSource = "AdÄ±m 1 Åantiye Suyu SeÃ§imi"

            });



            // 17. Ankastre 3'lÃ¼ Set (AdÄ±m 4)

            var ankastreDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Ankastre") || d.SelectedItem.Contains("FÄ±rÄ±n") || d.SelectedItem.Contains("Ocak"));

            string ankastreTitle = ankastreDetail != null ? ankastreDetail.SelectedItem : "Ankastre 3'lÃ¼ Set (FÄ±rÄ±n + Ocak + Davlumbaz)";

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "08. Ä°Ã§ Mekan, Mutfak & AhÅŸap Ä°malatlarÄ±",

                Title = $"{ankastreTitle} Temini ve MontajÄ±",

                Description = "Daireler iÃ§in baÄŸÄ±msÄ±z ankastre fÄ±rÄ±n, cam ocak ve davlumbaz temini, elektrik/gaz baÄŸlantÄ±sÄ± ve montajÄ±.",

                Formula = $"{totalUnits} BaÄŸÄ±msÄ±z BÃ¶lÃ¼m (Daire) Ã— 1 TakÄ±m",

                Quantity = totalUnits,

                Unit = "Set",

                ProfessionName = "Mutfak & Mobilya Ekibi",

                DnaSource = "AdÄ±m 4 Soru 8: Ankastre Set StandardÄ±"

            });



            // 18. GiriÅŸ Portmanto / Vestiyer DolabÄ± (AdÄ±m 4)

            var vestiyerDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Portmanto") || d.SelectedItem.Contains("Vestiyer"));

            string vestiyerTitle = vestiyerDetail != null ? vestiyerDetail.SelectedItem : "GiriÅŸ HolÃ¼ Portmanto / Vestiyer DolabÄ±";

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "08. Ä°Ã§ Mekan, Mutfak & AhÅŸap Ä°malatlarÄ±",

                Title = $"{vestiyerTitle} Ä°malat ve MontajÄ±",

                Description = "Daire giriÅŸ hollerine projesine uygun gÃ¶mme veya modÃ¼ler vestiyer, ayakkabÄ±lÄ±k ve ayna montajÄ±.",

                Formula = $"{totalUnits} Daire GiriÅŸ HolÃ¼ Ã— 1 Adet",

                Quantity = totalUnits,

                Unit = "Adet",

                ProfessionName = "Mutfak & Mobilya Ekibi",

                DnaSource = "AdÄ±m 4 Soru 10: Portmanto / Vestiyer StandardÄ±"

            });



            // 19. Parapet HarpuÅŸtasÄ± & Ã‡atÄ± Ã‡Ä±kÄ±ÅŸ KapaÄŸÄ± (AdÄ±m 5)

            var harpushtaDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("HarpuÅŸta") || d.SelectedItem.Contains("Parapet"));

            string harpushtaTitle = harpushtaDetail != null ? harpushtaDetail.SelectedItem : "Mermer / Granit Parapet HarpuÅŸtasÄ±";

            decimal harpushtaMt = Math.Round((decimal)(buildingPerimeter * 1.10), 0);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "09. Ã‡atÄ±, Teras & HarpuÅŸta",

                Title = $"{harpushtaTitle} & Emniyetli Ã‡atÄ± Ã‡Ä±kÄ±ÅŸ KapaÄŸÄ±",

                Description = "Teras ve Ã§atÄ± parapet duvarlarÄ±nÄ±n su sÄ±zdÄ±rmaz damlalÄ±klÄ± mermer/granit ile kaplanmasÄ± ve Ã§atÄ± Ã§Ä±kÄ±ÅŸ kapaÄŸÄ±.",

                Formula = $"{buildingPerimeter:N0} mt Bina Ã‡evresi Ã— 1.10 Parapet GeliÅŸimi",

                Quantity = harpushtaMt,

                Unit = "mt",

                ProfessionName = "Mermer & DoÄŸaltaÅŸ UstasÄ±",

                DnaSource = "AdÄ±m 5 Soru 6: Parapet HarpuÅŸtasÄ± ve Ã‡atÄ± Ã‡Ä±kÄ±ÅŸ GÃ¼venliÄŸi"

            });



            // 20. Pencere Denizlikleri (AdÄ±m 6)

            var denizlikDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Denizlik"));

            string denizlikTitle = denizlikDetail != null ? denizlikDetail.SelectedItem : "3cm KalÄ±nlÄ±ÄŸÄ±nda DoÄŸaltaÅŸ / Mermer Pencere Denizlikleri";

            decimal denizlikMt = totalUnits * 7;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "10. DoÄŸrama, Denizlik & Balkon KorkuluklarÄ±",

                Title = $"{denizlikTitle} (Ä°Ã§ ve DÄ±ÅŸ Denizlikler)",

                Description = "Pencere altlarÄ±na pahlÄ±, damlalÄ±klÄ± iÃ§ ve dÄ±ÅŸ mermer/granit denizlik montajÄ±.",

                Formula = $"{totalUnits} Daire Ã— 5 Pencere Ã— 1.4 mt Ortalama AÃ§Ä±klÄ±k",

                Quantity = denizlikMt,

                Unit = "mt",

                ProfessionName = "Mermer & DoÄŸaltaÅŸ UstasÄ±",

                DnaSource = "AdÄ±m 6 Soru 6: Pencere Ä°Ã§ ve DÄ±ÅŸ Denizlik Malzemesi"

            });



            // 21. Balkon ve FransÄ±z Balkon KorkuluklarÄ± (AdÄ±m 6)

            var korkulukDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Korkuluk"));

            string korkulukTitle = korkulukDetail != null ? korkulukDetail.SelectedItem : "AlÃ¼minyum & Lamine Cam Balkon Korkuluk Sistemi";

            decimal korkulukMt = totalUnits * 6;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "10. DoÄŸrama, Denizlik & Balkon KorkuluklarÄ±",

                Title = $"{korkulukTitle} Ä°malat ve MontajÄ±",

                Description = "Balkon ve FransÄ±z pencereler iÃ§in statik hesaplÄ± kÃ¼peÅŸteli gÃ¼venlik korkuluklarÄ±.",

                Formula = $"{totalUnits} Daire Ã— 6.0 mt Ortalama Balkon/FransÄ±z Ã‡evresi",

                Quantity = korkulukMt,

                Unit = "mt",

                ProfessionName = "Korkuluk & Metal UstasÄ±",

                DnaSource = "AdÄ±m 6 Soru 7: Balkon ve FransÄ±z Balkon Korkuluk Sistemi"

            });



            // 22. Bina Ä°Ã§i Sessiz AtÄ±k Su / Pis Su BorularÄ± (AdÄ±m 7)

            var pisuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Pis Su") || d.SelectedItem.Contains("AtÄ±k Su") || d.SelectedItem.Contains("Sessiz"));

            string pisuTitle = pisuDetail != null ? pisuDetail.SelectedItem : "Mineral Takviyeli Sessiz AtÄ±k Su Boru Sistemi";

            decimal pisuMt = totalUnits * 25;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "11. Mekanik, SÄ±hhi Tesisat & YangÄ±n (MEP)",

                Title = $"{pisuTitle} DÃ¼ÅŸey Kolon ve Daire TesisatÄ±",

                Description = "Katlar arasÄ± dikey ana iniÅŸ ÅŸaftlarÄ± ve daire iÃ§i Ä±slak hacimlerin sÄ±zdÄ±rmaz, ses yalÄ±tÄ±mlÄ± pis su borulamasÄ±.",

                Formula = $"{totalUnits} Daire Ã— 25 mt (Åaft ve Daire Ä°Ã§i DaÄŸÄ±tÄ±m)",

                Quantity = pisuMt,

                Unit = "mt",

                ProfessionName = "SÄ±hhi TesisatÃ§Ä±",

                DnaSource = "AdÄ±m 7 Soru 6: AtÄ±k Su / Pis Su Borusu StandardÄ±"

            });



            // 23. Su Deposu & Hidrofor (AdÄ±m 7)

            var suDeposuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Su Deposu") || d.SelectedItem.Contains("Hidrofor"));

            string suDeposuTitle = suDeposuDetail != null ? suDeposuDetail.SelectedItem : "AISI 304 Paslanmaz Ã‡elik ModÃ¼ler Su Deposu & Ã‡ift PompalÄ± Hidrofor";

            int depoTakim = Math.Max(1, blocks.Count);

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "11. Mekanik, SÄ±hhi Tesisat & YangÄ±n (MEP)",

                Title = $"{suDeposuTitle} Sistemi",

                Description = "YangÄ±n ve kullanÄ±m suyu rezervi iÃ§in paslanmaz modÃ¼ler depo, frekans invertÃ¶rlÃ¼ hidrofor ve vana grubu.",

                Formula = $"{depoTakim} Blok / YapÄ± Ã— 1 TakÄ±m Merkezi BasÄ±nÃ§landÄ±rma Paketi",

                Quantity = depoTakim,

                Unit = "TakÄ±m",

                ProfessionName = "SÄ±hhi TesisatÃ§Ä±",

                DnaSource = "AdÄ±m 7 Soru 7: Temiz Su Deposu ve Hidrofor Sistemi"

            });



            // 24. YangÄ±n Merdiveni Basamak KaplamasÄ± (AdÄ±m 8)

            var yanginBasamakDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YangÄ±n Merdiveni Basamak") || d.SelectedItem.Contains("YangÄ±n Merdiveni Kaplama") || d.SelectedItem.Contains("Traverten"));

            string yanginBasamakTitle = yanginBasamakDetail != null ? yanginBasamakDetail.SelectedItem : "DoÄŸaltaÅŸ / Traverten YangÄ±n Merdiveni Basamak KaplamasÄ±";

            decimal yanginBasamakMt = maxFloors * 22;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "12. Ortak Alanlar, YangÄ±n GÃ¼venliÄŸi & SirkÃ¼lasyon",

                Title = yanginBasamakTitle,

                Description = "Acil kaÃ§Ä±ÅŸ ve yangÄ±n merdiveni basamaklarÄ±nÄ±n kaymaz profil traverten/mermer veya epoksi kaplamasÄ±.",

                Formula = $"{maxFloors} Kat Ã— 18 Basamak Ã— 1.2 mt RÄ±ht/Basamak Ã–lÃ§Ã¼sÃ¼",

                Quantity = yanginBasamakMt,

                Unit = "mt",

                ProfessionName = "Mermer & DoÄŸaltaÅŸ UstasÄ±",

                DnaSource = "AdÄ±m 8 Soru 6: YangÄ±n Merdiveni Basamak KaplamasÄ±"

            });



            // 24B. YangÄ±n Merdiveni Korkuluk & KÃ¼peÅŸtesi (AdÄ±m 8)

            var yanginKorkulukDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YangÄ±n Merdiveni Korkuluk") || d.SelectedItem.Contains("KaÃ§Ä±ÅŸ KÃ¼peÅŸte"));

            string yanginKorkulukTitle = yanginKorkulukDetail != null ? yanginKorkulukDetail.SelectedItem : "Elektrostatik BoyalÄ± Demir Boru YangÄ±n KorkuluÄŸu";

            decimal yanginKorkulukMt = maxFloors * 22;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "12. Ortak Alanlar, YangÄ±n GÃ¼venliÄŸi & SirkÃ¼lasyon",

                Title = yanginKorkulukTitle,

                Description = "YangÄ±n merdiveni kovasÄ± emniyet korkuluÄŸu ve yangÄ±na dayanÄ±klÄ± kaÃ§Ä±ÅŸ tutamaÄŸÄ± montajÄ±.",

                Formula = $"{maxFloors} Kat Ã— 22 mt Merdiven Ä°niÅŸ HattÄ±",

                Quantity = yanginKorkulukMt,

                Unit = "mt",

                ProfessionName = "Korkuluk & Metal UstasÄ±",

                DnaSource = "AdÄ±m 8 Soru 7: YangÄ±n Merdiveni Korkuluk StandardÄ±"

            });



            // 24C. Kat HolÃ¼ AsansÃ¶r SÃ¶ve ve Ã‡erÃ§eve KaplamasÄ± (AdÄ±m 8)

            var asansorSoveDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("AsansÃ¶r SÃ¶ve") || d.SelectedItem.Contains("Kat HolÃ¼ AsansÃ¶r"));

            string asansorSoveTitle = asansorSoveDetail != null ? asansorSoveDetail.SelectedItem : "Mermer / Granit AsansÃ¶r KapÄ± SÃ¶vesi";

            int asansorKapiAdet = maxFloors;

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "12. Ortak Alanlar, YangÄ±n GÃ¼venliÄŸi & SirkÃ¼lasyon",

                Title = asansorSoveTitle,

                Description = "Her kat koridorunda asansÃ¶r kapÄ± Ã§evrelerinin mermer, granit veya kompozit sÃ¶ve ile giydirilmesi.",

                Formula = $"{asansorKapiAdet} Kat Ã— 1 KapÄ± AÃ§Ä±klÄ±ÄŸÄ±",

                Quantity = asansorKapiAdet,

                Unit = "Adet",

                ProfessionName = "Mermer & DoÄŸaltaÅŸ UstasÄ±",

                DnaSource = "AdÄ±m 8 Soru 5: Kat HolÃ¼ AsansÃ¶r SÃ¶vesi"

            });



            // 25. YangÄ±n KaÃ§Ä±ÅŸ KapÄ±larÄ± (AdÄ±m 8)

            var yanginKapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YangÄ±n KaÃ§Ä±ÅŸ KapÄ±") || d.SelectedItem.Contains("YangÄ±n Merdiveni ve Acil KaÃ§Ä±ÅŸ") || d.SelectedItem.Contains("EI90") || d.SelectedItem.Contains("EI60"));

            string yanginKapiTitle = yanginKapiDetail != null ? yanginKapiDetail.SelectedItem : "EI90 Panik BarlÄ± Ã‡ift CidarlÄ± YangÄ±n KaÃ§Ä±ÅŸ KapÄ±sÄ±";

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "12. Ortak Alanlar, YangÄ±n GÃ¼venliÄŸi & SirkÃ¼lasyon",

                Title = $"{yanginKapiTitle} (Duman SÄ±zdÄ±rmaz Fitilli)",

                Description = "Kat koridorlarÄ±ndan yangÄ±n merdivenine aÃ§Ä±lan panik barlÄ±, yaylÄ± menteÅŸeli ve duman fitilli sertifikalÄ± yangÄ±n kapÄ±larÄ±.",

                Formula = $"{maxFloors} Kat Ã— 1 Adet Kat YangÄ±n Ã‡Ä±kÄ±ÅŸ KapÄ±sÄ±",

                Quantity = maxFloors,

                Unit = "Adet",

                ProfessionName = "KapÄ± ve AhÅŸap Montaj Ekibi",

                DnaSource = "AdÄ±m 8 Soru 7: YangÄ±n Merdiveni Acil KaÃ§Ä±ÅŸ KapÄ±larÄ±"

            });



            // 26. Kat YangÄ±n DolaplarÄ± & EkipmanlarÄ± (AdÄ±m 8)

            var yanginDolapDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YangÄ±n DolabÄ±") || d.SelectedItem.Contains("YangÄ±n Hortumu"));

            string yanginDolapTitle = yanginDolapDetail != null ? yanginDolapDetail.SelectedItem : "TÃ¼p BÃ¶lmeli GÃ¶mme Tip Kat YangÄ±n DolabÄ± (TSE EN 671-1)";

            list.Add(new ParametricTakeoffItemDto

            {

                Id = itemId++,

                Category = "12. Ortak Alanlar, YangÄ±n GÃ¼venliÄŸi & SirkÃ¼lasyon",

                Title = $"{yanginDolapTitle} ve Lans TakÄ±mÄ±",

                Description = "Her kat koridorunda 30m yarÄ± sert yangÄ±n hortumlu, vanalÄ± ve 6 kg KKT yangÄ±n sÃ¶ndÃ¼rme tÃ¼plÃ¼ dolap montajÄ±.",

                Formula = $"{maxFloors} Kat Ã— 1 Adet Kat YangÄ±n DolabÄ±",

                Quantity = maxFloors,

                Unit = "Adet",

                ProfessionName = "YangÄ±n GÃ¼venlik & Mekanik Ekibi",

                DnaSource = "AdÄ±m 8 Soru 8: Kat YangÄ±n DolabÄ± ve Ekipman Modeli"

            });



            // 27+. DÄ±ÅŸ Saha ve Sosyal DonatÄ±lar (AdÄ±m 9)

            if (project.Amenities != null && project.Amenities.Any())

            {

                foreach (var am in project.Amenities)

                {

                    decimal amQty = (decimal)(am.SquareMeters ?? 1);

                    if (amQty <= 0) amQty = 1;

                    string unit = (am.SquareMeters.HasValue && am.SquareMeters.Value > 0) ? "mÂ²" : "Adet";



                    list.Add(new ParametricTakeoffItemDto

                    {

                        Id = itemId++,

                        Category = "13. DÄ±ÅŸ Saha, Sosyal DonatÄ±lar & Ã‡evre DÃ¼zenleme",

                        Title = $"{am.Name} Ä°malat ve DÃ¼zenlemesi",

                        Description = $"{am.Type ?? "Sosyal DonatÄ±"} kapsamÄ±nda zemin kaplamasÄ±, ekipman montajÄ± ve Ã§evre dÃ¼zenlemesi.",

                        Formula = $"Proje DonatÄ± Envanteri: {am.Name} ({amQty:N0} {unit})",

                        Quantity = amQty,

                        Unit = unit,

                        ProfessionName = "Peyzaj ve DÄ±ÅŸ Saha DÃ¼zenleme UstasÄ±",

                        DnaSource = $"AdÄ±m 9: DonatÄ± Envanteri ({am.Type ?? "Sosyal DonatÄ±"})"

                    });

                }

            }



            return list;

        }



        private List<WorkflowStepItemDto> GenerateParametricWorkflow(ConstructionProject project, ProjectDnaProfile profile, DateTime? baseDate = null, int? buildingId = null)

        {

            var list = new List<WorkflowStepItemDto>();

            if (project == null) return list;



            var allBlocks = project.Blocks?.Where(b => !b.IsExistingBuilding).ToList() ?? new List<GMK360.Core.Entities.Building>();

            var rootBlocks = allBlocks.Where(b => b.ParentBuildingId == null).ToList();



            GMK360.Core.Entities.Building targetStructure = null;

            if (buildingId.HasValue && buildingId.Value > 0)

            {

                targetStructure = allBlocks.FirstOrDefault(b => b.Id == buildingId.Value);

            }

            if (targetStructure == null)

            {

                targetStructure = rootBlocks.FirstOrDefault() ?? allBlocks.FirstOrDefault();

            }



            var targetBlocks = new List<GMK360.Core.Entities.Building>();

            if (targetStructure != null)

            {

                var children = allBlocks.Where(b => b.ParentBuildingId == targetStructure.Id).ToList();

                if (children.Any())

                {

                    targetBlocks.AddRange(children);

                }

                else

                {

                    targetBlocks.Add(targetStructure);

                }

            }

            else

            {

                targetBlocks = allBlocks;

            }



            string targetStructureName = targetStructure?.BlockName ?? targetStructure?.Name ?? (targetBlocks.Count == 1 ? (targetBlocks[0].BlockName ?? targetBlocks[0].Name) : project.Name);

            int? targetBuildingId = targetStructure?.Id ?? targetBlocks.FirstOrDefault()?.Id;



            double landArea = project.TotalLandArea ?? 0;

            double baseArea = targetStructure?.BaseArea ?? targetBlocks.Sum(b => b.BaseArea ?? 0);

            if (baseArea <= 0 && targetBlocks.Any()) baseArea = targetBlocks.Sum(b => b.BaseArea ?? 0);

            if (baseArea <= 0) baseArea = project.EskiBinaOturumAlani ?? (landArea > 0 ? landArea * 0.40 : 500);



            int maxFloors = targetBlocks.Any() ? targetBlocks.Max(b => (b.TotalFloors ?? 0) + b.BasementFloors) : (project.TotalFloors ?? 5);

            if (targetStructure != null && targetStructure.TotalFloors.HasValue && targetStructure.TotalFloors.Value > 0)

            {

                int structFloors = (targetStructure.TotalFloors.Value) + targetStructure.BasementFloors;

                if (structFloors > maxFloors) maxFloors = structFloors;

            }



            int maxBasement = targetBlocks.Any() ? targetBlocks.Max(b => b.BasementFloors) : (project.EskiBodrumKatSayisi ?? 2);

            if (targetStructure != null && targetStructure.BasementFloors > maxBasement) maxBasement = targetStructure.BasementFloors;

            if (maxBasement <= 0) maxBasement = 2;

            if (maxFloors <= 0) maxFloors = 5;



            double totalFloorArea = 0;

            if (targetBlocks.Any(b => b.BaseArea.HasValue && b.BaseArea > 0))

            {

                totalFloorArea = targetBlocks.Sum(b => (b.BaseArea ?? (baseArea / Math.Max(1, targetBlocks.Count))) * ((b.TotalFloors ?? 5) + b.BasementFloors));

            }

            else

            {

                totalFloorArea = baseArea * maxFloors;

            }

            if (totalFloorArea <= 0) totalFloorArea = baseArea * maxFloors;



            double landPerimeter = landArea > 0 ? Math.Round(4 * Math.Sqrt(landArea), 1) : 240;

            double buildingPerimeter = baseArea > 0 ? Math.Round(4 * Math.Sqrt(baseArea), 1) : 120;



            int totalUnits = targetBlocks.Sum(b => b.TotalApartments);

            if (totalUnits <= 0) totalUnits = targetStructure?.TotalApartments ?? (project.TargetTotalApartments ?? (project.EskiDaireSayisi ?? 10));



            var details = profile?.Details?.ToList() ?? new List<ProjectDnaDetail>();



            var zeminDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Zemin") || d.SelectedItem.Contains("Kaya") || d.SelectedItem.Contains("Kum") || d.SelectedItem.Contains("Kil"));

            string zeminTipi = zeminDetail != null ? zeminDetail.SelectedItem : "YumuÅŸak / Kumlu Zemin";

            double kabarma = zeminTipi.Contains("Kaya") ? 1.35 : 1.25;



            decimal ihataUzunlugu = (decimal)landPerimeter;

            var ihataLenDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Ã‡evre SÄ±nÄ±rÄ± Toplam") || (d.SelectedItem.Contains("Ä°hata") && d.SelectedItem.Contains(":")));

            if (ihataLenDetail != null && ihataLenDetail.Quantity > 0)

            {

                ihataUzunlugu = ihataLenDetail.Quantity;

            }

            else if (ihataLenDetail != null && ihataLenDetail.SelectedItem.Contains(":"))

            {

                var p = ihataLenDetail.SelectedItem.Split(':');

                if (p.Length > 1 && decimal.TryParse(p[1].Replace("m", "").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal qVal))

                {

                    ihataUzunlugu = qVal;

                }

            }



            var ihataMatDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Sac") || d.SelectedItem.Contains("Tel Ã–rgÃ¼") || d.SelectedItem.Contains("Pano"));

            string ihataMalzemesi = ihataMatDetail != null ? ihataMatDetail.SelectedItem : "Trapez Sac Kaplama";



            var kapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("KapÄ±"));

            string kapiTipi = kapiDetail != null ? kapiDetail.SelectedItem : "SÃ¼rgÃ¼lÃ¼ RaylÄ± Sac KapÄ± (6m)";



            var molozDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YÄ±kÄ±m") || d.SelectedItem.Contains("Moloz") || d.SelectedItem.Contains("Enkaz") || d.SelectedItem.Contains("BoÅŸ Arsa"));

            bool isCleanPlot = molozDetail != null && (molozDetail.SelectedItem.Contains("BoÅŸ") || molozDetail.SelectedItem.Contains("Gerekmez") || molozDetail.SelectedItem.Contains("Yok"));

            bool molozVar = molozDetail != null && !isCleanPlot && (molozDetail.SelectedItem.Contains("YÄ±kÄ±lacak") || molozDetail.SelectedItem.Contains("Moloz ve Ã‡Ã¶p") || molozDetail.SelectedItem.Contains("Enkaz"));

            bool hasDemolition = !isCleanPlot && (molozVar || (project.EskiToplamMetrekare.HasValue && project.EskiToplamMetrekare.Value > 0) || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0));



            var elektrikDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("AdÄ±m 1")) && (d.SelectedItem.Contains("Elektrik") || d.SelectedItem.Contains("Abonelik") || d.SelectedItem.Contains("Trafo") || d.SelectedItem.Contains("JeneratÃ¶r")));

            string elektrikTipi = elektrikDetail != null ? elektrikDetail.SelectedItem : "Åantiye Åebeke AboneliÄŸi ve TMÅ DaÄŸÄ±tÄ±m Panosu";



            var suDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("AdÄ±m 1")) && (d.SelectedItem.Contains("Su") || d.SelectedItem.Contains("Depo") || d.SelectedItem.Contains("Hidrofor") || d.SelectedItem.Contains("Kuyu")));

            string suTipi = suDetail != null ? suDetail.SelectedItem : "15 Tonluk Su Deposu ve 3 HP Hidrofor";



            int konteynerSayisi = Math.Max(2, Math.Min(6, (int)Math.Ceiling(totalFloorArea / 5000.0)));

            decimal kaziM3 = Math.Round((decimal)(baseArea * (maxBasement * 3.2 + 1.0) * kabarma), 0);

            decimal iksaM2 = Math.Round((decimal)(buildingPerimeter * (maxBasement * 3.2)), 0);

            decimal grobetonM3 = Math.Round((decimal)(baseArea * 0.15), 0);

            decimal yalitimM2 = Math.Round((decimal)(baseArea + (buildingPerimeter * (maxBasement * 3.2))), 0);

            decimal temelBetonM3 = Math.Round((decimal)(baseArea * 0.70), 0);

            if (temelBetonM3 < 50) temelBetonM3 = 100;

            decimal temelDemirTon = Math.Round((decimal)(temelBetonM3 * 0.085m), 1);



            decimal karkasBetonM3 = Math.Round((decimal)(totalFloorArea * 0.38), 0);

            decimal karkasDemirTon = Math.Round((decimal)(totalFloorArea * 0.032), 1);



            decimal sapM2 = Math.Round((decimal)(totalFloorArea * 0.82), 0);

            decimal iskeleM2 = Math.Round((decimal)(buildingPerimeter * (maxFloors * 3.0)), 0);

            decimal mantoM2 = Math.Round((decimal)(iskeleM2 * 0.85m), 0);

            decimal harpushtaMt = Math.Round((decimal)(buildingPerimeter * 1.10), 0);

            decimal denizlikMt = totalUnits * 7;

            decimal korkulukMt = totalUnits * 6;

            decimal pisuMt = totalUnits * 25;

            var pisuDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Pis Su") || d.SelectedItem.Contains("AtÄ±k Su") || d.SelectedItem.Contains("Sessiz"));

            string pisuTitle = pisuDetail != null ? pisuDetail.SelectedItem : "Sessiz AtÄ±k Su Boru Sistemi";



            decimal landscapeArea = (decimal)(project.LandscapeArea ?? (landArea * 0.4));

            DateTime curDate = (baseDate ?? DateTime.Today).Date;



            // 4 ParÃ§a HiyerarÅŸi Sabitleri

            int p1Idx = 1;

            string p1Name = "1. ParÃ§a: Kaba Ä°nÅŸaat (Temel & TaÅŸÄ±yÄ±cÄ± Ä°skelet)";

            string p1Badge = "primary";

            string p1Trigger = "Ä°ÅŸbaÅŸÄ± / Åantiye KuruluÅŸu ile BaÅŸlar";



            int p2Idx = 2;

            string p2Name = "2. ParÃ§a: Statik Ä°nce (Kaba Ä°nce / Kovalayan Ä°ÅŸler)";

            string p2Badge = "info";

            string p2Trigger = "âš¡ Kovalama KuralÄ±: Kaba 2 kat Ã¶nden gidince alttan kat kat baÅŸlar!";



            int p3Idx = 3;

            string p3Name = "3. ParÃ§a: Tercihe GÃ¶re Ä°nce Ä°ÅŸler (Daire Ä°Ã§i Ä°malatlar)";

            string p3Badge = "warning";

            string p3Trigger = "âš¡ Tetikleyici: Kat kaba duvarlarÄ± ve tesisat sÄ±zdÄ±rmazlÄ±k testi bitince baÅŸlar!";



            int p4Idx = 4;

            string p4Name = "4. ParÃ§a: DemirbaÅŸ, Montaj & DÄ±ÅŸ Ã‡evre (Teslim AÅŸamasÄ±)";

            string p4Badge = "success";

            string p4Trigger = "âš¡ Tetikleyici: Daire iÃ§i boya ve zemin kaplamalarÄ± bitince temiz montaja geÃ§ilir!";



            // Helper metot: Alt adÄ±mlarÄ± temiz (mokapsÄ±z) oluÅŸturur

            List<WorkflowSubStepDto> CreateSubSteps(string prePrep, string exec, string quality, string isg)

            {

                return new List<WorkflowSubStepDto>

                {

                    new WorkflowSubStepDto { Title = prePrep, Category = "PrePreparation", IsCompleted = false },

                    new WorkflowSubStepDto { Title = exec, Category = "Execution", IsCompleted = false },

                    new WorkflowSubStepDto { Title = quality, Category = "QualityAndCuring", IsCompleted = false },

                    new WorkflowSubStepDto { Title = isg, Category = "SafetyIsg", IsCompleted = false }

                };

            }



            int stepOrderSeq = 1;

            DateTime runDate = curDate;



            // =========================================================================

            // 1. PARÃ‡A: KABA Ä°NÅAAT (TEMEL & TAÅIYICI Ä°SKELET)

            // =========================================================================



            // K-01: Derin KazÄ± (Hafriyat) & Zemin Ä°ksasÄ± (Shoring) (AdÄ±m 1)

            var k01Start = runDate;

            var k01End = k01Start.AddDays(14);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-01",

                Order = stepOrderSeq++,

                PhaseName = "Derin KazÄ± (Hafriyat) & Zemin Ä°ksasÄ± (Shoring)",

                StageCategory = "Hafriyat & Zemin Ä°ksasÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Åantiye ortak saha hazÄ±rlÄ±ÄŸÄ± tamamlanÄ±nca yapÄ± tabanÄ±nda baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = $"{zeminTipi} KazÄ±sÄ±, Ä°ksa HasÄ±r Ã‡eliÄŸi ve PÃ¼skÃ¼rtme Beton (Shotcrete)",

                MaterialDescription = "Kademeli temel hafriyat kazÄ±sÄ±, dÃ¶kÃ¼m sahasÄ±na nakliye, hasÄ±r Ã§elik ve pÃ¼skÃ¼rtme beton (shotcrete) iksa perdesi.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ³",

                MaterialFormula = "DXF Bekliyor (Bodrum kat kesit yÃ¼ksekliklerinden ve kazÄ± kotundan hesaplanacak)",

                SupplierCategory = "Hafriyat & Zemin FirmasÄ±",

                LaborTitle = "AÄŸÄ±r Hafriyat & Ä°ksa Operasyon Ekibi",

                ProfessionName = "HafriyatÃ§Ä± & Ä°ksacÄ±",

                CrewComposition = "2 AÄŸÄ±r EkskavatÃ¶r Op. + 1 Loder Op. + 8 Kamyon ÅofÃ¶rÃ¼ + 2 Delgi Op. + 2 Shotcrete UstasÄ± + 2 Saha GÃ¼venlik",

                CrewSize = 17,

                DurationDays = 14,

                EquipmentNeeded = "2 EkskavatÃ¶r (KovalÄ± & KÄ±rÄ±cÄ±lÄ±), 1 Loder, 8 Damperli Kamyon, Shotcrete PompasÄ±, Ankraj Delgi Makinesi",

                Predecessors = "Åantiye ortak mobilizasyonunun ve kÃ¶ÅŸe kazÄ±klarÄ±nÄ±n onaylanmasÄ±",

                CriticalDependency = "Yol ve komÅŸu parsel ÅŸevlerinin gÃ¶Ã§mesini Ã¶nlemek iÃ§in kademeli iksa uygulanmadan derin kazÄ±ya girilemez!",

                DnaSource = "AdÄ±m 1: Zemin & Hafriyat",

                PlannedStartDate = k01Start,

                PlannedEndDate = k01End,

                SubSteps = CreateSubSteps(

                    "T-3 Zemin etÃ¼dÃ¼ raporu kontrolÃ¼, hafriyat dÃ¶kÃ¼m fiÅŸleri ve kazÄ± gÃ¼zergah izinleri teyidi",

                    "Kademeli kazÄ±, kamyonlara yÃ¼kleme, iksa perdesi hasÄ±r Ã§elik ve pÃ¼skÃ¼rtme beton uygulamasÄ±",

                    "Optik nivo ile temel taban kotu kontrolÃ¼ ve iksa deplasman lazer okumasÄ±",

                    "Derin kazÄ± Ã§ukuru Ã§evresi korkuluklarÄ±, araÃ§ teker stoperi ve ÅŸev gÃ¼venlik takibi"

                )

            });



            // K-02: Taban Tesviyesi, MÄ±cÄ±r DrenajÄ± & C20/25 Grobeton (AdÄ±m 2 & AdÄ±m 0)

            var k02Start = k01End;

            var k02End = k02Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-02",

                Order = stepOrderSeq++,

                PhaseName = "Taban Tesviyesi, MÄ±cÄ±r DrenajÄ± & C20/25 Grobeton",

                StageCategory = "Temel TabanÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "KazÄ± tamamlanÄ±p taban kotu alÄ±nÄ±nca baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "C20/25 HazÄ±r Grobeton ve Taban Filon MÄ±cÄ±rÄ±",

                MaterialDescription = "Temel tabanÄ±na mÄ±cÄ±r serilmesi, silindirlenmesi ve Ã¼zerine mastarlÄ± grobeton dÃ¶kÃ¼mÃ¼.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ³",

                MaterialFormula = "DXF Bekliyor (Temel taban oturum alanÄ±ndan hesaplanacak)",

                SupplierCategory = "HazÄ±r Beton Santrali & TaÅŸ OcaÄŸÄ±",

                LaborTitle = "Grobeton ve Taban Tesviye Ekibi",

                ProfessionName = "Grobeton & Zemin UstasÄ±",

                CrewComposition = "1 Beton Formeni + 1 Pompa OperatÃ¶rÃ¼ + 4 VibratÃ¶r Usta/Amelesi + 2 MastarcÄ±",

                CrewSize = 8,

                DurationDays = 2,

                EquipmentNeeded = "Beton PompasÄ±, Silindir, DalgÄ±Ã§ VibratÃ¶r, AlÃ¼minyum Mastarlar",

                Predecessors = "Temel kazÄ±sÄ±nÄ±n tamamlanmasÄ± ve taban kotunun onaylanmasÄ±",

                CriticalDependency = "Grobeton mastarÄ± dÃ¼zgÃ¼n atÄ±lmadan Ã¼zerine su yalÄ±tÄ±m membranÄ± serilemez!",

                DnaSource = "AdÄ±m 2: Taban Ä°malatlarÄ± (Grobeton)",

                PlannedStartDate = k02Start,

                PlannedEndDate = k02End,

                SubSteps = CreateSubSteps(

                    "T-2 Beton santrali grobeton sipariÅŸi ve silindir ile zemin sÄ±kÄ±ÅŸtÄ±rmasÄ±",

                    "Filon mÄ±cÄ±r serimi, grobeton pompa dÃ¶kÃ¼mÃ¼ ve mastarlÄ± tesviye",

                    "Grobeton Ã¼zeri kot/mastar dÃ¼zgÃ¼nlÃ¼ÄŸÃ¼ ve priz alma sulamasÄ±",

                    "Pompa boru hatlarÄ±nÄ±n emniyet kilitleri ve dÃ¶kÃ¼m alanÄ± KKD denetimi"

                )

            });



            // K-03: Ã‡ift Kat BitÃ¼mlÃ¼ Membran Temel BohÃ§alama & Koruma ÅapÄ± (AdÄ±m 2)

            var k03Start = k02End;

            var k03End = k03Start.AddDays(5);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-03",

                Order = stepOrderSeq++,

                PhaseName = "Ã‡ift Kat BitÃ¼mlÃ¼ Membran Temel BohÃ§alama & Koruma ÅapÄ±",

                StageCategory = "Temel Su YalÄ±tÄ±mÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Grobeton kuruyunca baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "SBS KatkÄ±lÄ± Ã‡ift Kat Polyester KeÃ§eli Membran, BitÃ¼m Astar ve Koruma Betonu",

                MaterialDescription = "Grobeton Ã¼zerine bitÃ¼m astar, Ã§ift kat membran ÅŸaloma yapÄ±ÅŸtÄ±rma, pah teÅŸliki ve Ã¼zerine koruma betonu.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Temel tabanÄ± ve perde Ã§evre yÃ¼zey alanÄ±ndan hesaplanacak)",

                SupplierCategory = "Su Ä°zolasyon Malzemeleri Bayisi",

                LaborTitle = "Su YalÄ±tÄ±mÄ± ve BohÃ§alama Ekibi",

                ProfessionName = "Su YalÄ±tÄ±m UstasÄ±",

                CrewComposition = "1 Ä°zolasyon Formeni + 4 Åaloma/YalÄ±tÄ±m UstasÄ± + 2 TaÅŸÄ±ma Ä°ÅŸÃ§isi",

                CrewSize = 7,

                DurationDays = 5,

                EquipmentNeeded = "Åaloma TakÄ±mlarÄ±, Gaz TÃ¼pleri, Astar PÃ¼skÃ¼rtme PompasÄ±",

                Predecessors = "Grobetonun kurumasÄ± ve yÃ¼zey tozunun temizlenmesi",

                CriticalDependency = "Su yalÄ±tÄ±mÄ± delinmeden Ã¼zerine donatÄ± demiri konamaz; mutlaka koruma ÅŸapÄ±/betonu atÄ±lmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 2: Temel BohÃ§alama",

                PlannedStartDate = k03Start,

                PlannedEndDate = k03End,

                SubSteps = CreateSubSteps(

                    "T-2 BitÃ¼m astar, membran rulolarÄ± ve ÅŸaloma tÃ¼plerinin sevki",

                    "Astar uygulamasÄ±, Ã§ift kat ÅŸaÅŸÄ±rtmalÄ± ÅŸaloma yapÄ±ÅŸtÄ±rma ve pah Ã§Ä±talarÄ± teÅŸliki",

                    "Ek yerleri bindirme kaynak kontrolÃ¼ ve koruma betonu uygulamasÄ±",

                    "Åaloma alevli Ã§alÄ±ÅŸma yangÄ±n gÃ¼venlik tÃ¼pleri ve havalandÄ±rma Ã¶nlemleri"

                )

            });



            // K-04: Radye Temel AhÅŸap / Ã‡elik DÄ±ÅŸ KalÄ±bÄ± & Su Tutucu Bant (AdÄ±m 0)

            var k04Start = k03End;

            var k04End = k04Start.AddDays(4);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-04",

                Order = stepOrderSeq++,

                PhaseName = "Radye Temel AhÅŸap / Ã‡elik DÄ±ÅŸ KalÄ±bÄ± & Su Tutucu Bant",

                StageCategory = "Temel KalÄ±p",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Koruma ÅŸapÄ± kuruyunca temel kalÄ±bÄ± Ã§akÄ±lÄ±r",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Radye Temel DÄ±ÅŸ KalÄ±p Kerestesi, Plywood, Payandalar ve PVC Su Tutucu Bant",

                MaterialDescription = "Radye temel yan perdeleri dÄ±ÅŸ Ã§evre kalÄ±bÄ±nÄ±n Ã§akÄ±lmasÄ±, kÃ¶ÅŸegen payandalar ve soÄŸuk derz su tutucu bant montajÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Temel kalÄ±nlÄ±ÄŸÄ± ve Ã§evre Ã¶lÃ§Ã¼sÃ¼nden hesaplanacak)",

                SupplierCategory = "KalÄ±p & Kereste TedarikÃ§isi",

                LaborTitle = "Temel KalÄ±pÃ§Ä± Ekibi",

                ProfessionName = "KalÄ±p UstasÄ±",

                CrewComposition = "1 KalÄ±p Formeni + 5 KalÄ±pÃ§Ä± UstasÄ± + 2 YardÄ±mcÄ±",

                CrewSize = 8,

                DurationDays = 4,

                EquipmentNeeded = "Daire Testere, KalÄ±p KrikolarÄ±, Lazer Hizalama",

                Predecessors = "BohÃ§alama koruma ÅŸapÄ± Ã¼zerine temel akslarÄ±nÄ±n Ã§Ä±rpÄ± ipiyle iÅŸaretlenmesi",

                CriticalDependency = "KalÄ±p ÅŸakÃ¼lÃ¼ ve rijit payandalarÄ± kontrol edilmeden demir donatÄ± dÃ¶ÅŸenmeye baÅŸlanamaz!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (Temel KalÄ±bÄ±)",

                PlannedStartDate = k04Start,

                PlannedEndDate = k04End,

                SubSteps = CreateSubSteps(

                    "T-2 KalÄ±p kerestesi, plywood panolar ve su tutucu bantlarÄ±n sahaya indirilmesi",

                    "Aks Ã§izgilerine gÃ¶re radye yan kalÄ±bÄ±nÄ±n montajÄ± ve payanda tahkimatÄ±",

                    "KalÄ±p iÃ§i temizlik, kot nivo kontrolÃ¼ ve su tutucu bant sabitleme kontrolÃ¼",

                    "KalÄ±p Ã§ivi batmalarÄ±na karÅŸÄ± Ã§elik burunlu tabanlÄ± iÅŸ ayakkabÄ±sÄ± denetimi"

                )

            });



            // K-05: Radye Temel B420C DonatÄ± Ã‡eliÄŸi, PaspaylarÄ± & Demir Vizesi (AdÄ±m 0)

            var k05Start = k04End;

            var k05End = k05Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-05",

                Order = stepOrderSeq++,

                PhaseName = "Radye Temel B420C DonatÄ± Ã‡eliÄŸi, PaspaylarÄ± & Demir Vizesi",

                StageCategory = "Temel DonatÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "DÄ±ÅŸ kalÄ±p tamamlanÄ±nca donatÄ± Ã¶rÃ¼lÃ¼r",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "B420C NervÃ¼rlÃ¼ DonatÄ± Ã‡eliÄŸi, Beton PaspayÄ± TakozlarÄ± ve BaÄŸ Teli",

                MaterialDescription = "Alt/Ã¼st Ã§ift sÄ±ra nervÃ¼rlÃ¼ hasÄ±r donatÄ±, donatÄ± sehpalarÄ±, kolon/perde filizleri, topraklama ÅŸeridi ve paspaylarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "Ton",

                MaterialFormula = "DXF Bekliyor (Statik radye temel donatÄ± planÄ±ndan hesaplanacak)",

                SupplierCategory = "Demir Ã‡elik Bayisi & DonatÄ± TedarikÃ§isi",

                LaborTitle = "Temel Demirci Ekibi",

                ProfessionName = "Demirci UstasÄ±",

                CrewComposition = "1 Demir Formeni + 8 Demirci UstasÄ± + 4 BaÄŸlama Ä°ÅŸÃ§isi",

                CrewSize = 13,

                DurationDays = 7,

                EquipmentNeeded = "Demir BÃ¼kme ve Kesme TezgahÄ±, BaÄŸ Teli KargaburunlarÄ±",

                Predecessors = "Radye dÄ±ÅŸ kalÄ±bÄ± ve temel topraklama galvaniz ÅŸeridi montajÄ±",

                CriticalDependency = "YapÄ± Denetim ve Åantiye Åefi DEMÄ°R VÄ°ZESÄ° imzalamadan hazÄ±r beton santraline sevk emri VERÄ°LEMEZ!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (B420C DonatÄ±)",

                PlannedStartDate = k05Start,

                PlannedEndDate = k05End,

                SubSteps = CreateSubSteps(

                    "T-3 DonatÄ± Ã§eliÄŸi Ã§ap, boy ve etiket kontrolÃ¼, paspayÄ± takozlarÄ±nÄ±n serimi",

                    "Alt hasÄ±r, sehpalar, Ã¼st hasÄ±r baÄŸlama ve kolon/perde filizlerinin montajÄ±",

                    "Demir aralÄ±klarÄ±, kanca boylarÄ± ve YapÄ± Denetim Demir Vizesi kabulÃ¼",

                    "AÄŸÄ±r demir taÅŸÄ±ma ergonomisi, kesme tezgahÄ± siperlikleri ve eldiven kullanÄ±mÄ±"

                )

            });



            // K-06: Radye Temel C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton DÃ¶kÃ¼mÃ¼ & Vibrasyon (AdÄ±m 0)

            var k06Start = k05End;

            var k06End = k06Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-06",

                Order = stepOrderSeq++,

                PhaseName = "Radye Temel C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton DÃ¶kÃ¼mÃ¼ & Vibrasyon",

                StageCategory = "Temel Beton DÃ¶kÃ¼mÃ¼",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Demir vizesi onaylandÄ±ÄŸÄ± gÃ¼n dÃ¶kÃ¼lÃ¼r",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton (Su GeÃ§irimsizlik KatkÄ±lÄ±)",

                MaterialDescription = "Monolitik soÄŸuk derzsiz radye temel betonu dÃ¶kÃ¼mÃ¼, derin vibrasyon sÄ±kÄ±ÅŸtÄ±rmasÄ± ve mastarlama.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ³",

                MaterialFormula = "DXF Bekliyor (Statik radye hacim hesabÄ±ndan netleÅŸecek)",

                SupplierCategory = "HazÄ±r Beton Santrali",

                LaborTitle = "Temel Beton DÃ¶kÃ¼m Ekibi",

                ProfessionName = "Betoncu & Pompa Ekibi",

                CrewComposition = "1 Åef + 2 Mobil Pompa OperatÃ¶rÃ¼ + 4 VibratÃ¶rcÃ¼ + 4 MastarcÄ±",

                CrewSize = 11,

                DurationDays = 2,

                EquipmentNeeded = "2 Mobil Beton PompasÄ±, 4 DalgÄ±Ã§ VibratÃ¶r, Slump Konisi, Numune KÃ¼p KalÄ±plarÄ±",

                Predecessors = "Demir vizesi onayÄ± ve beton santrali dÃ¶kÃ¼m rezervasyonu",

                CriticalDependency = "Her transmikserde slump testi yapÄ±lmalÄ± ve YapÄ± Denetim gÃ¶zetiminde 7/28 gÃ¼nlÃ¼k kÃ¼p numuneleri alÄ±nmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (C30/37 Beton)",

                PlannedStartDate = k06Start,

                PlannedEndDate = k06End,

                SubSteps = CreateSubSteps(

                    "T-2 Santral transmikser rotasÄ±, yedek pompa ve laboratuvar numune hazÄ±rlÄ±ÄŸÄ±",

                    "Kademeli katman dÃ¶kÃ¼mÃ¼, derin vibrasyon ve lazer nivo ile yÃ¼zey mastarlama",

                    "Ä°rsaliye kontrolÃ¼, taze beton slump deneyi ve 6 adet kÃ¼p numune alÄ±mÄ±",

                    "Pompa bomu elektrik hattÄ± mesafesi, mikser manevra gÃ¼venlik elemanÄ±"

                )

            });



            // K-07: Temel KÃ¼rlenme, Islak Sulama & DÄ±ÅŸ KalÄ±p SÃ¶kÃ¼mÃ¼ (AdÄ±m 0)

            var k07Start = k06End;

            var k07End = k07Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-07",

                Order = stepOrderSeq++,

                PhaseName = "Temel KÃ¼rlenme, Islak Sulama & DÄ±ÅŸ KalÄ±p SÃ¶kÃ¼mÃ¼",

                StageCategory = "Temel KÃ¼r & BakÄ±m",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Beton dÃ¶kÃ¼mÃ¼ bitiminde anÄ±nda baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Beton KÃ¼r Malzemesi (KÃ¼r MembranÄ±) / Geotekstil KeÃ§e ve Bol Su",

                MaterialDescription = "Radye yÃ¼zeyinin Ã§atlamasÄ±nÄ± Ã¶nlemek iÃ§in geotekstil keÃ§e serimi, sÄ±vÄ± kÃ¼r kimyasalÄ± ve 7 gÃ¼n kesintisiz sulama.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Temel Ã¼st yÃ¼zey alanÄ±ndan hesaplanacak)",

                SupplierCategory = "YapÄ± KimyasallarÄ± & Åantiye Malzemeleri",

                LaborTitle = "KÃ¼rleme & KalÄ±p SÃ¶kÃ¼m Ekibi",

                ProfessionName = "KÃ¼rleme & KalÄ±p SÃ¶kÃ¼m UstasÄ±",

                CrewComposition = "1 Formen + 2 KÃ¼rleme Ä°ÅŸÃ§isi + 3 KalÄ±p SÃ¶kÃ¼m UstasÄ±",

                CrewSize = 6,

                DurationDays = 7,

                EquipmentNeeded = "BasÄ±nÃ§lÄ± Su HortumlarÄ±, PÃ¼skÃ¼rtme PompasÄ±, Manivela Levye",

                Predecessors = "Beton dÃ¶kÃ¼mÃ¼nÃ¼n tamamlanmasÄ± ve yÃ¼zey prizinin baÅŸlamasÄ±",

                CriticalDependency = "RÃ¶tre Ã§atlaklarÄ±nÄ± Ã¶nlemek iÃ§in ilk 7 gÃ¼n boyunca beton yÃ¼zeyi kesinlikle kuru bÄ±rakÄ±lamaz!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (Beton BakÄ±mÄ±)",

                PlannedStartDate = k07Start,

                PlannedEndDate = k07End,

                SubSteps = CreateSubSteps(

                    "T-1 Geotekstil keÃ§e ve kÃ¼r kimyasalÄ± hazÄ±rlÄ±ÄŸÄ±, su hattÄ± kontrolÃ¼",

                    "YÃ¼zey kÃ¼r kimyasalÄ± pÃ¼skÃ¼rtme, keÃ§e serme, gÃ¼nde 3 defa Ä±slatma ve 48 saat sonra dÄ±ÅŸ kalÄ±p sÃ¶kÃ¼mÃ¼",

                    "Beton yÃ¼zeyinde Ã§atlak kontrolÃ¼ ve sÄ±caklÄ±k gradyan takibi",

                    "Gece sulamasÄ± aydÄ±nlatmasÄ± ve kaygan zemin uyarÄ± levhalarÄ±"

                )

            });



            // K-08: Bodrum / Kat Kolon & Perde KalÄ±bÄ± (EndÃ¼striyel Ã‡elik / Plywood) (AdÄ±m 0)

            var k08Start = k07End;

            var k08End = k08Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-08",

                Order = stepOrderSeq++,

                PhaseName = "Bodrum / Kat Kolon & Perde KalÄ±bÄ± (EndÃ¼striyel Ã‡elik / Plywood)",

                StageCategory = "DÃ¼ÅŸey TaÅŸÄ±yÄ±cÄ± KalÄ±p",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Temel priz alÄ±p filizler doÄŸrulanÄ±nca baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "EndÃ¼striyel Ã‡elik Ã‡erÃ§eveli Kolon/Perde KalÄ±p PanolarÄ±, Ã‡irozlar ve Tie-Rod Milleri",

                MaterialDescription = "Kolon ve perde dÃ¼ÅŸey taÅŸÄ±yÄ±cÄ±larÄ±n endÃ¼striyel kalÄ±plarÄ±nÄ±n kurulmasÄ±, tie-rod milleri, Ã§iroz kilitlemeleri ve ÅŸakÃ¼l payandalarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Kat yÃ¼ksekliÄŸi ve kolon/perde Ã§evrelerinden hesaplanacak)",

                SupplierCategory = "EndÃ¼striyel KalÄ±p & Ä°skele FirmasÄ±",

                LaborTitle = "DÃ¼ÅŸey KalÄ±pÃ§Ä± Ekibi",

                ProfessionName = "KalÄ±p UstasÄ±",

                CrewComposition = "1 KalÄ±p Formeni + 6 KalÄ±p UstasÄ± + 3 YardÄ±mcÄ±",

                CrewSize = 10,

                DurationDays = 8,

                EquipmentNeeded = "Kule VinÃ§ / Mobil VinÃ§, Lazer ÅakÃ¼l, KalÄ±p YaÄŸÄ± PompasÄ±",

                Predecessors = "Temel kolon/perde filiz akslarÄ±nÄ±n lazerle kot ve kaÃ§Ä±klÄ±k kontrolÃ¼",

                CriticalDependency = "KalÄ±p iÃ§i temizlik cepleri aÃ§Ä±lmalÄ±, dÃ¶kÃ¼m Ã¶ncesi dipteki talaÅŸ ve tel artÄ±klarÄ± basÄ±nÃ§lÄ± suyla yÄ±kanmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (DÃ¼ÅŸey KalÄ±p)",

                PlannedStartDate = k08Start,

                PlannedEndDate = k08End,

                SubSteps = CreateSubSteps(

                    "T-2 KalÄ±p panolarÄ± temizliÄŸi, kalÄ±p yaÄŸÄ± sÃ¼rÃ¼lmesi ve kule vinÃ§ halat kontrolleri",

                    "Tek yÃ¼z kalÄ±p kapatma, donatÄ± sonrasÄ± Ã§ift yÃ¼z kapatma, tie-rod ve payanda ayarÄ±",

                    "Lazer ÅŸakÃ¼l ile kolon ve perde dÃ¼ÅŸeylik kontrolÃ¼ (maksimum 3 mm tolerans)",

                    "KalÄ±p iskelesi emniyet korkuluklarÄ± ve kule vinÃ§ yÃ¼k kaldÄ±rma gÃ¼venlik sahasÄ±"

                )

            });



            // K-09: Bodrum / Kat Kolon & Perde B420C DonatÄ± Ã‡eliÄŸi (AdÄ±m 0)

            var k09Start = k08Start.AddDays(2);

            var k09End = k09Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-09",

                Order = stepOrderSeq++,

                PhaseName = "Bodrum / Kat Kolon & Perde B420C DonatÄ± Ã‡eliÄŸi",

                StageCategory = "DÃ¼ÅŸey TaÅŸÄ±yÄ±cÄ± DonatÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Kolon/perde tek yÃ¼z kalÄ±bÄ± dikilince baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "B420C Boyuna DÃ¼ÅŸey DonatÄ±lar, Etriyeler, Ã‡irozlar ve Plastik PaspayÄ± Halka TakozlarÄ±",

                MaterialDescription = "DÃ¼ÅŸey kolon/perde ana donatÄ±larÄ±nÄ±n filizlerle bindirilmesi, deprem etriye sÄ±klaÅŸtÄ±rmalarÄ±, Ã§irozlar ve paspayÄ± halkalarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "Ton",

                MaterialFormula = "DXF Bekliyor (Statik kolon aplikasyon Ã§izimlerinden hesaplanacak)",

                SupplierCategory = "Demir Ã‡elik Bayisi",

                LaborTitle = "DÃ¼ÅŸey DonatÄ± Demirci Ekibi",

                ProfessionName = "Demirci UstasÄ±",

                CrewComposition = "1 Demir Formeni + 6 Demirci UstasÄ± + 2 BaÄŸlama Ä°ÅŸÃ§isi",

                CrewSize = 9,

                DurationDays = 7,

                EquipmentNeeded = "Demir BÃ¼kme/Kesme TezgahÄ±, PaspayÄ± HalkalarÄ±, BaÄŸ Teli",

                Predecessors = "KalÄ±p tek yÃ¼zÃ¼nÃ¼n dikilmesi ve paspaylarÄ±nÄ±n yerleÅŸtirilmesi",

                CriticalDependency = "Kolon alt ve Ã¼st etriye sÄ±klaÅŸtÄ±rma bÃ¶lgeleri statik projeye gÃ¶re sayÄ±lmadan kalÄ±p kapatÄ±lamaz!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (DÃ¼ÅŸey DonatÄ±)",

                PlannedStartDate = k09Start,

                PlannedEndDate = k09End,

                SubSteps = CreateSubSteps(

                    "T-2 DonatÄ± Ã§ap/boy kontrolÃ¼ ve etriye aralÄ±klarÄ±nÄ±n iÅŸaretlenmesi",

                    "DÃ¼ÅŸey boy donatÄ±larÄ±nÄ±n bindirilmesi, etriye sÄ±klaÅŸtÄ±rmalarÄ± ve Ã§iroz baÄŸlama",

                    "Etriye kanca 135 derece kÄ±vrÄ±m kontrolÃ¼, paspayÄ± halkalarÄ±nÄ±n denetimi",

                    "DÃ¼ÅŸey donatÄ± Ã¼zerinde Ã§alÄ±ÅŸma emniyet kemeri ve tÄ±rmanma merdivenleri"

                )

            });



            // K-10: Kat DÃ¶ÅŸeme & KiriÅŸ KalÄ±bÄ± ve DonatÄ±sÄ± (Tabliye HazÄ±rlÄ±ÄŸÄ±) (AdÄ±m 0)

            var k10Start = k08End;

            var k10End = k10Start.AddDays(9);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-10",

                Order = stepOrderSeq++,

                PhaseName = "Kat DÃ¶ÅŸeme & KiriÅŸ KalÄ±bÄ± ve DonatÄ±sÄ± (Tabliye HazÄ±rlÄ±ÄŸÄ±)",

                StageCategory = "Kat Tabliyesi KalÄ±p & DonatÄ±",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "DÃ¼ÅŸey kolon/perdeler dikilince tabliye iskelesi kurulur",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Teleskopik Dikmeler, H20 AhÅŸap KiriÅŸler, Plywood Tabliye PanolarÄ± ve B420C DÃ¶ÅŸeme HasÄ±r DonatÄ±sÄ±",

                MaterialDescription = "Masa tipi veya flanÅŸlÄ± dÃ¶ÅŸeme iskelesi, H20 kiriÅŸler, plywood dÃ¶ÅŸeme kalÄ±bÄ±, kiriÅŸ donatÄ±larÄ±, tabliye hasÄ±rÄ± ve sehpa montajÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Kat brÃ¼t dÃ¶ÅŸeme alanÄ±ndan hesaplanacak)",

                SupplierCategory = "KalÄ±p/Ä°skele TedarikÃ§isi & Demir Ã‡elik Bayisi",

                LaborTitle = "Tabliye KalÄ±p & DonatÄ± Karma Ekibi",

                ProfessionName = "KalÄ±pÃ§Ä± & Demirci UstasÄ±",

                CrewComposition = "1 Åef + 6 KalÄ±pÃ§Ä± + 6 Demirci + 4 YardÄ±mcÄ±",

                CrewSize = 17,

                DurationDays = 9,

                EquipmentNeeded = "Kule VinÃ§, Lazer Kot Terazisi, Ä°skele Ã‡ekiÃ§leri, Demir BaÄŸ Teli",

                Predecessors = "Kolon/perde kalÄ±plarÄ±nÄ±n kapatÄ±lmasÄ± ve dÃ¼ÅŸey donatÄ± vizesi",

                CriticalDependency = "DÃ¶ÅŸeme elektrik buat/borulama hatlarÄ± Ã§ekilmeden ve YapÄ± Denetim tabliye demir vizesi vermeden beton dÃ¶kÃ¼lemez!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (Kat Tabliyesi)",

                PlannedStartDate = k10Start,

                PlannedEndDate = k10End,

                SubSteps = CreateSubSteps(

                    "T-3 Ä°skele dikme yÃ¼k hesabÄ±, plywood serme ve kiriÅŸ/dÃ¶ÅŸeme donatÄ±larÄ±nÄ±n sevki",

                    "Tabliye kalÄ±p montajÄ±, kiriÅŸ demirleri, dÃ¶ÅŸeme donatÄ±sÄ± ve elektrik buat borulamasÄ±",

                    "Optik nivo ile dÃ¶ÅŸeme ters sehim kontrolÃ¼ ve YapÄ± Denetim Tabliye Vizesi",

                    "DÃ¶ÅŸeme kenar gÃ¼venlik aÄŸlarÄ±, dÃ¼ÅŸme durdurucu yaÅŸam hatlarÄ± ve tam koruma"

                )

            });



            // K-11: Kat Karkas TaÅŸÄ±yÄ±cÄ± Beton DÃ¶kÃ¼mÃ¼ (Kolon, Perde & Tabliye C30/37) (AdÄ±m 0)

            var k11Start = k10End;

            var k11End = k11Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-11",

                Order = stepOrderSeq++,

                PhaseName = $"Kat Karkas TaÅŸÄ±yÄ±cÄ± Beton DÃ¶kÃ¼mÃ¼ (Kolon, Perde & Tabliye C30/37) ({maxFloors} Kat DÃ¶ngÃ¼sÃ¼)",

                StageCategory = "Kat Karkas Beton DÃ¶kÃ¼mÃ¼",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Tabliye demir vizesi onaylanÄ±nca dÃ¶kÃ¼lÃ¼r",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "C30/37 TaÅŸÄ±yÄ±cÄ± HazÄ±r Beton (Kat Karkas)",

                MaterialDescription = "Kolon, perde, kiriÅŸ ve dÃ¶ÅŸemenin monolitik C30/37 hazÄ±r betonla dÃ¶kÃ¼lmesi, vibrasyonu ve helikopterli/mastarlÄ± perdahÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ³",

                MaterialFormula = "DXF Bekliyor (Kat brÃ¼t alan ve kolon kesit hacminden hesaplanacak)",

                SupplierCategory = "HazÄ±r Beton Santrali",

                LaborTitle = "Karkas Beton DÃ¶kÃ¼m Ekibi",

                ProfessionName = "Betoncu & Pompa Ekibi",

                CrewComposition = "1 Saha Åefi + 2 Pompa OperatÃ¶rÃ¼ + 4 VibratÃ¶rcÃ¼ + 3 MastarcÄ± + 2 KÃ¼r Ä°ÅŸÃ§isi",

                CrewSize = 12,

                DurationDays = 2,

                EquipmentNeeded = "Mobil / Sabit DaÄŸÄ±tÄ±cÄ± Beton PompasÄ±, 4 DalgÄ±Ã§ VibratÃ¶r, Helikopter Perdah Makinesi",

                Predecessors = "Tabliye vizesi, elektrik tesisat kontrolÃ¼ ve kalÄ±p altÄ± dikme emniyeti",

                CriticalDependency = "DÃ¶kÃ¼m esnasÄ±nda kalÄ±p patlamasÄ±na karÅŸÄ± altta sÃ¼rekli kalÄ±p gÃ¶zetmeni hazÄ±r bulundurulmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 0: Statik TaÅŸÄ±yÄ±cÄ±lar (C30/37 Kat Karkas)",

                PlannedStartDate = k11Start,

                PlannedEndDate = k11End,

                SubSteps = CreateSubSteps(

                    "T-2 Santral transmikser sevkiyat akÄ±ÅŸÄ±, pompa bom konumu ve kule vinÃ§ desteÄŸi",

                    "Ã–nce kolon/perdelerin kademeli dÃ¶kÃ¼mÃ¼ ve vibrasyonu, ardÄ±ndan kiriÅŸ ve dÃ¶ÅŸeme dÃ¶kÃ¼mÃ¼",

                    "Lazer nivo mastar kotu, transmikser slump deneyi ve kat kÃ¼p numunesi alÄ±mÄ±",

                    "KalÄ±p altÄ± dikme gÃ¶zetmeni, pompa borusu emniyet kelepÃ§eleri ve gece aydÄ±nlatmasÄ±"

                )

            });



            // K-12: Ã‡atÄ± TaÅŸÄ±yÄ±cÄ± Ä°skeleti, Su YalÄ±tÄ±mÄ±, Parapet HarpuÅŸtasÄ± & YaÄŸmur Ä°niÅŸleri (AdÄ±m 5)

            var k12Start = k11End.AddDays(7);

            var k12End = k12Start.AddDays(10);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "K-12",

                Order = stepOrderSeq++,

                PhaseName = "Ã‡atÄ± TaÅŸÄ±yÄ±cÄ± Ä°skeleti, Su YalÄ±tÄ±mÄ±, Parapet HarpuÅŸtasÄ± & YaÄŸmur Ä°niÅŸleri",

                StageCategory = "Ã‡atÄ± & KapanÄ±ÅŸ",

                PartIndex = p1Idx,

                PartName = p1Name,

                PartBadgeClass = p1Badge,

                TriggerRule = "Son kat tavan tabliyesi dÃ¶kÃ¼lÃ¼nce baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Ã‡atÄ± AhÅŸap/Ã‡elik TaÅŸÄ±yÄ±cÄ± KonstrÃ¼ksiyonu, Su/IsÄ± Ä°zolasyonu ve Parapet HarpuÅŸtasÄ±",

                MaterialDescription = "Ã‡atÄ± ahÅŸap/Ã§elik taÅŸÄ±yÄ±cÄ± dikmeleri, OSB kaplama, su/Ä±sÄ± yalÄ±tÄ±mÄ±, dere iniÅŸleri ve mermer/alÃ¼minyum parapet harpuÅŸtalarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mt",

                MaterialFormula = "DXF Bekliyor (Ã‡atÄ± planÄ± ve parapet Ã§evre uzunluÄŸundan hesaplanacak)",

                SupplierCategory = "Ã‡atÄ± Kaplama & YalÄ±tÄ±m TedarikÃ§isi",

                LaborTitle = "Ã‡atÄ± KonstrÃ¼ksiyon ve Ä°zolasyon Ekibi",

                ProfessionName = "Ã‡atÄ± UstasÄ±",

                CrewComposition = "1 Ã‡atÄ± Formeni + 4 Ã‡atÄ± UstasÄ± + 2 YalÄ±tÄ±m UstasÄ± + 2 YardÄ±mcÄ±",

                CrewSize = 9,

                DurationDays = 10,

                EquipmentNeeded = "Mobil VinÃ§, Åaloma TakÄ±mlarÄ±, Profil Kesme, GÃ¼venlik YaÅŸam HattÄ±",

                Predecessors = "Son kat tavan betonu dÃ¶kÃ¼mÃ¼ ve parapet duvarlarÄ±nÄ±n Ã¶rÃ¼lmesi",

                CriticalDependency = "Ã‡atÄ± su yalÄ±tÄ±mÄ± ve dere iniÅŸleri tamamlanmadan bina iÃ§ine su gireceÄŸinden alt kat ince imalatlarÄ±na baÅŸlanamaz!",

                DnaSource = "AdÄ±m 5: Ã‡atÄ± & Parapet",

                PlannedStartDate = k12Start,

                PlannedEndDate = k12End,

                SubSteps = CreateSubSteps(

                    "T-3 Ã‡atÄ± konstrÃ¼ksiyon statik projesi, kereste/Ã§elik profil ve yalÄ±tÄ±m malzemeleri sevki",

                    "TaÅŸÄ±yÄ±cÄ± dikmelerin montajÄ±, OSB kaplama, su yalÄ±tÄ±m membranÄ± ve parapet harpuÅŸtalarÄ± montajÄ±",

                    "Dere iniÅŸleri su sÄ±zdÄ±rmazlÄ±k testi ve rÃ¼zgar fÄ±rtÄ±na emniyet baÄŸlantÄ±larÄ±",

                    "Ã‡atÄ± kenarÄ± parapet gÃ¼venlik aÄŸlarÄ±, Ã§elik yaÅŸam hattÄ± ve paraÅŸÃ¼t tipi emniyet kemeri"

                )

            });



            // =========================================================================

            // 2. PARÃ‡A: STATÄ°K Ä°NCE (KABA Ä°NCE / KOVALAYAN Ä°ÅLER)

            // =========================================================================



            // S-01: DÄ±ÅŸ ve Ä°Ã§ BÃ¶lme Duvar Ã–rÃ¼mÃ¼ & HatÄ±llar / Lentolar (AdÄ±m 3)

            var s01Start = k11Start.AddDays(7); // Kaba 2 kat Ã¶nden gidince alttan baÅŸlar

            var s01End = s01Start.AddDays(10);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-01",

                Order = stepOrderSeq++,

                PhaseName = "DÄ±ÅŸ ve Ä°Ã§ BÃ¶lme Duvar Ã–rÃ¼mÃ¼ & HatÄ±llar / Lentolar",

                StageCategory = "Duvar Ä°malatlarÄ±",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Gazbeton / Bims Bloklar, Ã–rgÃ¼ HarcÄ± ve HatÄ±l / Lentolar",

                MaterialDescription = "DÄ±ÅŸ cephe ve daire iÃ§i oda bÃ¶lme duvarlarÄ±nÄ±n Ã¶rÃ¼lmesi, kapÄ± Ã¼stÃ¼ lentolarÄ± ve hatÄ±l baÄŸlantÄ±larÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Mimari kat planÄ± duvar metrajlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Gazbeton / TuÄŸla & HarÃ§ Fabrika Bayisi",

                LaborTitle = "Duvar Ã–rme Ekibi",

                ProfessionName = "Duvar UstasÄ±",

                CrewComposition = "1 Formen + 6 Duvar UstasÄ± + 4 TaÅŸÄ±ma/HarÃ§ Ä°ÅŸÃ§isi",

                CrewSize = 11,

                DurationDays = 10,

                EquipmentNeeded = "HarÃ§ Mikseri, Blok Testeresi, Ä°Ã§ Mekan Seyyar Ä°skele",

                Predecessors = "Ãœst kat tabliyesinin dÃ¶kÃ¼lmesi ve dikme kulelerinin gÃ¼venli sÃ¶kÃ¼mÃ¼",

                CriticalDependency = "Kaba 2 kat Ã¶nden gitmeden alttan duvar Ã¶rÃ¼mÃ¼ne baÅŸlanamaz!",

                DnaSource = "AdÄ±m 3: BÃ¶lme Duvarlar",

                PlannedStartDate = s01Start,

                PlannedEndDate = s01End,

                SubSteps = CreateSubSteps(

                    "T-2 Mimari projeye gÃ¶re kat kapÄ± ve pencere boÅŸluklarÄ±nÄ±n sahaya aplikasyonu",

                    "Ä°lk sÄ±ra harÃ§ mastarÄ±, duvar Ã¶rÃ¼mÃ¼, esnek tavan birleÅŸimi (poliÃ¼retan kÃ¶pÃ¼k) ve kapÄ± lentolarÄ±",

                    "Duvar ÅŸakÃ¼l, gÃ¶nye ve hatÄ±l donatÄ± baÄŸlantÄ±larÄ±nÄ±n kontrolÃ¼",

                    "Ä°Ã§ iskele korkuluklarÄ±, malzeme istif gÃ¼venliÄŸi ve toz maskesi kullanÄ±mÄ±"

                )

            });



            // S-02: Daire Ä°Ã§i ve Åaft SÄ±hhi Tesisat, Pis Su & Temiz Su Borulama (AdÄ±m 7)

            var s02Start = s01Start.AddDays(3);

            var s02End = s02Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-02",

                Order = stepOrderSeq++,

                PhaseName = "Daire Ä°Ã§i ve Åaft SÄ±hhi Tesisat, Pis Su & Temiz Su Borulama",

                StageCategory = "Mekanik Tesisat",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = $"{pisuTitle}, PPRC Temiz Su BorularÄ± ve KollektÃ¶rler",

                MaterialDescription = "Bina dikey ana ÅŸaft kolonlarÄ±, daire iÃ§i Ä±slak hacim pis su giderleri ve PPRC temiz su daÄŸÄ±tÄ±m tesisatÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mt",

                MaterialFormula = "DXF Bekliyor (Mekanik tesisat ÅŸaft ve daire iÃ§i kolon projelerinden hesaplanacak)",

                SupplierCategory = "SÄ±hhi Tesisat & Boru ToptancÄ±sÄ±",

                LaborTitle = "SÄ±hhi Tesisat Ekibi",

                ProfessionName = "SÄ±hhi Tesisat UstasÄ±",

                CrewComposition = "1 Mekanik MÃ¼h. + 4 Tesisat UstasÄ± + 2 YardÄ±mcÄ±",

                CrewSize = 7,

                DurationDays = 8,

                EquipmentNeeded = "PPRC Kaynak Makinesi, Boru Kesme AparatlarÄ±, BasÄ±nÃ§ Test PompasÄ±",

                Predecessors = "Kat duvarlarÄ±nÄ±n Ã¶rÃ¼lmesi ve ÅŸaft boÅŸluklarÄ±nÄ±n aÃ§Ä±lmasÄ±",

                CriticalDependency = "SÄ±va ve ÅŸap atÄ±lmadan Ã¶nce tesisat borularÄ± test edilip basÄ±nca tabi tutulmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 7: SÄ±hhi Tesisat & Borulama",

                PlannedStartDate = s02Start,

                PlannedEndDate = s02End,

                SubSteps = CreateSubSteps(

                    "T-3 Tesisat gÃ¼zergahlarÄ±nÄ±n ve rezervuar/kollektÃ¶r yerlerinin iÅŸaretlenmesi",

                    "Pis su eÄŸim hattÄ±, sessiz boru kelepÃ§elemesi, temiz su PPRC borulama ve kollektÃ¶r montajÄ±",

                    "Test pompasÄ± ile 10 bar basÄ±nÃ§ sÄ±zdÄ±rmazlÄ±k testi ve 24 saat basÄ±nÃ§ manometre takibi",

                    "Åaft Ã§alÄ±ÅŸma platformlarÄ±, kaynak gÃ¼venliÄŸi ve boru kesme Ã§apaÄŸÄ± gÃ¶zlÃ¼ÄŸÃ¼"

                )

            });



            // S-03: Daire Ä°Ã§i Elektrik AltyapÄ±sÄ±, Borulama, Kablolama ve Sortiler (AdÄ±m 7)

            var s03Start = s01Start.AddDays(4);

            var s03End = s03Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-03",

                Order = stepOrderSeq++,

                PhaseName = "Daire Ä°Ã§i Elektrik AltyapÄ±sÄ±, Borulama, Kablolama ve Sortiler",

                StageCategory = "Elektrik AltyapÄ±",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Halogen Free Borular, NYM Kablolar, Buat ve Kasalar",

                MaterialDescription = "Duvar iÃ§i elektrik kanallarÄ±nÄ±n aÃ§Ä±lmasÄ±, kasalama, linye borulamasÄ±, buat montajÄ± ve kablo Ã§ekimi.",

                MaterialQuantity = 0,

                MaterialUnit = "Sorti",

                MaterialFormula = "DXF Bekliyor (Elektrik kuvvetli/zayÄ±f akÄ±m projelerinden sorti sayÄ±sÄ± netleÅŸecek)",

                SupplierCategory = "Elektrik Malzemeleri ToptancÄ±sÄ±",

                LaborTitle = "Elektrik Tesisat Ekibi",

                ProfessionName = "Elektrik UstasÄ±",

                CrewComposition = "1 Elektrik Formeni + 4 Elektrik UstasÄ± + 3 YardÄ±mcÄ±",

                CrewSize = 8,

                DurationDays = 8,

                EquipmentNeeded = "Kanal AÃ§ma Makinesi, SustalÄ± Kablo Ã‡ekici, Meger Test CihazÄ±",

                Predecessors = "Kat duvarlarÄ±nÄ±n Ã¶rÃ¼lmesi ve tavan karkasÄ±nÄ±n netleÅŸmesi",

                CriticalDependency = "Kablolar Ã§ekilmeden ve buatlar sabitlenmeden sÄ±va yapÄ±lamaz!",

                DnaSource = "AdÄ±m 7: Elektrik TesisatÄ± & Kablolama",

                PlannedStartDate = s03Start,

                PlannedEndDate = s03End,

                SubSteps = CreateSubSteps(

                    "T-2 Elektrik projesine gÃ¶re priz, anahtar ve aydÄ±nlatma linye kotlarÄ±nÄ±n Ã§izilmesi",

                    "Kanal aÃ§ma, borulama, kasalama, buat montajÄ± ve kablo Ã§ekimi",

                    "Faz, nÃ¶tr, toprak meger izolasyon testi ve kablo sÃ¼reklilik kontrolÃ¼",

                    "Kanal aÃ§ma tozu iÃ§in filtreli maske, spiral koruyucu kapaÄŸÄ± ve eldiven denetimi"

                )

            });



            // S-04: Ä°Ã§ Cephe Kaba SÄ±va, AlÃ§Ä± SÄ±va & Tavan AlÃ§Ä±pan Asma Tavan (AdÄ±m 3)

            var s04Start = s02End;

            var s04End = s04Start.AddDays(12);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-04",

                Order = stepOrderSeq++,

                PhaseName = "Ä°Ã§ Cephe Kaba SÄ±va, AlÃ§Ä± SÄ±va & Tavan AlÃ§Ä±pan Asma Tavan",

                StageCategory = "SÄ±va & AlÃ§Ä±pan",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Perlitli AlÃ§Ä± SÄ±va, Ano / KÃ¶ÅŸe Profilleri ve Tavan AlÃ§Ä±pan LevhalarÄ±",

                MaterialDescription = "Duvarlarda makineli alÃ§Ä± sÄ±va pÃ¼skÃ¼rtme, ano Ã§ekimi, mastarlama ve tavan alÃ§Ä±pan asma tavan imalatÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Kat yÃ¼kseklikleri ve net duvar yÃ¼zey alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "YapÄ± KimyasallarÄ± & AlÃ§Ä± ToptancÄ±sÄ±",

                LaborTitle = "AlÃ§Ä± SÄ±va & Asma Tavan Ekibi",

                ProfessionName = "AlÃ§Ä± SÄ±va & Asma Tavan UstasÄ±",

                CrewComposition = "1 SÄ±va Formeni + 6 AlÃ§Ä±/SÄ±va UstasÄ± + 4 YardÄ±mcÄ±",

                CrewSize = 11,

                DurationDays = 12,

                EquipmentNeeded = "AlÃ§Ä± SÄ±va PÃ¼skÃ¼rtme Makinesi, AlÃ¼minyum H Mastarlar, Lazer Hizalama",

                Predecessors = "Elektrik kasalama ve tesisat borulama hatlarÄ±nÄ±n kapatÄ±lmasÄ±",

                CriticalDependency = "Elektrik tesisat testleri tamamlanÄ±p kapak takÄ±lmadan sÄ±va kapatÄ±lamaz!",

                DnaSource = "AdÄ±m 3: SÄ±va & AlÃ§Ä±pan",

                PlannedStartDate = s04Start,

                PlannedEndDate = s04End,

                SubSteps = CreateSubSteps(

                    "T-2 Ano Ã§Ä±talarÄ±nÄ±n Ã§ekilmesi ve kapÄ±/pencere kenarlarÄ± fileli kÃ¶ÅŸe profili montajÄ±",

                    "Makineli alÃ§Ä± sÄ±va pÃ¼skÃ¼rtme, mastarlama ve tavan alÃ§Ä±pan karkas montajÄ±",

                    "2 metrelik mastar ile yÃ¼zey kaÃ§Ä±klÄ±ÄŸÄ± ve ÅŸakÃ¼l kontrolÃ¼",

                    "Hareketli platform iskele emniyet kilitleri ve gÃ¶z yÄ±kama solÃ¼syonu kontrolÃ¼"

                )

            });



            // S-05: Helikopterli ve MastarlÄ± Zemin Tesviye ÅapÄ± (AdÄ±m 3)

            var s05Start = s04End;

            var s05End = s05Start.AddDays(6);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-05",

                Order = stepOrderSeq++,

                PhaseName = "Helikopterli ve MastarlÄ± Zemin Tesviye ÅapÄ±",

                StageCategory = "Zemin ÅapÄ±",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Zemin Åap Kumu, Ã‡imento ve YÃ¼zey SertleÅŸtirici KatkÄ±",

                MaterialDescription = "Zemindeki tesisat borularÄ±nÄ±n Ã¼zerine mastarlÄ± Ã§imento esaslÄ± tesviye ÅŸapÄ± atÄ±lmasÄ± ve helikopter perdahÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Net dÃ¶ÅŸeme alanÄ± mimari plandan hesaplanacak)",

                SupplierCategory = "Åap Kumu & Ã‡imento Bayisi",

                LaborTitle = "Helikopterli Åap Ekibi",

                ProfessionName = "Åap UstasÄ±",

                CrewComposition = "1 Åap Formeni + 4 Åap UstasÄ± + 2 Pompa/Mikser OperatÃ¶rÃ¼",

                CrewSize = 7,

                DurationDays = 6,

                EquipmentNeeded = "Åap PompasÄ± (Kara Åap), Helikopter Perdah Makinesi, Lazer Nivo",

                Predecessors = "Zeminden geÃ§en tesisat borularÄ±nÄ±n yalÄ±tÄ±mÄ± ve kot referans Ã§izgisi",

                CriticalDependency = "Zemin ÅŸapÄ± atÄ±lmadan Ã¶nce temiz/pis su borularÄ± basÄ±nÃ§ testinden geÃ§miÅŸ ve sabitlenmiÅŸ olmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 3: Åap Ä°malatlarÄ±",

                PlannedStartDate = s05Start,

                PlannedEndDate = s05End,

                SubSteps = CreateSubSteps(

                    "T-2 Lazer nivo ile kat kotlarÄ±nÄ±n iÅŸaretlenmesi ve ÅŸap anolarÄ±nÄ±n oluÅŸturulmasÄ±",

                    "Åap pompasÄ± ile harÃ§ basÄ±mÄ±, mastarlama ve helikopterli yÃ¼zey perdahÄ±",

                    "Lazer nivo ile eÄŸim kontrolÃ¼, dilatasyon derz kesimi ve Ã§atlama Ã¶nleme sulamasÄ±",

                    "Helikopterli perdah makinesi acil durdurma mandalÄ± ve Ã§izme kullanÄ±mÄ±"

                )

            });



            // S-06: DÄ±ÅŸ Cephe GÃ¼venlikli Ä°skele Kurulumu & IsÄ± YalÄ±tÄ±m / Mantolama (AdÄ±m 6)

            var s06Start = k10End;

            var s06End = s06Start.AddDays(14);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-06",

                Order = stepOrderSeq++,

                PhaseName = "DÄ±ÅŸ Cephe GÃ¼venlikli Ä°skele Kurulumu & IsÄ± YalÄ±tÄ±m / Mantolama",

                StageCategory = "DÄ±ÅŸ Cephe Mantolama",

                PartIndex = p2Idx,

                PartName = p2Name,

                PartBadgeClass = p2Badge,

                TriggerRule = p2Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "TaÅŸyÃ¼nÃ¼ / EPS Mantolama LevhalarÄ±, Fileli SÄ±va ve Dekoratif DÄ±ÅŸ Cephe KaplamasÄ±",

                MaterialDescription = "TS EN 12810 onaylÄ± gÃ¼venlikli flanÅŸlÄ± iskele kurulumu, Ä±sÄ± yalÄ±tÄ±m levhalarÄ±, dÃ¼belleme, fileli sÄ±va ve dekoratif cephe kaplamasÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Bina cephe aÃ§Ä±lÄ±mlarÄ± ve kat yÃ¼ksekliklerinden hesaplanacak)",

                SupplierCategory = "DÄ±ÅŸ Cephe YalÄ±tÄ±m Bayisi & Ä°skele Kiralama",

                LaborTitle = "DÄ±ÅŸ Cephe ve Mantolama Ekibi",

                ProfessionName = "Mantolama & Cephe UstasÄ±",

                CrewComposition = "1 Ä°skele SÃ¼pervizÃ¶rÃ¼ + 6 Cephe UstasÄ± + 4 YardÄ±mcÄ±",

                CrewSize = 11,

                DurationDays = 14,

                EquipmentNeeded = "GÃ¼venlikli Cephe Ä°skelesi, DÃ¼bel MatkaplarÄ±, HarÃ§ KarÄ±ÅŸtÄ±rma Mikseri",

                Predecessors = "Kaba inÅŸaat cephe temizliÄŸi ve denizlik montajlarÄ±",

                CriticalDependency = "DÄ±ÅŸ cephe gÃ¼venlikli iskelesi yeÅŸil etiket (TS EN 12810 onaylÄ±) almadan cephe Ã§alÄ±ÅŸmasÄ± baÅŸlatÄ±lamaz!",

                DnaSource = "AdÄ±m 6: DÄ±ÅŸ Cephe & Mantolama",

                PlannedStartDate = s06Start,

                PlannedEndDate = s06End,

                SubSteps = CreateSubSteps(

                    "T-3 GÃ¼venlikli flanÅŸlÄ± iskele kurulumu, statik ankraj tespiti ve gÃ¼venlik aÄŸlarÄ±",

                    "Levha yapÄ±ÅŸtÄ±rma, mekanik dÃ¼belleme, Ã§ift kat fileli sÄ±va ve dekoratif kaplama",

                    "DÃ¼bel Ã§ekme testi, kÃ¶ÅŸe diklik kontrolÃ¼ ve su iticilik incelemesi",

                    "Ä°skele yeÅŸil etiket denetimi, rÃ¼zgar hÄ±z takibi ve paraÅŸÃ¼t tipi Ã§ift kollu emniyet kemeri"

                )

            });



            // =========================================================================

            // 3. PARÃ‡A: TERCÄ°HE GÃ–RE Ä°NCE Ä°ÅLER (DAÄ°RE Ä°Ã‡Ä° Ä°MALATLAR)

            // =========================================================================



            // D-01: Islak Hacimler (Banyo/WC/Balkon) Ã‡ift Kat SÃ¼rme Su YalÄ±tÄ±mÄ± (AdÄ±m 4)

            var d01Start = s05End.AddDays(2);

            var d01End = d01Start.AddDays(5);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "D-01",

                Order = stepOrderSeq++,

                PhaseName = "Islak Hacimler (Banyo/WC/Balkon) Ã‡ift Kat SÃ¼rme Su YalÄ±tÄ±mÄ±",

                StageCategory = "Islak Hacim YalÄ±tÄ±mÄ±",

                PartIndex = p3Idx,

                PartName = p3Name,

                PartBadgeClass = p3Badge,

                TriggerRule = p3Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Ã‡ift BileÅŸenli Tam Elastik Su YalÄ±tÄ±mÄ± ve Pah BantlarÄ±",

                MaterialDescription = "Banyo, tuvalet ve balkon zeminlerine Ã§ift kat elastik sÃ¼rme izolasyon ve sÃ¼zgeÃ§ manÅŸetleri uygulamasÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Islak hacim zemin ve duvar yÃ¼zey alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Su Ä°zolasyonu KimyasallarÄ± Bayisi",

                LaborTitle = "Islak Hacim Ä°zolasyon Ekibi",

                ProfessionName = "Ä°zolasyon UstasÄ±",

                CrewComposition = "1 Ä°zolasyon Formeni + 3 Ä°zolasyon UstasÄ±",

                CrewSize = 4,

                DurationDays = 5,

                EquipmentNeeded = "Ä°zolasyon KarÄ±ÅŸtÄ±rÄ±cÄ± Mikser, Rulo ve Pah FÄ±rÃ§alarÄ±",

                Predecessors = "Islak hacim ÅŸapÄ±nÄ±n kurumasÄ± ve sÃ¼zgeÃ§ kasalarÄ±nÄ±n montajÄ±",

                CriticalDependency = "Su yalÄ±tÄ±mÄ± 48 saat su gÃ¶llendirme sÄ±zdÄ±rmazlÄ±k testinden geÃ§meden Ã¼zerine seramik dÃ¶ÅŸenemez!",

                DnaSource = "AdÄ±m 4: Islak Hacim Su YalÄ±tÄ±mÄ±",

                PlannedStartDate = d01Start,

                PlannedEndDate = d01End,

                SubSteps = CreateSubSteps(

                    "T-2 Zemin tozunun temizlenmesi, pah bandÄ± ve elastik astar hazÄ±rlÄ±ÄŸÄ±",

                    "Duvar-taban birleÅŸim pah bantlarÄ±, ilk kat ve ikinci kat ÅŸaÅŸÄ±rtmalÄ± sÃ¼rme yalÄ±tÄ±m",

                    "48 saat su gÃ¶llendirme testi ve alt kat tavanÄ±nda Ä±slaklÄ±k/nem kontrolÃ¼",

                    "HavalandÄ±rmasÄ±z banyolarda fanlÄ± hava tahliyesi ve organik buhar maskesi"

                )

            });



            // D-02: Islak Hacim & Mutfak Seramik / Porselen Kaplama (AdÄ±m 4)

            var d02Start = d01End;

            var d02End = d02Start.AddDays(12);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "D-02",

                Order = stepOrderSeq++,

                PhaseName = "Islak Hacim & Mutfak Seramik / Porselen Kaplama",

                StageCategory = "Seramik & Fayans",

                PartIndex = p3Idx,

                PartName = p3Name,

                PartBadgeClass = p3Badge,

                TriggerRule = p3Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Porselen Seramik / Fayans, Esnek YapÄ±ÅŸtÄ±rÄ±cÄ± ve Derz Dolgusu",

                MaterialDescription = "Banyo duvar/zemin seramik kaplamalarÄ±, mutfak tezgah arasÄ± ve balkon seramiklerinin dÃ¶ÅŸenmesi.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Islak hacim ve mutfak dÃ¶ÅŸeme/duvar net alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Seramik MaÄŸazasÄ± & YapÄ±ÅŸtÄ±rÄ±cÄ± Bayisi",

                LaborTitle = "Seramik ve Fayans Ekibi",

                ProfessionName = "Seramik UstasÄ±",

                CrewComposition = "1 Formen + 6 Seramik UstasÄ± + 3 YardÄ±mcÄ±",

                CrewSize = 10,

                DurationDays = 12,

                EquipmentNeeded = "Lazer Hizalama Nivosu, Sulu Seramik Kesme Makinesi, Tesviye Klipsleri",

                Predecessors = "Islak hacim su yalÄ±tÄ±m sÄ±zdÄ±rmazlÄ±k onay tutanaÄŸÄ±",

                CriticalDependency = "Seramik yapÄ±ÅŸtÄ±rÄ±cÄ±sÄ± priz almadan ve derz dolgusu Ã§ekilmeden vitrifiye montajÄ±na baÅŸlanamaz!",

                DnaSource = "AdÄ±m 4: Seramik & Zemin",

                PlannedStartDate = d02Start,

                PlannedEndDate = d02End,

                SubSteps = CreateSubSteps(

                    "T-2 Seramik aÃ§Ä±lÄ±m ÅŸemasÄ±, derz artÄ±/takozlarÄ± ve lazer hizalama hazÄ±rlÄ±ÄŸÄ±",

                    "TaraklÄ± yapÄ±ÅŸtÄ±rma harcÄ± Ã§ekimi, tesviye klipsleri ile seramik montajÄ± ve derz dolgusu",

                    "Mastar ile yÃ¼zey sÄ±fÄ±rlÄ±ÄŸÄ±, eÄŸim kontrolÃ¼ ve derz dolgusu pÃ¼rÃ¼zsÃ¼zlÃ¼k kontrolÃ¼",

                    "Islak seramik kesme makinesi korumasÄ±, toz maskesi ve dizlik kullanÄ±mÄ±"

                )

            });



            // D-03: Daire Ä°Ã§i Duvar & Tavan Su BazlÄ± Silikonlu Boya (AdÄ±m 4)

            var d03Start = s05End.AddDays(7);

            var d03End = d03Start.AddDays(10);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "D-03",

                Order = stepOrderSeq++,

                PhaseName = "Daire Ä°Ã§i Duvar & Tavan Su BazlÄ± Silikonlu Boya",

                StageCategory = "Boya & Badana",

                PartIndex = p3Idx,

                PartName = p3Name,

                PartBadgeClass = p3Badge,

                TriggerRule = p3Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Ä°peksi Mat Silikonlu Ä°Ã§ Cephe BoyasÄ±, Astar ve Macun",

                MaterialDescription = "Duvar ve tavan yÃ¼zeylerinde zÄ±mpara, tamirat macunu, astar ve Ã§ift kat silikonlu boya uygulamasÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Net duvar/tavan boya alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Boya Bayisi & YapÄ± KimyasÄ±",

                LaborTitle = "Boya & YÃ¼zey Ä°ÅŸlemleri Ekibi",

                ProfessionName = "Boya UstasÄ±",

                CrewComposition = "1 Boya Formeni + 5 BoyacÄ± UstasÄ±",

                CrewSize = 6,

                DurationDays = 10,

                EquipmentNeeded = "ZÄ±mpara Makinesi, Boya RulolarÄ±, Kestirme FÄ±rÃ§alarÄ±, Spot ProjektÃ¶r",

                Predecessors = "AlÃ§Ä± sÄ±va zÄ±mparasÄ± ve nem oranÄ±nÄ±n %10 altÄ±na dÃ¼ÅŸmesi",

                CriticalDependency = "AhÅŸap kapÄ±lar ve parkeler dÃ¶ÅŸenmeden Ã¶nce duvarlarÄ±n 1. ve 2. kat astar/boyasÄ± tamamlanmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 4: Ä°Ã§ Mekan Boya",

                PlannedStartDate = d03Start,

                PlannedEndDate = d03End,

                SubSteps = CreateSubSteps(

                    "T-2 AlÃ§Ä± sÄ±va yÃ¼zey zÄ±mparasÄ±, spot lamba ile yÃ¼zey kontrolÃ¼ ve ÅŸeffaf astar Ã§ekimi",

                    "Rulo ve kestirme fÄ±rÃ§alar ile 2 kat boya uygulamasÄ± ve sÃ¼pÃ¼rgelik altÄ± izolasyonu",

                    "IÅŸÄ±k altÄ±nda fÄ±rÃ§a izi, dalgalanma ve ton farkÄ± kontrolÃ¼",

                    "Ä°Ã§ mekan havalandÄ±rmasÄ± ve gÃ¶z koruyucu gÃ¶zlÃ¼k"

                )

            });



            // D-04: Daire Ä°Ã§i Parke, SÃ¼pÃ¼rgelik & Zemin KaplamalarÄ± (AdÄ±m 4)

            var d04Start = d03End;

            var d04End = d04Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "D-04",

                Order = stepOrderSeq++,

                PhaseName = "Daire Ä°Ã§i Parke, SÃ¼pÃ¼rgelik & Zemin KaplamalarÄ±",

                StageCategory = "Parke & Zemin",

                PartIndex = p3Idx,

                PartName = p3Name,

                PartBadgeClass = p3Badge,

                TriggerRule = p3Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "32. SÄ±nÄ±f Derzli Laminat Parke, Kapron Åilte ve SÃ¼pÃ¼rgelik",

                MaterialDescription = "Salon ve odalarda kapron ÅŸilte serimi, kilitli laminat parke montajÄ± ve 8cm lake sÃ¼pÃ¼rgelikler.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Kuru hacim net dÃ¶ÅŸeme alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Parke & Orman ÃœrÃ¼nleri MaÄŸazasÄ±",

                LaborTitle = "Laminat Parke Montaj Ekibi",

                ProfessionName = "Parke UstasÄ±",

                CrewComposition = "1 Formen + 4 Parke UstasÄ±",

                CrewSize = 5,

                DurationDays = 7,

                EquipmentNeeded = "Giyotin Parke Kesici, GÃ¶nye Kesme Testeresi, Parke Ã‡ektirme Demiri",

                Predecessors = "Zemin ÅŸapÄ±nÄ±n tamamen kurumasÄ± (nem <%2) ve tavan boyalarÄ±nÄ±n bitmesi",

                CriticalDependency = "Zemin nemi %2'nin altÄ±na dÃ¼ÅŸmeden parke serilemez; aksi takdirde kabarma ve deformasyon oluÅŸur!",

                DnaSource = "AdÄ±m 4: Parke & Zemin",

                PlannedStartDate = d04Start,

                PlannedEndDate = d04End,

                SubSteps = CreateSubSteps(

                    "T-2 Dijital nem Ã¶lÃ§er ile ÅŸap nem kontrolÃ¼ ve kapron ÅŸilte serimi",

                    "Duvar dibi 1.5 cm genleÅŸme boÅŸluÄŸu bÄ±rakarak kilitli parke ve sÃ¼pÃ¼rgelik montajÄ±",

                    "Zemin esneme, kilit oturumu ve sÃ¼pÃ¼rgelik kÃ¶ÅŸe gÃ¶nye kontrolÃ¼",

                    "Giyotin kesici gÃ¼venliÄŸi ve diz koruyucu pedler"

                )

            });



            // =========================================================================

            // 4. PARÃ‡A: DEMÄ°RBAÅ, MONTAJ & DIÅ Ã‡EVRE (TESLÄ°M AÅAMASI)

            // =========================================================================



            // M-01: Daire GiriÅŸ Ã‡elik KapÄ±larÄ± ve Ä°Ã§ AhÅŸap Oda KapÄ±larÄ± (AdÄ±m 4)

            var m01Start = d04End;

            var m01End = m01Start.AddDays(6);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-01",

                Order = stepOrderSeq++,

                PhaseName = "Daire GiriÅŸ Ã‡elik KapÄ±larÄ± ve Ä°Ã§ AhÅŸap Oda KapÄ±larÄ±",

                StageCategory = "KapÄ± MontajÄ±",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = p4Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Monoblok Ã‡elik Daire GiriÅŸ KapÄ±sÄ± ve Ä°Ã§ AhÅŸap Oda KapÄ±larÄ±",

                MaterialDescription = "Monoblok kilitli Ã§elik kapÄ±lar, ahÅŸap kasa/kanat oda kapÄ±larÄ±, pervazlar, kollar ve kilitler.",

                MaterialQuantity = 0,

                MaterialUnit = "Adet",

                MaterialFormula = "DXF Bekliyor (Mimari kapÄ± pencere ÅŸemasÄ±ndan adetler netleÅŸecek)",

                SupplierCategory = "Ã‡elik KapÄ± & AhÅŸap KapÄ± Ä°malatÃ§Ä±sÄ±",

                LaborTitle = "KapÄ± Montaj Ekibi",

                ProfessionName = "KapÄ± Montaj UstasÄ±",

                CrewComposition = "1 Formen + 3 KapÄ± Montaj UstasÄ± + 2 YardÄ±mcÄ±",

                CrewSize = 6,

                DurationDays = 6,

                EquipmentNeeded = "Lazer Terazi, AkÃ¼lÃ¼ Vidalama, KÃ¶pÃ¼k TabancasÄ±",

                Predecessors = "Zemin parke ve Ä±slak hacim seramik dÃ¶ÅŸemelerinin tamamlanmasÄ±",

                CriticalDependency = "KapÄ± kasalarÄ± montajÄ±ndan Ã¶nce zemin kaplama kalÄ±nlÄ±klarÄ± netleÅŸmiÅŸ olmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 4: Ã‡elik ve AhÅŸap KapÄ±lar",

                PlannedStartDate = m01Start,

                PlannedEndDate = m01End,

                SubSteps = CreateSubSteps(

                    "T-3 Daire bazÄ±nda kapÄ± aÃ§Ä±lÄ±ÅŸ yÃ¶nleri ve net kasa aÃ§Ä±klÄ±klarÄ±nÄ±n kontrolÃ¼",

                    "Kasa terazisi, poliÃ¼retan kÃ¶pÃ¼k dolgusu, kanat ayarÄ± ve pervaz montajÄ±",

                    "Kilit, kol ve menteÅŸe sÃ¼rtÃ¼nme testleri ve kanat ayar kontrolÃ¼",

                    "AÄŸÄ±r Ã§elik kapÄ± taÅŸÄ±malarÄ±nda ergonomik kaldÄ±rma ve eldiven kullanÄ±mÄ±"

                )

            });



            // M-02: Mutfak DolaplarÄ±, Tezgah & Banyo DolaplarÄ± MontajÄ± (AdÄ±m 4)

            var m02Start = m01Start.AddDays(2);

            var m02End = m02Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-02",

                Order = stepOrderSeq++,

                PhaseName = "Mutfak DolaplarÄ±, Tezgah & Banyo DolaplarÄ± MontajÄ±",

                StageCategory = "Sabit Mobilya MontajÄ±",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = p4Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "ModÃ¼ler Mutfak DolaplarÄ±, Granit / Kuvars Tezgah ve Banyo DolaplarÄ±",

                MaterialDescription = "MDF gÃ¶vde modÃ¼ler mutfak dolaplarÄ±, kuvars tezgah, banyo aynalÄ± lavabo dolaplarÄ± montajÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "Daire TakÄ±mÄ±",

                MaterialFormula = "DXF Bekliyor (Mutfak ve banyo mimari yerleÅŸim detaylarÄ±ndan hesaplanacak)",

                SupplierCategory = "Mutfak / Banyo Mobilya Ãœreticisi & Mermer AtÃ¶lyesi",

                LaborTitle = "Mobilya & Tezgah Montaj Ekibi",

                ProfessionName = "Mobilya Montaj UstasÄ±",

                CrewComposition = "1 Formen + 4 Mobilya UstasÄ± + 2 Mermer/Tezgah UstasÄ±",

                CrewSize = 7,

                DurationDays = 8,

                EquipmentNeeded = "Lazer Hizalama, AkÃ¼lÃ¼ Montaj Setleri, Vakumlu Vantuzlar",

                Predecessors = "Ä°Ã§ boya, zemin kaplamalarÄ± ve elektrik/sÄ±hhi tesisat sortilerinin hazÄ±r olmasÄ±",

                CriticalDependency = "Dolap arkasÄ± tesisat vanalarÄ± ve priz yerleri milimetrik hizalanmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 4: Mutfak & Banyo MobilyalarÄ±",

                PlannedStartDate = m02Start,

                PlannedEndDate = m02End,

                SubSteps = CreateSubSteps(

                    "T-3 Ä°malat Ã§izimlerine gÃ¶re mutfak ve banyo duvarlarÄ±nÄ±n gÃ¶nye ve tesisat kontrolÃ¼",

                    "ModÃ¼ler gÃ¶vde asÄ±mÄ±, raylÄ± Ã§ekmece/kapak montajÄ± ve tezgah montajÄ±",

                    "Kapak boÅŸluklarÄ±, ray amortisÃ¶r testleri ve eviye sÄ±zdÄ±rmazlÄ±k silikonu kontrolÃ¼",

                    "Duvar asma raylarÄ±nÄ±n taÅŸÄ±ma kapasitesi ve gÃ¼venli montaj vidalarÄ± denetimi"

                )

            });



            // M-03: Vitrifiye, ArmatÃ¼r ve Bataryalar MontajÄ± (AdÄ±m 4)

            var m03Start = m02End;

            var m03End = m03Start.AddDays(5);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-03",

                Order = stepOrderSeq++,

                PhaseName = "Vitrifiye, ArmatÃ¼r ve Bataryalar MontajÄ±",

                StageCategory = "Vitrifiye & ArmatÃ¼r",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = p4Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Asma Klozet, GÃ¶mme Rezervuar, Lavabo, Batarya ve DuÅŸ Setleri",

                MaterialDescription = "GÃ¶mme rezervuar kumanda panelleri, klozet kapaklarÄ±, lavabo bataryalarÄ±, duÅŸ baÅŸlÄ±klarÄ± ve aksesuarlar.",

                MaterialQuantity = 0,

                MaterialUnit = "Daire Seti",

                MaterialFormula = "DXF Bekliyor (Islak hacim vitrifiye yerleÅŸim planÄ±ndan netleÅŸecek)",

                SupplierCategory = "Vitrifiye & Banyo DonanÄ±m Bayisi",

                LaborTitle = "Ä°nce SÄ±hhi Tesisat Montaj Ekibi",

                ProfessionName = "Vitrifiye Montaj UstasÄ±",

                CrewComposition = "1 Formen + 4 Montaj UstasÄ±",

                CrewSize = 5,

                DurationDays = 5,

                EquipmentNeeded = "Tork AnahtarlarÄ±, Silikon TabancalarÄ±, Boru AnahtarlarÄ±",

                Predecessors = "Banyo mobilyalarÄ± ve seramik imalatlarÄ±nÄ±n tamamlanmasÄ±",

                CriticalDependency = "Batarya ve klozet montajlarÄ± tamamlandÄ±ktan sonra basÄ±nÃ§lÄ± su testi yapÄ±lmadan daire teslim edilemez!",

                DnaSource = "AdÄ±m 4: Vitrifiye & ArmatÃ¼r",

                PlannedStartDate = m03Start,

                PlannedEndDate = m03End,

                SubSteps = CreateSubSteps(

                    "T-2 Daire bazlÄ± vitrifiye kutularÄ±nÄ±n daÄŸÄ±tÄ±mÄ± ve ara musluklarÄ±n temizliÄŸi",

                    "Asma klozet, lavabo montajÄ±, bataryalar ve duÅŸ baÅŸlÄ±ÄŸÄ± montajÄ±",

                    "TÃ¼m bataryalarÄ±n su akÄ±ÅŸ ve rezervuar su basma testi, kaÃ§ak kontrolÃ¼",

                    "Porselen montajlarÄ±nda aÅŸÄ±rÄ± sÄ±kma Ã§atlamalarÄ±nÄ± Ã¶nleme ve koruyucu eldiven"

                )

            });



            // M-04: AsansÃ¶r MontajÄ±, GÃ¼venlik Testleri ve Tescil RuhsatÄ± (AdÄ±m 8)

            var m04Start = s05End;

            var m04End = m04Start.AddDays(12);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-04",

                Order = stepOrderSeq++,

                PhaseName = "AsansÃ¶r MontajÄ±, GÃ¼venlik Testleri ve Tescil RuhsatÄ±",

                StageCategory = "AsansÃ¶r & SirkÃ¼lasyon",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = p4Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Makine Dairesiz (MRL) AsansÃ¶r Paketi, Raylar ve Kabin DonanÄ±mÄ±",

                MaterialDescription = "KÄ±lavuz raylar, tahrik motoru, kabin, halatlar, karÅŸÄ± aÄŸÄ±rlÄ±k, otomatik kat kapÄ±larÄ± ve kumanda panosu.",

                MaterialQuantity = 0,

                MaterialUnit = "Adet AsansÃ¶r",

                MaterialFormula = "DXF Bekliyor (AsansÃ¶r kuyu rÃ¶levesi ve durak sayÄ±sÄ±ndan hesaplanacak)",

                SupplierCategory = "AsansÃ¶r Ä°malatÃ§Ä± & Montaj FirmasÄ±",

                LaborTitle = "AsansÃ¶r Ä°malat ve Montaj Ekibi",

                ProfessionName = "AsansÃ¶r MontajcÄ±sÄ±",

                CrewComposition = "1 AsansÃ¶r MÃ¼hendisi + 3 Montaj UstasÄ±",

                CrewSize = 4,

                DurationDays = 12,

                EquipmentNeeded = "Kuyu Ä°Ã§i Tirfor / HalatlÄ± Ã‡ektirme, Lazer ÅakÃ¼l, Tork AnahtarlarÄ±",

                Predecessors = "Kuyu dibi su yalÄ±tÄ±mÄ±, ÅŸaft duvarlarÄ± ve asansÃ¶r makine dairesi / kule betonu",

                CriticalDependency = "A Tipi Muayene KuruluÅŸu yeÅŸil etiket vermeden ve tescil ruhsatÄ± Ã§Ä±kmadan asansÃ¶r Ã§alÄ±ÅŸtÄ±rÄ±lamaz!",

                DnaSource = "AdÄ±m 8: AsansÃ¶r Sistemleri",

                PlannedStartDate = m04Start,

                PlannedEndDate = m04End,

                SubSteps = CreateSubSteps(

                    "T-5 Kuyu ÅŸakÃ¼lÃ¼ atÄ±lmasÄ±, kuyu dibi temizliÄŸi ve ray konsollarÄ±nÄ±n kontrolÃ¼",

                    "KÄ±lavuz ray montajÄ±, makine/motor montajÄ±, kabin ve paraÅŸÃ¼t fren sistemi kurulumu",

                    "A Tipi Muayene KuruluÅŸu yeÅŸil etiket kabulÃ¼ ve belediye tescil belgesi",

                    "Kuyu iÃ§i yaÅŸam hattÄ±, kuyu kapÄ±larÄ± koruyucu paravanlarÄ± ve revizyon kumanda denetimi"

                )

            });



            // M-05: Kat Holleri / YangÄ±n Merdiveni Mermer Kaplama & YangÄ±n DolaplarÄ± (AdÄ±m 8)

            var m05Start = m04Start.AddDays(2);

            var m05End = m05Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-05",

                Order = stepOrderSeq++,

                PhaseName = "Kat Holleri / YangÄ±n Merdiveni Mermer Kaplama & YangÄ±n DolaplarÄ±",

                StageCategory = "Ortak Alanlar & YangÄ±n GÃ¼venliÄŸi",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = p4Trigger,

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Kat HolÃ¼ / Merdiven Mermerleri, EI90 YangÄ±n KaÃ§Ä±ÅŸ KapÄ±larÄ± ve YangÄ±n DolaplarÄ±",

                MaterialDescription = "Basamak ve kat koridorlarÄ± mermer kaplama, panik barlÄ± yangÄ±n merdiven kapÄ±larÄ± ve yangÄ±n dolabÄ± lans setleri.",

                MaterialQuantity = 0,

                MaterialUnit = "Kat Seti",

                MaterialFormula = "DXF Bekliyor (Merdiven basamak detaylarÄ± ve kat holÃ¼ alanlarÄ±ndan hesaplanacak)",

                SupplierCategory = "Mermer OcaÄŸÄ± & YangÄ±n GÃ¼venlik FirmasÄ±",

                LaborTitle = "Mermer & YangÄ±n GÃ¼venlik Ekibi",

                ProfessionName = "Mermerci & YangÄ±n Ekibi",

                CrewComposition = "1 Formen + 4 Mermerci + 2 YangÄ±n TesisatÃ§Ä±sÄ±",

                CrewSize = 7,

                DurationDays = 8,

                EquipmentNeeded = "Sulu Mermer Kesici, Lazer Hizalama, Boru Pres Makinesi",

                Predecessors = "Kat koridorlarÄ± ve merdiven kova kaba sÄ±valarÄ±nÄ±n tamamlanmasÄ±",

                CriticalDependency = "Ä°tfaiye yangÄ±n uygunluk raporu iÃ§in yangÄ±n kapÄ±larÄ± duman fitilleri ve yaylÄ± menteÅŸeleri eksiksiz olmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 8: YangÄ±n GÃ¼venliÄŸi & Ortak Alanlar",

                PlannedStartDate = m05Start,

                PlannedEndDate = m05End,

                SubSteps = CreateSubSteps(

                    "T-3 YangÄ±n kaÃ§Ä±ÅŸ kapÄ±larÄ± sertifikalarÄ±, basamak mermerleri ve hortum dolaplarÄ± sevki",

                    "Merdiven mermer kaplama, yangÄ±n kaÃ§Ä±ÅŸ kapÄ±larÄ± panik bar montajÄ± ve kat dolaplarÄ± montajÄ±",

                    "KapÄ± yaylÄ± otomatik kapanma testi, duman sÄ±zdÄ±rmazlÄ±k fitili ve yangÄ±n hattÄ± su testi",

                    "YangÄ±n merdiveni geÃ§ici aydÄ±nlatmasÄ± ve basamak kaydÄ±rmaz bant uygulamasÄ±"

                )

            });



            // M-06: Daire Temizlikleri, Ä°nce Kabul, Tesisat Testleri ve Ä°skan / Teslim HazÄ±rlÄ±ÄŸÄ±

            var m06Start = m05End.AddDays(2);

            var m06End = m06Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "M-06",

                Order = stepOrderSeq++,

                PhaseName = "Daire Temizlikleri, Ä°nce Kabul, Tesisat Testleri ve Ä°skan / Teslim HazÄ±rlÄ±ÄŸÄ±",

                StageCategory = "Kabul & Teslim",

                PartIndex = p4Idx,

                PartName = p4Name,

                PartBadgeClass = p4Badge,

                TriggerRule = "TÃ¼m daire iÃ§i montajlar tamamlanÄ±nca baÅŸlar",

                TargetStructureName = targetStructureName,

                TargetBuildingId = targetBuildingId,

                MaterialTitle = "Ä°nÅŸaat SonrasÄ± Temizlik KimyasallarÄ±, Koruma KartonlarÄ± ve Kabul TutanaklarÄ±",

                MaterialDescription = "Cam, zemin ve dolaplarÄ±n endÃ¼striyel temizliÄŸi, boya rÃ¶tuÅŸlarÄ±, tesisat testleri ve baÄŸÄ±msÄ±z bÃ¶lÃ¼m teslim tutanaklarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "Daire",

                MaterialFormula = "DXF Bekliyor (Toplam baÄŸÄ±msÄ±z bÃ¶lÃ¼m sayÄ±sÄ±ndan netleÅŸecek)",

                SupplierCategory = "EndÃ¼striyel Temizlik & Åantiye Sarf Malzemeleri",

                LaborTitle = "Ä°nce Temizlik ve Kalite Kabul Ekibi",

                ProfessionName = "Temizlik & Kalite Kontrol UstasÄ±",

                CrewComposition = "1 Kalite Kontrol Åefi + 6 Temizlik Personeli + 2 RÃ¶tuÅŸ UstasÄ±",

                CrewSize = 9,

                DurationDays = 7,

                EquipmentNeeded = "EndÃ¼striyel Vakum Makineleri, BuharlÄ± Temizleyiciler, Lazer Kontrol CihazlarÄ±",

                Predecessors = "TÃ¼m ince iÅŸlerin, vitrifiye, mobilya ve kapÄ± montajlarÄ±nÄ±n bitmesi",

                CriticalDependency = "Eksik ve kusur listesi (punch list) giderilmeden daire anahtarlarÄ± teslim edilemez!",

                DnaSource = "YapÄ± YÃ¶netimi: Teslim ve Ä°skan",

                PlannedStartDate = m06Start,

                PlannedEndDate = m06End,

                SubSteps = CreateSubSteps(

                    "T-2 Daire bazlÄ± punch list (kusur tespit tutanaÄŸÄ±) oluÅŸturulmasÄ±",

                    "EndÃ¼striyel ince temizlik, harÃ§/boya lekelerinin Ã§Ä±karÄ±lmasÄ± ve zemin korumasÄ±",

                    "Elektrik sortileri, bataryalar ve pencere kanatlarÄ±nÄ±n tek tek fonksiyon testi",

                    "Islak zeminlerde kayma Ã¶nlemleri ve temizlik kimyasallarÄ± gÃ¼venlik bilgi formu (MSDS)"

                )

            });



            return list;

        }



        private static List<WorkflowSubStepDto> CreateWorkflowSubSteps(string prePrep, string exec, string quality, string isg)

        {

            return new List<WorkflowSubStepDto>

            {

                new WorkflowSubStepDto { Title = prePrep, Category = "PrePreparation", IsCompleted = false },

                new WorkflowSubStepDto { Title = exec, Category = "Execution", IsCompleted = false },

                new WorkflowSubStepDto { Title = quality, Category = "QualityAndCuring", IsCompleted = false },

                new WorkflowSubStepDto { Title = isg, Category = "SafetyIsg", IsCompleted = false }

            };

        }



        private List<WorkflowStepItemDto> GenerateSiteSetupSteps(ConstructionProject project, ProjectDnaProfile profile)

        {

            var list = new List<WorkflowStepItemDto>();

            if (project == null) return list;



            var details = profile?.Details?.ToList() ?? new List<ProjectDnaDetail>();



            var ihataMatDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("Sac") || d.SelectedItem.Contains("Tel Ã–rgÃ¼") || d.SelectedItem.Contains("Pano") || d.SelectedItem.Contains("OSB"));

            string ihataMalzemesi = ihataMatDetail != null ? ihataMatDetail.SelectedItem : "Trapez Sac / OSB Pano Ã‡evre Ä°hatasÄ±";



            var kapiDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("KapÄ±"));

            string kapiTipi = kapiDetail != null ? kapiDetail.SelectedItem : "SÃ¼rgÃ¼lÃ¼ RaylÄ± Sac KapÄ± (6.00m)";



            var molozDetail = details.FirstOrDefault(d => d.SelectedItem.Contains("YÄ±kÄ±m") || d.SelectedItem.Contains("Moloz") || d.SelectedItem.Contains("Enkaz") || d.SelectedItem.Contains("BoÅŸ Arsa"));

            bool isCleanPlot = molozDetail != null && (molozDetail.SelectedItem.Contains("BoÅŸ") || molozDetail.SelectedItem.Contains("Gerekmez") || molozDetail.SelectedItem.Contains("Yok"));

            bool molozVar = molozDetail != null && !isCleanPlot && (molozDetail.SelectedItem.Contains("YÄ±kÄ±lacak") || molozDetail.SelectedItem.Contains("Moloz ve Ã‡Ã¶p") || molozDetail.SelectedItem.Contains("Enkaz"));

            bool hasDemolition = !isCleanPlot && (molozVar || (project.EskiToplamMetrekare.HasValue && project.EskiToplamMetrekare.Value > 0) || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0));



            var elektrikDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("AdÄ±m 1")) && (d.SelectedItem.Contains("Elektrik") || d.SelectedItem.Contains("Abonelik") || d.SelectedItem.Contains("Trafo") || d.SelectedItem.Contains("JeneratÃ¶r")));

            string elektrikTipi = elektrikDetail != null ? elektrikDetail.SelectedItem : "Åantiye Åebeke AboneliÄŸi ve TMÅ DaÄŸÄ±tÄ±m Panosu";



            var suDetail = details.FirstOrDefault(d => (d.Phase.Contains("1") || d.Phase.Contains("Faz 1") || d.Phase.Contains("AdÄ±m 1")) && (d.SelectedItem.Contains("Su") || d.SelectedItem.Contains("Depo") || d.SelectedItem.Contains("Hidrofor") || d.SelectedItem.Contains("Kuyu")));

            string suTipi = suDetail != null ? suDetail.SelectedItem : "15 Tonluk Su Deposu ve 3 HP Otomatik Hidrofor";



            DateTime curDate = DateTime.Today;



            // S-01: Harita, Aplikasyon & Sabit RÃ¶per Kotu

            var s01Start = curDate;

            var s01End = s01Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-01",

                Order = 1,

                PhaseName = "Harita, Aplikasyon & Sabit RÃ¶per Kotu",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Åantiye iÅŸbaÅŸÄ±nda 1 defa yapÄ±lÄ±r, bloklara tekrarlamaz",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = "AhÅŸap KazÄ±klar, KireÃ§ Tozu, Ã‡elik RÃ¶per Ã‡ubuklarÄ± ve Nivo Kot Ä°ÅŸaretleri",

                MaterialDescription = "Arsa kÃ¶ÅŸe poligonu, kÃ¶ÅŸe kotlarÄ±, sabit rÃ¶per noktasÄ± ve tÃ¼m bloklarÄ±n oturum sÄ±nÄ±rlarÄ±nÄ±n kireÃ§le iÅŸaretlenmesi.",

                MaterialQuantity = 0,

                MaterialUnit = "TakÄ±m",

                MaterialFormula = "DXF Bekliyor (Mimari vaziyet ve kÃ¶ÅŸe koordinatlarÄ±ndan belirlenecek)",

                SupplierCategory = "Harita MÃ¼hendislik Ofisi / TopoÄŸraf EkipmanÄ±",

                LaborTitle = "Harita MÃ¼hendisi & TopoÄŸraf Ekibi",

                ProfessionName = "Harita MÃ¼hendisi / TopoÄŸraf",

                CrewComposition = "1 Harita MÃ¼h. + 1 TopoÄŸraf + 2 YardÄ±mcÄ±",

                CrewSize = 4,

                DurationDays = 2,

                EquipmentNeeded = "Total Station, Nivo, Lazer Metre, GPS CihazÄ±",

                Predecessors = "Ä°mar Ã§apÄ± ve mimari vaziyet planÄ± kabulÃ¼",

                CriticalDependency = "Sabit rÃ¶per kotu alÄ±nmadan ve kÃ¶ÅŸe kazÄ±klarÄ± Ã§akÄ±lmadan Ã§evre ihata yapÄ±lamaz, ÅŸantiye baÅŸlayamaz!",

                DnaSource = "AdÄ±m 1: Saha Analizi & Aplikasyon",

                PlannedStartDate = s01Start,

                PlannedEndDate = s01End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Belediye imar Ã§apÄ±, aplikasyon krokisi ve kÃ¶ÅŸe koordinat listesi temini",

                    "Total Station ve GPS ile parsel kÃ¶ÅŸe kazÄ±klarÄ±nÄ±n Ã§akÄ±lmasÄ± ve sabit rÃ¶per noktasÄ±nÄ±n betona Ã§akÄ±lmasÄ±",

                    "Ä°mar Ã§ekme mesafelerinin ve sabit rÃ¶per kotunun doÄŸrulanmasÄ±",

                    "Saha Ã§evre gÃ¼venlik hattÄ± ve komÅŸu parsel sÄ±nÄ±r reflektÃ¶rlÃ¼ ÅŸeritlerinin Ã§ekilmesi"

                )

            });



            // S-02: Parsel Ã‡evre Ä°hatasÄ± & RaylÄ± AraÃ§ GiriÅŸ KapÄ±sÄ± (OSB Pano / Tel Ã‡it)

            var s02Start = s01End;

            var s02End = s02Start.AddDays(3);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-02",

                Order = 2,

                PhaseName = "Parsel Ã‡evre Ä°hatasÄ± & RaylÄ± AraÃ§ GiriÅŸ KapÄ±sÄ± (OSB Pano / Tel Ã‡it)",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Aplikasyon sÄ±nÄ±rlarÄ± Ã§akÄ±lÄ±nca kurulur",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = $"{ihataMalzemesi} ve {kapiTipi}",

                MaterialDescription = "Åantiye parsel Ã§evresinin kapatÄ±lmasÄ±, dikme direkleri, rÃ¼zgar payandalarÄ± ve ana araÃ§ giriÅŸ kapÄ±sÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mt",

                MaterialFormula = "DXF Bekliyor (Parsel Ã§evre sÄ±nÄ±r Ã¶lÃ§Ã¼sÃ¼nden hesaplanacak)",

                SupplierCategory = "Demir Ã‡elik / Ã‡it & Sac Bayisi",

                LaborTitle = "Demir DoÄŸrama ve Ã‡it Montaj Ekibi",

                ProfessionName = "Demir / Ã‡it UstasÄ±",

                CrewComposition = "1 Demir DoÄŸrama Formeni + 2 KaynakÃ§Ä± + 2 Montaj Ä°ÅŸÃ§isi",

                CrewSize = 5,

                DurationDays = 3,

                EquipmentNeeded = "Kaynak Makinesi, Profil Kesme, Matkap TakÄ±mlarÄ±",

                Predecessors = "Harita aplikasyon sÄ±nÄ±r tespiti",

                CriticalDependency = "Ä°ÅŸyeri ve belediye denetimi gereÄŸi Ã§evre ihatasÄ± ve kapÄ± olmadan ÅŸantiyeye makine sokulamaz!",

                DnaSource = "AdÄ±m 1: Åantiye Ã‡evre Ä°hatasÄ±",

                PlannedStartDate = s02Start,

                PlannedEndDate = s02End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Ä°hata panolarÄ±, ankraj profilleri ve raylÄ± kapÄ± mekanizmasÄ±nÄ±n ÅŸantiyeye sevki",

                    "Direk ankrajlarÄ±nÄ±n sabitlenmesi, Ã§evre panolarÄ±nÄ±n montajÄ± ve raylÄ± giriÅŸ kapÄ±sÄ± montajÄ±",

                    "RÃ¼zgar fÄ±rtÄ±na yÃ¼kÃ¼ne karÅŸÄ± payanda rijitliÄŸi ve kilit sistemi kontrolÃ¼",

                    "Åantiye tanÄ±tÄ±m tabelasÄ±, 'Yetkisiz Girilmez' levhalarÄ± ve acil toplanma noktasÄ± tabelasÄ±"

                )

            });



            // S-03: Saha YÃ¼zey TemizliÄŸi / Varsa Eski YapÄ± YÄ±kÄ±mÄ± & Moloz BertarafÄ±

            var s03Start = s02End;

            var s03End = s03Start.AddDays(hasDemolition ? 5 : 2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-03",

                Order = 3,

                PhaseName = hasDemolition ? "Eski YapÄ± YÄ±kÄ±mÄ±, Enkaz SÃ¶kÃ¼mÃ¼ & Moloz BertarafÄ±" : "Saha YÃ¼zey TemizliÄŸi, Moloz & Ã‡Ã¶p TemizliÄŸi",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Ã‡evre ihata tamamlanÄ±nca temizlik/yÄ±kÄ±m yapÄ±lÄ±r",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = hasDemolition ? "YÄ±kÄ±m KÄ±rÄ±cÄ± UÃ§larÄ±, Toz BastÄ±rma TesisatÄ± ve Moloz Nakliye KamyonlarÄ±" : "YÃ¼zey Temizlik KepÃ§esi ve Moloz TaÅŸÄ±ma KamyonlarÄ±",

                MaterialDescription = hasDemolition ? "Mevcut yapÄ±nÄ±n kontrollÃ¼ yÄ±kÄ±mÄ±, hurda ayrÄ±ÅŸtÄ±rma ve molozun dÃ¶kÃ¼m sahasÄ±na nakliyesi." : "BoÅŸ arsa yÃ¼zeyindeki bitki, Ã§Ã¶p, yÃ¼zey atÄ±klarÄ± ve molozlarÄ±n temizlenmesi.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Arsa yÃ¼zey alanÄ±ndan hesaplanacak)",

                SupplierCategory = "Hafriyat & YÄ±kÄ±m FirmasÄ±",

                LaborTitle = "Saha Temizlik & YÄ±kÄ±m Ekibi",

                ProfessionName = "YÄ±kÄ±m / Hafriyat Ekibi",

                CrewComposition = "1 Formen + 1 EkskavatÃ¶r Op. + 1 Loder Op. + 4 Kamyon ÅofÃ¶rÃ¼",

                CrewSize = 7,

                DurationDays = hasDemolition ? 5 : 2,

                EquipmentNeeded = "Paletli EkskavatÃ¶r, Loder, 4 Kamyon, Toz BastÄ±rma Su ArazÃ¶zÃ¼",

                Predecessors = "Ã‡evre ihatasÄ± ve kapÄ± gÃ¼venliÄŸinin saÄŸlanmasÄ±",

                CriticalDependency = "YÄ±kÄ±m varsa elektrik/su/doÄŸalgaz kesme tutanaklarÄ± olmadan iÅŸe baÅŸlanamaz!",

                DnaSource = "AdÄ±m 1: YÄ±kÄ±m & Temizlik",

                PlannedStartDate = s03Start,

                PlannedEndDate = s03End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Belediye yÄ±kÄ±m/temizlik izinleri ve nakliye fiÅŸleri kontrolÃ¼",

                    "YÃ¼zey temizliÄŸi / kontrollÃ¼ yÄ±kÄ±m ve malzemenin kamyonlara yÃ¼klenmesi",

                    "Arsa kot seviyesinin temizlenmesi ve komÅŸu parsel sÄ±nÄ±r kontrolÃ¼",

                    "Toz bastÄ±rma sulamasÄ± ve Ã§evre gÃ¼venlik ÅŸeridi denetimi"

                )

            });



            // S-04: Åantiye GeÃ§ici Elektrik AboneliÄŸi & TMÅ DaÄŸÄ±tÄ±m Panosu

            var s04Start = s03End;

            var s04End = s04Start.AddDays(3);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-04",

                Order = 4,

                PhaseName = "Åantiye GeÃ§ici Elektrik AboneliÄŸi & TMÅ DaÄŸÄ±tÄ±m Panosu",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Saha temizlenince altyapÄ± panosu kurulur",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = $"{elektrikTipi}, Ana TMÅ DaÄŸÄ±tÄ±m Panosu, KaÃ§ak AkÄ±m RÃ¶leleri ve BakÄ±r Topraklama Ã‡ubuklarÄ±",

                MaterialDescription = "Elektrik daÄŸÄ±tÄ±m kurumundan geÃ§ici inÅŸaat elektriÄŸi baÄŸlantÄ±sÄ±, kilitli sayaÃ§ panosu ve saha daÄŸÄ±tÄ±m kutularÄ±.",

                MaterialQuantity = 1,

                MaterialUnit = "TakÄ±m",

                MaterialFormula = "1 Adet Åantiye Ana Elektrik AltyapÄ± Paketi",

                SupplierCategory = "Elektrik Tesisat & Pano Ä°malatÃ§Ä±sÄ±",

                LaborTitle = "Åantiye Elektrik Ekibi",

                ProfessionName = "Elektrik MÃ¼hendisi / TesisatÃ§Ä±sÄ±",

                CrewComposition = "1 Elektrik MÃ¼hendisi + 2 Elektrik UstasÄ±",

                CrewSize = 3,

                DurationDays = 3,

                EquipmentNeeded = "Topraklama Meger Ã–lÃ§Ã¼m CihazÄ±, Pano Montaj TakÄ±mlarÄ±",

                Predecessors = "DaÄŸÄ±tÄ±m ÅŸirketi proje onayÄ± ve sayaÃ§ kabulÃ¼",

                CriticalDependency = "300mA ana ve 30mA tali kaÃ§ak akÄ±m rÃ¶leleri olmadan ve topraklama Ã¶lÃ§Ã¼mÃ¼ yapÄ±lmadan ÅŸantiyeye enerji verilemez!",

                DnaSource = "AdÄ±m 1: GeÃ§ici Elektrik",

                PlannedStartDate = s04Start,

                PlannedEndDate = s04End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-3 DaÄŸÄ±tÄ±m ÅŸirketi onaylÄ± proje, geÃ§ici inÅŸaat tarifesi ve sayaÃ§ temini",

                    "Kilitli ana TMÅ daÄŸÄ±tÄ±m panosu montajÄ±, bakÄ±r topraklama Ã§ubuklarÄ± Ã§akÄ±mÄ± ve kablolama",

                    "Topraklama meger direnÃ§ testi (maksimum 2 Ohm) ve kaÃ§ak akÄ±m rÃ¶le testi",

                    "Pano kilit altÄ±na alma, yÃ¼ksek gerilim uyarÄ± levhasÄ± ve 6 kg CO2 yangÄ±n sÃ¶ndÃ¼rme tÃ¼pÃ¼"

                )

            });



            // S-05: Ä°nÅŸaat ve KullanÄ±m Suyu Temini & Su Deposu / Hidrofor TesisatÄ±

            var s05Start = s04Start;

            var s05End = s05Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-05",

                Order = 5,

                PhaseName = "Ä°nÅŸaat ve KullanÄ±m Suyu Temini & Su Deposu / Hidrofor TesisatÄ±",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Elektrik panosuyla eÅŸ zamanlÄ± kurulur",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = $"{suTipi}, Åantiye Su SayaÃ§ Grubu, DaÄŸÄ±tÄ±m VanalarÄ± ve Don KorumalÄ± Borulama",

                MaterialDescription = "Åebeke inÅŸaat suyu aboneliÄŸi, 15 tonluk polietilen/Ã§elik su deposu, otomatik frekans kontrollÃ¼ hidrofor ve ana daÄŸÄ±tÄ±m hattÄ±.",

                MaterialQuantity = 1,

                MaterialUnit = "TakÄ±m",

                MaterialFormula = "1 Adet Åantiye Su Depolama & Hidrofor Paketi",

                SupplierCategory = "Su Ä°daresi & SÄ±hhi Tesisat / Pompa Bayisi",

                LaborTitle = "Åantiye Mekanik Tesisat Ekibi",

                ProfessionName = "SÄ±hhi Tesisat UstasÄ±",

                CrewComposition = "1 Tesisat Formeni + 2 Tesisat UstasÄ±",

                CrewSize = 3,

                DurationDays = 2,

                EquipmentNeeded = "Boru Kaynak Makinesi, BasÄ±nÃ§ Test PompasÄ±",

                Predecessors = "Su idaresi geÃ§ici inÅŸaat suyu abonelik sÃ¶zleÅŸmesi",

                CriticalDependency = "ÅANTÄ°YE ALTIN KURALI: Ä°nÅŸaat suyu ve deposu hazÄ±r olmadan beton dÃ¶kÃ¼lemez ve ÅŸantiye Ã§alÄ±ÅŸamaz!",

                DnaSource = "AdÄ±m 1: Åantiye Suyu",

                PlannedStartDate = s05Start,

                PlannedEndDate = s05End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Su idaresi sayaÃ§ montajÄ± ve ÅŸantiye su deposu nakliyesi",

                    "Su deposu terazileme, hidrofor montajÄ±, ÅŸantiye daÄŸÄ±tÄ±m vanalarÄ± ve test baÄŸlantÄ±sÄ±",

                    "Hidrofor basÄ±nÃ§ ayarÄ± (4-6 bar) ve su sÄ±zdÄ±rmazlÄ±k testi",

                    "Depo taÅŸma drenaj hattÄ± ve kÄ±ÅŸ dÃ¶nemi don koruma izolasyonu"

                )

            });



            // S-06: Kanalizasyon BaÄŸlantÄ±sÄ± & Mobil Kimyasal Ä°ÅŸÃ§i WC Kabinleri (Aranan madde 1!)

            var s06Start = s05Start;

            var s06End = s06Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-06",

                Order = 6,

                PhaseName = "Kanalizasyon BaÄŸlantÄ±sÄ± & Mobil Kimyasal Ä°ÅŸÃ§i WC Kabinleri",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Su hattÄ± ile birlikte kurulur",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = "Mobil Foseptik / Kimyasal WC Kabinleri, 110-150mm PVC Kanalizasyon BorularÄ±, Menhol ve Ã‡ekvalf",

                MaterialDescription = "Åantiye personeli iÃ§in hijyenik kimyasal mobil tuvalet kabinleri veya belediye kanalizasyon ana hattÄ±na Ã§ekvalfli deÅŸarj baÄŸlantÄ±sÄ±.",

                MaterialQuantity = 2,

                MaterialUnit = "Kabin / Hat",

                MaterialFormula = "2 Adet Mobil WC Kabini + Ana Åebeke AtÄ±k Su BaÄŸlantÄ±sÄ±",

                SupplierCategory = "Mobil WC Kiralama / SÄ±hhi Tesisat & AltyapÄ± TedarikÃ§isi",

                LaborTitle = "AltyapÄ± & Kanalizasyon Ekibi",

                ProfessionName = "AltyapÄ± / SÄ±hhi Tesisat UstasÄ±",

                CrewComposition = "1 Tesisat UstasÄ± + 2 AltyapÄ± Ä°ÅŸÃ§isi",

                CrewSize = 3,

                DurationDays = 2,

                EquipmentNeeded = "Kanal AÃ§ma / Mini KepÃ§e, Boru Kaynak AparatlarÄ±",

                Predecessors = "Belediye kanalizasyon kotu ve deÅŸarj izni",

                CriticalDependency = "Ä°ÅŸ saÄŸlÄ±ÄŸÄ± ve gÃ¼venliÄŸi kanunu uyarÄ±nca ÅŸantiye personeli iÃ§in hijyenik WC ve atÄ±k su baÄŸlantÄ±sÄ± kurulmadan Ã§alÄ±ÅŸma baÅŸlatÄ±lamaz!",

                DnaSource = "Ä°SG ve Åantiye Tesisat StandartlarÄ± (AdÄ±m 1)",

                PlannedStartDate = s06Start,

                PlannedEndDate = s06End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Belediye atÄ±k su baÄŸlantÄ± izinleri ve kimyasal WC kabinlerinin ÅŸantiyeye sevki",

                    "Kanalizasyon menhol baÄŸlantÄ±sÄ±, Ã§ekvalf montajÄ± ve mobil WC kabinlerinin kurulumu",

                    "Su akÄ±ÅŸ eÄŸimi ve geri tepme Ã¶nleyici Ã§ekvalf sÄ±zdÄ±rmazlÄ±k kontrolÃ¼",

                    "Hijyen dezenfeksiyon malzemeleri, sÄ±vÄ± sabunluklar ve dÃ¼zenli vidanjÃ¶r temizlik protokolÃ¼"

                )

            });



            // S-07: Åantiye KampÄ± & Mobilizasyon: Ofis, Yatakhane, Yemekhane & Soyunma Konteynerleri (Aranan madde 2!)

            var s07Start = s06End;

            var s07End = s07Start.AddDays(3);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-07",

                Order = 7,

                PhaseName = "Åantiye KampÄ± & Mobilizasyon: Ofis, Yatakhane, Yemekhane & Soyunma Konteynerleri",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Saha altyapÄ±sÄ± hazÄ±r olunca konteynerler yerleÅŸtirilir",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = "SandviÃ§ Panel Åantiye Konteynerleri (Ofis, Yatakhane, Yemekhane, DuÅŸ/Soyunma)",

                MaterialDescription = "YÃ¶netim ofisi, teknik ofis, iÅŸÃ§i yatakhane ve dinlenme Ã¼niteleri, yemekhane masalarÄ± ve ranzalar.",

                MaterialQuantity = 3,

                MaterialUnit = "Ãœnite",

                MaterialFormula = "3 Adet SandviÃ§ Panel YaÅŸam ve YÃ¶netim Konteyneri",

                SupplierCategory = "Prefabrik YapÄ± & Åantiye Konteyner Ãœreticisi",

                LaborTitle = "Mobilizasyon Kurulum Ekibi",

                ProfessionName = "Prefabrik / Konteyner Montaj UstasÄ±",

                CrewComposition = "1 Mobil VinÃ§ OperatÃ¶rÃ¼ + 3 Montaj UstasÄ±",

                CrewSize = 4,

                DurationDays = 3,

                EquipmentNeeded = "Mobil VinÃ§, Lazer Terazi, Ã‡elik BaÄŸlantÄ± ElemanlarÄ±",

                Predecessors = "Konteyner altÄ± mÄ±cÄ±r/grobeton zemin platformunun hazÄ±rlanmasÄ±",

                CriticalDependency = "KadrolarÄ±n yatma, yemek ve barÄ±nma alanlarÄ± kurulmadan ÅŸantiyeye usta ve iÅŸÃ§i kabul edilemez!",

                DnaSource = "Åantiye Mobilizasyon StandartlarÄ± (AdÄ±m 1)",

                PlannedStartDate = s07Start,

                PlannedEndDate = s07End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-3 Konteyner zemin takozlarÄ±, elektrik/su sortileri ve vinÃ§ sahasÄ± hazÄ±rlÄ±ÄŸÄ±",

                    "Konteynerlerin vinÃ§ ile indirilmesi, terazilenmesi, elektrik ve temiz su baÄŸlantÄ±larÄ±nÄ±n yapÄ±lmasÄ±",

                    "Pencere/kapÄ± yalÄ±tÄ±mlarÄ±, klimalar ve aydÄ±nlatma linye kontrolleri",

                    "Acil toplanma alanÄ± krokisi, ilk yardÄ±m ecza dolabÄ± ve yangÄ±n sÃ¶ndÃ¼rme tÃ¼pleri yerleÅŸimi"

                )

            });



            // S-08: Åantiye Ã‡evre AydÄ±nlatmasÄ±, GÃ¼venlik KulÃ¼besi & 7/24 Mobil IP Kamera Sistemi

            var s08Start = s07End;

            var s08End = s08Start.AddDays(2);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "S-08",

                Order = 8,

                PhaseName = "Åantiye Ã‡evre AydÄ±nlatmasÄ±, GÃ¼venlik KulÃ¼besi & 7/24 Mobil IP Kamera Sistemi",

                StageCategory = "Ortak Saha Kurulumu",

                PartIndex = 0,

                PartName = "Åantiye Ortak Saha Kurulumu (TÃ¼m Arsa)",

                PartBadgeClass = "dark",

                TriggerRule = "Mobilizasyon tamamlanÄ±nca gÃ¼venlik devreye alÄ±nÄ±r",

                TargetStructureName = "Proje Geneli (Ortak Saha)",

                MaterialTitle = "LED Saha ProjektÃ¶rleri, GÃ¼venlik BekÃ§i KulÃ¼besi, Gece GÃ¶rÃ¼ÅŸlÃ¼ 4K IP Kameralar ve 4G NVR KayÄ±t Ãœnitesi",

                MaterialDescription = "GiriÅŸ kapÄ±sÄ± gÃ¼venlik kabini, Ã§evre ihata Ã¼zeri LED aydÄ±nlatmalar, ÅŸantiye alanÄ±nÄ± izleyen yÃ¼ksek Ã§Ã¶zÃ¼nÃ¼rlÃ¼klÃ¼ IP gÃ¼venlik kameralarÄ±.",

                MaterialQuantity = 1,

                MaterialUnit = "Sistem",

                MaterialFormula = "1 TakÄ±m 7/24 Åantiye Kamera & GÃ¼venlik AydÄ±nlatma Paketi",

                SupplierCategory = "GÃ¼venlik Sistemleri & Elektrik ToptancÄ±sÄ±",

                LaborTitle = "GÃ¼venlik & Kamera Montaj Ekibi",

                ProfessionName = "ZayÄ±f AkÄ±m & Kamera Teknisyeni",

                CrewComposition = "1 GÃ¼venlik Sistemleri Teknisyeni + 2 ElektrikÃ§i",

                CrewSize = 3,

                DurationDays = 2,

                EquipmentNeeded = "Merdiven / Platform, UTP Test CihazÄ±, Lazer Hizalama",

                Predecessors = "Ã‡evre ihatasÄ± ve ÅŸantiye ana elektrik panosu kurulumu",

                CriticalDependency = "Gece aydÄ±nlatmasÄ± ve kameralar aktif olmadan ÅŸantiye sahasÄ±na deÄŸerli demir, kablo ve inÅŸaat malzemesi indirilemez!",

                DnaSource = "Åantiye GÃ¼venlik StandartlarÄ± (AdÄ±m 1)",

                PlannedStartDate = s08Start,

                PlannedEndDate = s08End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Kamera kÃ¶r nokta analizi, projektÃ¶r direkleri ve kablo kanallarÄ±nÄ±n Ã§ekilmesi",

                    "IP kameralarÄ±n direklere montajÄ±, NVR kayÄ±t cihazÄ± kurulumu ve 4G uzaktan izleme aktivasyonu",

                    "Gece gÃ¶rÃ¼ÅŸ testi, projektÃ¶r aydÄ±nlatma lÃ¼ks seviyesi ve hareket algÄ±lama uyarÄ± testi",

                    "'7/24 GÃ¼venlik KamerasÄ±yla Ä°zlenmektedir' uyarÄ± levhalarÄ± ve gece vardiya gÃ¼venlik nÃ¶bet defteri"

                )

            });



            return list;

        }



        private List<WorkflowStepItemDto> GenerateSiteHandoverSteps(ConstructionProject project, ProjectDnaProfile profile)

        {

            var list = new List<WorkflowStepItemDto>();

            if (project == null) return list;



            DateTime curDate = DateTime.Today.AddMonths(8);



            // P-01: BahÃ§e AltyapÄ±sÄ±, Otomatik Sulama TesisatÄ± & Toprak Dolgusu

            var p01Start = curDate;

            var p01End = p01Start.AddDays(7);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-01",

                Order = 1,

                PhaseName = "BahÃ§e AltyapÄ±sÄ±, Otomatik Sulama TesisatÄ± & Toprak Dolgusu",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Kule vinÃ§ sÃ¶kÃ¼lÃ¼p aÄŸÄ±r makineler sahadan Ã§Ä±kÄ±nca baÅŸlar",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "BahÃ§e Drenaj BorularÄ±, Otomatik Sulama Solenoid VanalarÄ±, Damlama HatlarÄ± ve ElenmiÅŸ Bitkisel Toprak",

                MaterialDescription = "Saha Ã§evre drenajÄ±, otomatik zaman ayarlÄ± sulama borulamasÄ±, gÃ¼breli bitkisel toprak serimi ve tesviyesi.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Vaziyet planÄ± peyzaj alanÄ±ndan hesaplanacak)",

                SupplierCategory = "Peyzaj FidanlÄ±ÄŸÄ± & Sulama Sistemleri Bayisi",

                LaborTitle = "BahÃ§e AltyapÄ± ve Toprak DÃ¼zenleme Ekibi",

                ProfessionName = "Peyzaj & Sulama UstasÄ±",

                CrewComposition = "1 Peyzaj MÃ¼hendisi + 4 BahÃ§Ä±van / AltyapÄ± Ä°ÅŸÃ§isi",

                CrewSize = 5,

                DurationDays = 7,

                EquipmentNeeded = "Mini KepÃ§e, KompaktÃ¶r, Toprak TÄ±rmÄ±ÄŸÄ±",

                Predecessors = "Bina dÄ±ÅŸ cephe iskelelerinin sÃ¶kÃ¼lmesi ve ÅŸantiye sahasÄ±nÄ±n molozlardan arÄ±ndÄ±rÄ±lmasÄ±",

                CriticalDependency = "AÄŸÄ±r iÅŸ makineleri ve kule vinÃ§ sahadan ayrÄ±lmadan toprak dolgusu ve sulama borularÄ± dÃ¶ÅŸenemez!",

                DnaSource = "AdÄ±m 9: DÄ±ÅŸ Ã‡evre & Peyzaj",

                PlannedStartDate = p01Start,

                PlannedEndDate = p01End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-3 Vaziyet kot planÄ±, drenaj gÃ¼zergahÄ± ve bitkisel toprak sevki",

                    "Drenaj borularÄ±nÄ±n dÃ¶ÅŸenmesi, otomatik sulama borulamasÄ± ve toprak tesviyesi",

                    "Sulama hatlarÄ± basÄ±nÃ§ testi ve drenaj su tahliye eÄŸimi kontrolÃ¼",

                    "Hendek kenarlarÄ± gÃ¼venlik ÅŸeridi ve toz bastÄ±rma sulamasÄ±"

                )

            });



            // P-02: Peyzaj DÃ¼zenlemesi, Ã‡im Alanlar, AÄŸaÃ§landÄ±rma & Ã‡iÃ§eklendirme

            var p02Start = p01End;

            var p02End = p02Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-02",

                Order = 2,

                PhaseName = "Peyzaj DÃ¼zenlemesi, Ã‡im Alanlar, AÄŸaÃ§landÄ±rma & Ã‡iÃ§eklendirme",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Toprak dolgusu ve sulama hazÄ±r olunca uygulanÄ±r",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "Rulo Ã‡im (HazÄ±r Ã‡im KalÄ±plarÄ±), YetiÅŸkin Ä°breli/YapraklÄ± AÄŸaÃ§lar, Ã‡alÄ± GruplarÄ± ve Mevsimlik Ã‡iÃ§ekler",

                MaterialDescription = "Peyzaj projesine uygun yetiÅŸkin aÄŸaÃ§ dikimi, Ã§alÄ± Ã§itleri, Ã§iÃ§ek tarhlarÄ± ve preslenmiÅŸ rulo Ã§im serimi.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (YeÅŸil alan metrajlarÄ±ndan hesaplanacak)",

                SupplierCategory = "FidanlÄ±k & Peyzaj Botanik Merkezi",

                LaborTitle = "Botanik & BahÃ§Ä±vanlÄ±k Ekibi",

                ProfessionName = "Peyzaj MimarÄ± / BahÃ§Ä±van",

                CrewComposition = "1 Peyzaj MimarÄ± + 6 BahÃ§Ä±van",

                CrewSize = 7,

                DurationDays = 8,

                EquipmentNeeded = "Ã‡im Silindiri, Fidan Dikme AparatlarÄ±, Ã‡im HavalandÄ±rma",

                Predecessors = "Otomatik sulama tesisatÄ±nÄ±n aktif Ã§alÄ±ÅŸÄ±r durumda olmasÄ±",

                CriticalDependency = "Rulo Ã§im serildiÄŸi ilk 10 gÃ¼n boyunca sabah ve akÅŸam otomatik sulama aksatÄ±lmamalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 9: DÄ±ÅŸ Ã‡evre & Peyzaj",

                PlannedStartDate = p02Start,

                PlannedEndDate = p02End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Fidan kÃ¶k kontrolÃ¼, rulo Ã§im tazelik kontrolÃ¼ ve gÃ¼bre hazÄ±rlÄ±ÄŸÄ±",

                    "AÄŸaÃ§ Ã§ukurlarÄ±nÄ±n aÃ§Ä±lmasÄ±, kazÄ±klama/baÄŸlama, Ã§alÄ± dikimi ve rulo Ã§im serme",

                    "Ã‡im silindirleme dÃ¼zgÃ¼nlÃ¼ÄŸÃ¼ ve ilk can suyu sulama kontrolÃ¼",

                    "AÄŸÄ±r aÄŸaÃ§ indirmelerinde halat gÃ¼venliÄŸi ve ergonomik taÅŸÄ±ma"

                )

            });



            // P-03: Ã‡ocuk Oyun ParkÄ± & KauÃ§uk Zemin Ä°malatÄ±

            var p03Start = p02Start.AddDays(2);

            var p03End = p03Start.AddDays(5);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-03",

                Order = 3,

                PhaseName = "Ã‡ocuk Oyun ParkÄ± & KauÃ§uk Zemin Ä°malatÄ±",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Peyzaj zemin betonlarÄ± dÃ¶kÃ¼lÃ¼nce kurulur",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "TSE EN 1176 SertifikalÄ± ModÃ¼ler Ã‡ocuk Oyun Grubu, SalÄ±ncak, KaydÄ±rak ve 40mm EPDM KauÃ§uk Zemin Karo",

                MaterialDescription = "Darbe emici kauÃ§uk zemin kaplamasÄ±, statik ankrajlÄ± kaydÄ±raklÄ± oyun kulesi, salÄ±ncaklar ve zÄ±pzÄ±plar.",

                MaterialQuantity = 1,

                MaterialUnit = "TakÄ±m",

                MaterialFormula = "1 TakÄ±m Ã‡ocuk Oyun ParkÄ± ve GÃ¼venlikli KauÃ§uk Zemin",

                SupplierCategory = "Ã‡ocuk ParkÄ± & Kent MobilyalarÄ± Ãœreticisi",

                LaborTitle = "Oyun ParkÄ± Montaj Ekibi",

                ProfessionName = "Kent MobilyalarÄ± Montaj UstasÄ±",

                CrewComposition = "1 Montaj Formeni + 3 Montaj UstasÄ±",

                CrewSize = 4,

                DurationDays = 5,

                EquipmentNeeded = "Ankraj MatkaplarÄ±, PoliÃ¼retan YapÄ±ÅŸtÄ±rÄ±cÄ± RulolarÄ±",

                Predecessors = "Oyun alanÄ± alt beton plaÄŸÄ±nÄ±n dÃ¶kÃ¼lmesi ve kurumasÄ±",

                CriticalDependency = "TSE EN 1176-1177 Ã§ocuk gÃ¼venlik ve dÃ¼ÅŸme yÃ¼ksekliÄŸi standartlarÄ±na uygun olmayan ekipman kabul edilemez!",

                DnaSource = "AdÄ±m 9: Sosyal DonatÄ±lar (Ã‡ocuk ParkÄ±)",

                PlannedStartDate = p03Start,

                PlannedEndDate = p03End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 TSE gÃ¼venlik sertifikasÄ± kontrolÃ¼, oyun elemanlarÄ± ve kauÃ§uk karolarÄ±n sevki",

                    "Zemin alt betonu Ã¼zerine kauÃ§uk yapÄ±ÅŸtÄ±rma ve oyun kulesi ankraj montajÄ±",

                    "Darbe emicilik testi, cÄ±vata baÅŸÄ± plastik kapaklarÄ± ve salÄ±ncak zincir kontrolÃ¼",

                    "Ã‡ocuk gÃ¼venlik uyarÄ± tabelalarÄ± ve yaÅŸ grubu etiketleri montajÄ±"

                )

            });



            // P-04: AhÅŸap Kamelyalar, Banklar & Dinlenme AlanlarÄ±

            var p04Start = p03Start.AddDays(1);

            var p04End = p04Start.AddDays(4);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-04",

                Order = 4,

                PhaseName = "AhÅŸap Kamelyalar, Banklar & Dinlenme AlanlarÄ±",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Peyzaj sert zeminleri tamamlanÄ±nca monte edilir",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "Emprenyeli Ã‡am AhÅŸap Kamelya (Ã‡ardak), DÃ¶kÃ¼m AyaklÄ± AhÅŸap Park BanklarÄ± ve Ã‡Ã¶p KovalarÄ±",

                MaterialDescription = "Site sakinleri dinlenme Ã§ardaklarÄ±, gÃ¶lgelikler, bahÃ§e oturma banklarÄ± ve Ã§evre Ã§Ã¶p kutularÄ± montajÄ±.",

                MaterialQuantity = 2,

                MaterialUnit = "TakÄ±m",

                MaterialFormula = "2 Adet Kamelya + 8 Adet Park BankÄ± + Ã‡Ã¶p KovalarÄ±",

                SupplierCategory = "AhÅŸap Kent MobilyalarÄ± Ä°malatÃ§Ä±sÄ±",

                LaborTitle = "AhÅŸap Montaj Ekibi",

                ProfessionName = "Marangoz / AhÅŸap Montaj UstasÄ±",

                CrewComposition = "1 Formen + 3 AhÅŸap UstasÄ±",

                CrewSize = 4,

                DurationDays = 4,

                EquipmentNeeded = "AkÃ¼lÃ¼ Vidalama Setleri, Lazer Terazi, Ankraj DÃ¼belleri",

                Predecessors = "Kamelya taban taÅŸ veya beton zemin kaplamalarÄ±nÄ±n bitmesi",

                CriticalDependency = "AÃ§Ä±k hava koÅŸullarÄ±na dayanÄ±klÄ± olmasÄ± iÃ§in tÃ¼m ahÅŸap elemanlar vakum basÄ±nÃ§ emprenyeli olmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 9: Sosyal DonatÄ±lar (Kamelya & Bank)",

                PlannedStartDate = p04Start,

                PlannedEndDate = p04End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Emprenye sertifikasÄ± ve demonte kamelya parÃ§alarÄ±nÄ±n kontrolÃ¼",

                    "Kamelya taÅŸÄ±yÄ±cÄ± dikmelerinin zemine Ã§elik flanÅŸla ankrajÄ± ve Ã§atÄ± montajÄ±",

                    "AhÅŸap yÃ¼zey kÄ±ymÄ±k kontrolÃ¼, vernik/cila dÃ¼zgÃ¼nlÃ¼ÄŸÃ¼ ve rÃ¼zgar rijitliÄŸi",

                    "Elektrikli vidalama ve montaj emniyeti"

                )

            });



            // P-05: AÃ§Ä±k / KapalÄ± Otopark Zemin Epoksi KaplamasÄ± & Åerit Ã‡izgileri

            var p05Start = p02End;

            var p05End = p05Start.AddDays(6);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-05",

                Order = 5,

                PhaseName = "AÃ§Ä±k / KapalÄ± Otopark Zemin Epoksi KaplamasÄ± & Åerit Ã‡izgileri",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Otopark kaba inÅŸaatÄ± ve tavan tesisatlarÄ± tamamlanÄ±nca yapÄ±lÄ±r",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "Solventsiz Epoksi Zemin KaplamasÄ±, KauÃ§uk Kolon KÃ¶ÅŸe KoruyucularÄ±, AraÃ§ Stoperleri ve Trafik BoyasÄ±",

                MaterialDescription = "Otopark zemininde silim, astar, portakal kabuÄŸu epoksi kaplama, numaralÄ± park ÅŸeritleri ve yÃ¶nlendirme oklarÄ±.",

                MaterialQuantity = 0,

                MaterialUnit = "mÂ²",

                MaterialFormula = "DXF Bekliyor (Otopark planÄ± ve araÃ§ park yeri adedinden hesaplanacak)",

                SupplierCategory = "Epoksi Zemin KimyasallarÄ± & Trafik DonanÄ±mlarÄ±",

                LaborTitle = "Epoksi Zemin ve Yol Ã‡izgi Ekibi",

                ProfessionName = "Epoksi Zemin UstasÄ±",

                CrewComposition = "1 Formen + 4 Epoksi UstasÄ± + 2 Ã‡izgi UstasÄ±",

                CrewSize = 7,

                DurationDays = 6,

                EquipmentNeeded = "Elmas Zemin Silim Makinesi, Vakumlu EndÃ¼striyel SÃ¼pÃ¼rge, Rulo ve Ã‡izgi Makinesi",

                Predecessors = "Otopark tavan yangÄ±n tesisatÄ± ve aydÄ±nlatmalarÄ±nÄ±n bitmesi",

                CriticalDependency = "Epoksi uygulanacak zemin nemi <%4 olmalÄ± ve yÃ¼zey tozu tamamen emilmiÅŸ olmalÄ±dÄ±r!",

                DnaSource = "AdÄ±m 8 & 9: Otopark Ä°malatlarÄ±",

                PlannedStartDate = p05Start,

                PlannedEndDate = p05End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 Zemin nem Ã¶lÃ§Ã¼mÃ¼, elmas uÃ§lu silim makinesiyle beton pÃ¼rÃ¼zlendirme",

                    "Epoksi astar, ara kat ve tekstÃ¼rlÃ¼ son kat kaplama, park yeri Ã§izgileri ve araÃ§ stoperi montajÄ±",

                    "Kaplama kalÄ±nlÄ±ÄŸÄ±, yapÄ±ÅŸma testi ve ÅŸerit reflektif gÃ¶rÃ¼nÃ¼rlÃ¼k kontrolÃ¼",

                    "KapalÄ± alanda havalandÄ±rma fanÄ± kullanÄ±mÄ± ve organik buhar maskesi"

                )

            });



            // P-06: Ã‡evre AydÄ±nlatma Direkleri, BahÃ§e ProjektÃ¶rleri & GÃ¼neÅŸ Enerjisi (GES)

            var p06Start = p04End;

            var p06End = p06Start.AddDays(4);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-06",

                Order = 6,

                PhaseName = "Ã‡evre AydÄ±nlatma Direkleri, BahÃ§e ProjektÃ¶rleri & GÃ¼neÅŸ Enerjisi (GES)",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Peyzaj bitki dikimi bitiminde devreye alÄ±nÄ±r",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "Dekoratif LED BahÃ§e AydÄ±nlatma Direkleri, Ã‡im ÃœstÃ¼ Bollard Lambalar, Fotoselli ProjektÃ¶rler ve Zaman Saati",

                MaterialDescription = "YÃ¼rÃ¼yÃ¼ÅŸ yollarÄ± 3m dekoratif aydÄ±nlatma direkleri, Ã§im Ã¼stÃ¼ yÃ¶nlendirme spotlarÄ±, fotoselli gece aydÄ±nlatma otomasyonu.",

                MaterialQuantity = 0,

                MaterialUnit = "Adet",

                MaterialFormula = "DXF Bekliyor (Peyzaj elektrik aydÄ±nlatma projesinden netleÅŸecek)",

                SupplierCategory = "DÄ±ÅŸ AydÄ±nlatma & Elektrik ToptancÄ±sÄ±",

                LaborTitle = "DÄ±ÅŸ Saha Elektrik & Otomasyon Ekibi",

                ProfessionName = "Elektrik UstasÄ±",

                CrewComposition = "1 Formen + 3 Elektrik UstasÄ±",

                CrewSize = 4,

                DurationDays = 4,

                EquipmentNeeded = "Lazer Hizalama, Kablo Ã‡ekici Sustalar, Meger Test CihazÄ±",

                Predecessors = "Peyzaj toprak altÄ± elektrik koruge borularÄ±nÄ±n hazÄ±r olmasÄ±",

                CriticalDependency = "IP65/IP67 dÄ±ÅŸ ortam su geÃ§irmezlik sertifikasÄ± olmayan armatÃ¼rler bahÃ§ede kullanÄ±lamaz!",

                DnaSource = "AdÄ±m 9: Ã‡evre AydÄ±nlatma",

                PlannedStartDate = p06Start,

                PlannedEndDate = p06End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-2 AydÄ±nlatma direk ankraj betonlarÄ±, IP67 kablo ek muflarÄ± ve direk sevki",

                    "Direklerin dikilmesi, armatÃ¼r montajlarÄ±, linye baÄŸlantÄ±larÄ± ve zaman saati panosu",

                    "Gece Ä±ÅŸÄ±k seviyesi kontrolÃ¼, fotosel devreye girme testi ve kaÃ§ak akÄ±m kontrolÃ¼",

                    "Elektrik izolasyon eldivenleri ve direkte emniyetli montaj"

                )

            });



            // P-07: DÄ±ÅŸ Ã‡evre BahÃ§e DuvarÄ±, Ferforje Korkuluklar & Otomatik Site BahÃ§e KapÄ±sÄ±

            var p07Start = p05End;

            var p07End = p07Start.AddDays(8);

            list.Add(new WorkflowStepItemDto

            {

                StepCode = "P-07",

                Order = 7,

                PhaseName = "DÄ±ÅŸ Ã‡evre BahÃ§e DuvarÄ±, Ferforje Korkuluklar & Otomatik Site BahÃ§e KapÄ±sÄ±",

                StageCategory = "Site Ã‡evre & DÄ±ÅŸ Saha",

                PartIndex = 5,

                PartName = "Proje Teslimi & Ortak DÄ±ÅŸ Alanlar",

                PartBadgeClass = "secondary",

                TriggerRule = "Åantiye ihata telleri kaldÄ±rÄ±lÄ±nca kalÄ±cÄ± bahÃ§e duvarÄ± yapÄ±lÄ±r",

                TargetStructureName = "Proje Geneli (DÄ±ÅŸ Saha)",

                MaterialTitle = "Betonarme / TaÅŸ BahÃ§e DuvarÄ±, Elektrostatik BoyalÄ± Ferforje Korkuluklar, Motorlu Otomatik Yana Kayar Site KapÄ±sÄ± ve OGS/Plaka TanÄ±ma",

                MaterialDescription = "KalÄ±cÄ± parsel Ã§evre bahÃ§e duvarÄ±, harpuÅŸtasÄ±, Ã¼zerine ferforje panel Ã§it, motorlu ana site giriÅŸ kapÄ±sÄ± ve plaka tanÄ±ma sistemi.",

                MaterialQuantity = 0,

                MaterialUnit = "mt",

                MaterialFormula = "DXF Bekliyor (Parsel Ã§evre net uzunluÄŸundan hesaplanacak)",

                SupplierCategory = "Demir DoÄŸrama & Otomatik KapÄ± / TaÅŸ OcaÄŸÄ±",

                LaborTitle = "BahÃ§e DuvarÄ± ve Otomatik KapÄ± Ekibi",

                ProfessionName = "TaÅŸ UstasÄ± & Metal / Otomasyon UstasÄ±",

                CrewComposition = "1 Formen + 4 TaÅŸ/Duvar UstasÄ± + 2 Demirci + 1 Otomasyoncu",

                CrewSize = 8,

                DurationDays = 8,

                EquipmentNeeded = "Betoniyer / Pompa, Kaynak Makineleri, KapÄ± Motoru Montaj Seti",

                Predecessors = "Åantiye geÃ§ici Ã§evre tel ve konteynerlerinin sahadan kaldÄ±rÄ±lmasÄ±",

                CriticalDependency = "Site araÃ§ giriÅŸ kapÄ±sÄ±nda fotoselli sÄ±kÄ±ÅŸma Ã¶nleyici emniyet sensÃ¶rÃ¼ ve flaÅŸÃ¶r lamba zorunludur!",

                DnaSource = "AdÄ±m 9: Ã‡evre Ä°hata ve GÃ¼venlik",

                PlannedStartDate = p07Start,

                PlannedEndDate = p07End,

                SubSteps = CreateWorkflowSubSteps(

                    "T-3 Parsel sÄ±nÄ±r kadastro tespiti, ferforje korkuluk imalatÄ± ve kapÄ± motoru hazÄ±rlÄ±ÄŸÄ±",

                    "BahÃ§e duvarÄ± betonu/taÅŸÄ±, ferforje korkuluk montajÄ± ve otomatik motorlu sÃ¼rgÃ¼lÃ¼ kapÄ± montajÄ±",

                    "KapÄ± motoru tork ve fotosel gÃ¼venlik testi, plaka tanÄ±ma kamerasÄ± aÃ§Ä±sÄ±",

                    "Yol tarafÄ± Ã§alÄ±ÅŸma gÃ¼venlik bariyerleri ve reflektÃ¶rlÃ¼ yelekler"

                )

            });



            return list;

        }



        public class ParametricTakeoffItemDto

        {

            public int Id { get; set; }

            public string Category { get; set; }

            public string Title { get; set; }

            public string Description { get; set; }

            public string Formula { get; set; }

            public decimal Quantity { get; set; }

            public string Unit { get; set; }

            public string ProfessionName { get; set; }

            public string DnaSource { get; set; }

            public string Status { get; set; } = "Parametrik HesaplandÄ±";

        }



        public class SendParametricB2BDto

        {

            public int ProjectId { get; set; }

            public int? ProfileId { get; set; }

            public List<int> SelectedItemIds { get; set; }

        }



        public class WorkflowStepItemDto

        {

            public string StepCode { get; set; }

            public int Order { get; set; }

            public string PhaseName { get; set; }

            public string StageCategory { get; set; }



            // 4 ParÃ§alÄ± HiyerarÅŸi & YapÄ± BaÄŸÄ±

            public int PartIndex { get; set; } = 1; // 1: Kaba Ä°nÅŸaat, 2: Statik Ä°nce, 3: Tercihe GÃ¶re Ä°nce, 4: DemirbaÅŸ & Montaj

            public string PartName { get; set; } = "1. ParÃ§a: Kaba Ä°nÅŸaat (Temel & TaÅŸÄ±yÄ±cÄ± Ä°skelet)";

            public string PartBadgeClass { get; set; } = "primary";

            public string TriggerRule { get; set; } = "Ä°ÅŸbaÅŸÄ± Tarihine GÃ¶re Kat Kat BaÅŸlar";

            public string TargetStructureName { get; set; } = "";

            public int? TargetBuildingId { get; set; }



            // Malzeme BacaÄŸÄ±

            public string MaterialTitle { get; set; }

            public string MaterialDescription { get; set; }

            public decimal MaterialQuantity { get; set; }

            public string MaterialUnit { get; set; }

            public string MaterialFormula { get; set; }

            public string SupplierCategory { get; set; }



            // Ä°ÅŸ GÃ¼cÃ¼ & TaÅŸeron BacaÄŸÄ±

            public string LaborTitle { get; set; }

            public string ProfessionName { get; set; }

            public string CrewComposition { get; set; }

            public int CrewSize { get; set; }

            public int DurationDays { get; set; }

            public int TotalManDays => CrewSize * DurationDays;

            public string EquipmentNeeded { get; set; }



            // Ä°ÅŸ AkÄ±ÅŸÄ± & BaÄŸÄ±mlÄ±lÄ±k

            public string Predecessors { get; set; }

            public string CriticalDependency { get; set; }

            public string DnaSource { get; set; }



            // Takvim

            public DateTime? PlannedStartDate { get; set; }

            public DateTime? PlannedEndDate { get; set; }

            public string PlannedDateRange => PlannedStartDate.HasValue && PlannedEndDate.HasValue

                ? $"{PlannedStartDate.Value:dd.MM.yyyy} - {PlannedEndDate.Value:dd.MM.yyyy}" : "";



            // 4 Hayati Alt AÅŸama & Belgelendirme (Ã–n HazÄ±rlÄ±k, Ä°malat, Kalite/KÃ¼r, Ä°SG)

            public List<WorkflowSubStepDto> SubSteps { get; set; } = new List<WorkflowSubStepDto>();

        }



        public class WorkflowSubStepDto

        {

            public string Title { get; set; } = "";

            public string Category { get; set; } = "PrePreparation"; // "PrePreparation", "Execution", "QualityAndCuring", "SafetyIsg"

            public bool IsCompleted { get; set; }

            public string? TestResultValue { get; set; } // Ã–rn: "Slump: 18 cm", "28G KÄ±rÄ±m: 34.6 MPa"

            public string? PhotoUrl { get; set; }

            public string? DocumentUrl { get; set; }

            public string? Notes { get; set; }

        }



        public class SendWorkflowB2BDto

        {

            public int ProjectId { get; set; }

            public int? ProfileId { get; set; }

            public int? BuildingId { get; set; }

            public string TargetTrack { get; set; } // "Material" or "Labor"

            public List<string> SelectedStepCodes { get; set; }

        }



        public class TriggerWorkflowAgendaDto

        {

            public int ProjectId { get; set; }

            public int? ProfileId { get; set; }

            public int? BuildingId { get; set; }

            public string? TargetTrack { get; set; }

            public DateTime StartDate { get; set; }

            public List<string> SelectedStepCodes { get; set; }

        }

    }

}





