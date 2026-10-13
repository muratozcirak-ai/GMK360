using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GMK360.Web.Controllers
{
    public class PhaseTwoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhaseTwoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("PhaseTwo/Index/{projectId}")]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();

            ViewData["ProjectId"] = project.Id;
            ViewData["ProjectName"] = project.Name;
            ViewData["BypassedPhases"] = project.BypassedPhases;
            ViewBag.LinkedDocs = await _context.ProjectLegalDocuments.Where(d => d.ConstructionProjectId == projectId && d.LinkedPhaseCategory == GMK360.Core.Entities.Construction.BudgetPhaseCategory.TemelVeAltYapi).ToListAsync();
            ViewBag.PhaseId = (int)GMK360.Core.Entities.Construction.BudgetPhaseCategory.TemelVeAltYapi;

            var items = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId && b.PhaseCategory == BudgetPhaseCategory.TemelVeAltYapi)
                .ToListAsync();

            var templates = await _context.SystemPhaseTemplates.Where(t => t.PhaseCategory == BudgetPhaseCategory.TemelVeAltYapi).ToListAsync();
            ViewData["HasTemplates"] = templates.Any();

            var itemIds = items.Select(i => i.Id).ToList();
            var quoteRequests = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .Where(q => q.SourceModule == "PhaseTwoBudget" && itemIds.Contains(q.SourceReferenceId))
                .ToListAsync();
            ViewBag.Quotes = quoteRequests;

            // Sadece Fizibilite Aşaması İçin Listeyi Gönderiyoruz
            return View(items);
        }

        [HttpPost("PhaseTwo/AddItem")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddItem(int projectId, string subCategory, string itemName, decimal plannedUnitPrice)
        {
            var item = new ConstructionBudgetItem
            {
                ConstructionProjectId = projectId,
                PhaseCategory = BudgetPhaseCategory.TemelVeAltYapi,
                SubCategory = subCategory,
                ItemName = itemName,
                PlannedUnitPrice = plannedUnitPrice,
                Quantity = 1,
                Unit = "Adet",
                SourceType = BudgetItemSourceType.Manual,
                QuoteStatus = BudgetQuoteStatus.WaitingForPrice
            };
            _context.ConstructionBudgetItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpPost("PhaseTwo/SyncFromPool/{projectId}")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SyncFromPool(int projectId)
        {
            var existingItems = await _context.ConstructionBudgetItems
                .Where(b => b.ConstructionProjectId == projectId && b.PhaseCategory == BudgetPhaseCategory.TemelVeAltYapi)
                .Select(b => b.ItemName)
                .ToListAsync();

            var templates = await _context.SystemPhaseTemplates
                .Where(t => t.PhaseCategory == BudgetPhaseCategory.TemelVeAltYapi && !existingItems.Contains(t.ItemName))
                .ToListAsync();

            foreach(var t in templates)
            {
                var item = new ConstructionBudgetItem
                {
                    ConstructionProjectId = projectId,
                    PhaseCategory = BudgetPhaseCategory.TemelVeAltYapi,
                    SubCategory = t.SubCategory,
                    ItemName = t.ItemName,
                    Quantity = 1,
                    Unit = "Adet",
                    SourceType = BudgetItemSourceType.Manual,
                    QuoteStatus = t.IsQuoteRequired ? BudgetQuoteStatus.WaitingForPrice : BudgetQuoteStatus.EstimatedOrQuoted
                };
                _context.ConstructionBudgetItems.Add(item);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = projectId });
        }

        [HttpPost("PhaseTwo/UpdatePrice")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePrice(int id, decimal totalCost, decimal quantity, string unit, string description, decimal? estimatedMaterialCost, decimal? estimatedLaborCost)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Quantity = quantity > 0 ? quantity : 1;
            item.Unit = string.IsNullOrEmpty(unit) ? "Götürü" : unit;
            item.PlannedUnitPrice = totalCost / item.Quantity; // Calculate unit price from total
            
            item.Description = description;
            item.EstimatedMaterialCost = estimatedMaterialCost;
            item.EstimatedLaborCost = estimatedLaborCost;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseTwo/AcceptQuote")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AcceptQuote(int inviteId, int budgetItemId)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(budgetItemId);
            if (item == null) return NotFound();

            var invite = await _context.B2BQuoteInvites.FindAsync(inviteId);
            if (invite == null) return NotFound();

            invite.IsFeasibilitySelected = true;
            item.PlannedUnitPrice = invite.OfferedPrice ?? 0;
            item.QuoteStatus = BudgetQuoteStatus.EstimatedOrQuoted;
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId = item.ConstructionProjectId });
        }

        [HttpPost("PhaseTwo/DeleteItem/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.ConstructionBudgetItems.FindAsync(id);
            if (item != null)
            {
                _context.ConstructionBudgetItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { projectId = item?.ConstructionProjectId });
        }
    
        [HttpGet("PhaseTwo/OperationBoard/{projectId}")]
        public async Task<IActionResult> OperationBoard(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();
            
            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;
            
            // Read actual tasks to pass to View if generated
            var tasks = await _context.ProjectScheduleTasks
                .Include(t => t.PrePreparations)
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Id) // Basic ordering for now
                .ToListAsync();

            return View(tasks);
        }

        [IgnoreAntiforgeryToken]
        [HttpGet("PhaseTwo/GenerateOperationTasks/{projectId}")]
        public async Task<IActionResult> GenerateOperationTasks(int projectId, bool isZeminAsmolen = false, bool isNormalKatsAsmolen = false)
        {
            var project = await _context.ConstructionProjects
                .Include(p => p.Blocks)
                    .ThenInclude(b => b.ChildBuildings)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null) return NotFound("Proje bulunamadi.");

            // DEBUG
            // return Ok("Ulastik");

            var parentBuilding = project.Blocks?.FirstOrDefault();
            
            // Temizlik
            var existingTasks = _context.ProjectScheduleTasks.Where(x => x.ProjectId == projectId).ToList();
            var existingSubs = _context.ProjectScheduleSubTasks.Where(x => existingTasks.Select(t => t.Id).Contains(x.ProjectScheduleTaskId)).ToList();
            _context.ProjectScheduleSubTasks.RemoveRange(existingSubs);
            _context.ProjectScheduleTasks.RemoveRange(existingTasks);
            await _context.SaveChangesAsync();

            var tasks = new List<GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask>();
            
            int basementFloors = parentBuilding?.BasementFloors ?? 2;
            bool hasGroundFloor = parentBuilding?.HasGroundFloor ?? true;
            
            // 1. Zemin Etüdü ve Saha Ölçümleri (Genel - Baza)
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Santiye Cevre Kapama ve Saha Mobilizasyonu", FloorOrZone = "Saha Genel", Status = "Pending" });
            if (project.ProjectOriginId == 1 || (project.EskiKatSayisi.HasValue && project.EskiKatSayisi.Value > 0))
            {
                tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Mevcut Bina Yikimi ve Moloz Nakliyesi", FloorOrZone = "Saha Genel", Status = "Pending" });
            }
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Saha Ölçümleri ve Harita Aplikasyon İşleri", FloorOrZone = "Saha Genel", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Hafriyat, Kazı ve Zemin Düzeltme İşleri", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            
            
            // Bohcalama Detayi (Guncellenmis)
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Grobeton Dokumu (Yalitim Alti Hazirlik)", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: Koselere Pah Yapilmasi", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: Astar Surulmesi", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: 1. ve 2. Kat Membran Serilmesi", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: XPS/EPS Isi Yalitim Levhalari", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: Geotekstil Kece Serilmesi", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: Koruma Betonu Dokumu", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Bohcalama: Drenaj Hatti ve Micirlama", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });

            // Temel
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Temel (Radye) Demir Donati Isleri", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Temel (Radye) Cevre Kalibi Kurulumu", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Temel Ici Elektrik/Topraklama ve Sihhi Tesisat Altyapisi", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });
            tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Temel Beton Dokumu", FloorOrZone = $"Baza / Kot -{basementFloors}", Status = "Pending" });

            var towers = parentBuilding?.ChildBuildings?.Any() == true 
                            ? parentBuilding.ChildBuildings.ToList() 
                            : new List<GMK360.Core.Entities.Building> { parentBuilding };
            
            int maxTotalFloors = 0;
            foreach(var t in towers) {
                int maxF = t.TotalFloors ?? 5; // Simplified
                if(maxF > maxTotalFloors) maxTotalFloors = maxF;
            }

            for(int i = 1; i <= maxTotalFloors; i++) {
                foreach(var tower in towers) {
                    int maxF = tower.TotalFloors ?? 5;
                    if(i > maxF) continue; // Bu kule bu kata kadar cikmiyor.

                    string towerPrefix = towers.Count > 1 ? $"{tower.BlockName} Kulesi / " : "";
                    string katName = $"{towerPrefix}Kat {i}";

                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Kolon/Kiris Demir Donati Isleri", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Kolon/Kiris Kalip Isleri", FloorOrZone = katName, Status = "Pending" });
                    
                    if (isNormalKatsAsmolen)
                    {
                        tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Asmolen (Bims/EPS) Dizimi ve Iskele/Kalip", FloorOrZone = katName, Status = "Pending" });
                        tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Asmolen Nervur Demir Donati Isleri", FloorOrZone = katName, Status = "Pending" });
                    }
                    else
                    {
                        tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Doseme Iskele ve Kalip Kurulumu", FloorOrZone = katName, Status = "Pending" });
                        tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Doseme Demir Donati Isleri", FloorOrZone = katName, Status = "Pending" });
                    }
                    
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Doseme Ici Elektrik ve Sihhi Tesisat Borulama", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Kat Beton Dokumu", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Dis ve Ic Cephe Duvar Orumu", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Elektrik ve Mekanik Duvar Ici Alt Yapi (Kirim/Boru)", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Kaba ve Ince Siva Isleri", FloorOrZone = katName, Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Zemin Sap Dokumu", FloorOrZone = katName, Status = "Pending" });
                }
            }

            foreach(var tower in towers) {
                if (tower.HasRoof)
                {
                    string towerPrefix = towers.Count > 1 ? $"{tower.BlockName} Kulesi / " : "";
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Cati Iskeleti ve Izolasyon", FloorOrZone = $"{towerPrefix}Cati (Baca)", Status = "Pending" });
                    tasks.Add(new GMK360.Core.Entities.Construction.Workflows.ProjectScheduleTask { OrderIndex = tasks.Count + 1, ProjectId = projectId, TaskName = "Cati Kaplama ve Yagmur Inisleri", FloorOrZone = $"{towerPrefix}Cati (Baca)", Status = "Pending" });
                }
            }


            _context.ProjectScheduleTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            return RedirectToAction("OperationBoard", new { projectId = projectId });
        }

        [HttpGet("PhaseTwo/IsgChecklist/{projectId}")]
        public async Task<IActionResult> IsgChecklist(int projectId)
        {
            var project = await _context.ConstructionProjects.FindAsync(projectId);
            if (project == null) return NotFound();
            
            ViewData["ProjectId"] = projectId;
            ViewData["ProjectName"] = project.Name;
            
            var isgQuestions = await _context.MasterIsgQuestions.Where(q => true).ToListAsync();
            
            return View(isgQuestions);
        }
        }
    }

