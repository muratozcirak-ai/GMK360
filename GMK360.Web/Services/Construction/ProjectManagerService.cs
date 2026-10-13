using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction.Workflows;

namespace GMK360.Web.Services.Construction
{
    public class ProjectManagerService
    {
        private readonly ApplicationDbContext _context;

        public ProjectManagerService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =================================================================================
        // OTO-PİLOT MATRİS OLUŞTURUCU (KARAR MOTORU)
        // Şef projeyi başlattığında, DNA'ya göre terzi işi iş akışı çizen metod.
        // =================================================================================
        
        // =================================================================================
        // BAZA-KULE (AĞAÇ) MİMARİSİ OLUŞTURUCU
        // Şefin girdiği kat sayılarına göre binanın hiyerarşik (Parent-Child) iskeletini çizer.
        // =================================================================================
        public void GenerateElevationTree(int projectId, int basementCount, int baseFloorCount, int towerA_Floors, int towerB_Floors)
        {
            // 1. ORTAK BAZA KATLARINI OLUŞTUR (Gövde)
            // Bodrumlar (-2, -1) ve Ticari Baza Katları (0, 1, 2)
            ProjectElevation lastBaseElevation = null;
            
            for (int i = -basementCount; i <= baseFloorCount; i++)
            {
                var baseElevation = new ProjectElevation
                {
                    ProjectId = projectId,
                    BlockName = "Ortak Baza",
                    Name = i < 0 ? $"{i}. Bodrum" : (i == 0 ? "Zemin Kat" : $"Baza {i}. Kat"),
                    ElevationLevel = i,
                    // Zincir Mantığı: Her kat, bir alt katın (Parent) üstüne biner.
                    ParentElevationId = lastBaseElevation?.Id 
                };
                
                _context.ProjectElevations.Add(baseElevation);
                _context.SaveChanges(); // ID alabilmek için hemen kaydediyoruz
                
                lastBaseElevation = baseElevation; // Bir sonrakinin Annesi (Parent) bu olacak
            }

            // DİKKAT: lastBaseElevation şu an Ortak Baza'nın en üst katı! (Örn: Baza 2. Kat)
            // Şimdi bu noktadan itibaren ağaç ikiye (Kulelere) dallanacak.
            
            if (lastBaseElevation == null) return;

            // 2. KULELERİ DALLANDIR (Ağacın Dalları)
            
            // --- A KULESİ ---
            ProjectElevation lastTowerAElevation = lastBaseElevation; // Kökü bazanın tepesine bağladık!
            for (int i = 1; i <= towerA_Floors; i++)
            {
                int currentLevel = baseFloorCount + i;
                var towerAElev = new ProjectElevation
                {
                    ProjectId = projectId,
                    BlockName = "A Kulesi",
                    Name = $"A Kulesi {currentLevel}. Kat",
                    ElevationLevel = currentLevel,
                    ParentElevationId = lastTowerAElevation.Id // Zincir devam ediyor
                };
                _context.ProjectElevations.Add(towerAElev);
                _context.SaveChanges();
                lastTowerAElevation = towerAElev;
            }

            // --- B KULESİ ---
            ProjectElevation lastTowerBElevation = lastBaseElevation; // Kökü YİNE bazanın tepesine bağladık!
            for (int i = 1; i <= towerB_Floors; i++)
            {
                int currentLevel = baseFloorCount + i;
                var towerBElev = new ProjectElevation
                {
                    ProjectId = projectId,
                    BlockName = "B Kulesi",
                    Name = $"B Kulesi {currentLevel}. Kat",
                    ElevationLevel = currentLevel,
                    ParentElevationId = lastTowerBElevation.Id // Zincir devam ediyor
                };
                _context.ProjectElevations.Add(towerBElev);
                _context.SaveChanges();
                lastTowerBElevation = towerBElev;
            }
        }

        
        // =================================================================================
        // MASTER METOD: PROJEYİ AYAĞA KALDIRAN (BÜYÜK PATLAMA) METODU
        // =================================================================================
        public void InitializeProject(int projectId, int basementCount, int baseFloorCount, int towerA_Floors, int towerB_Floors)
        {
            // 1. İskeleti (Ağacı) Kur (Adım 3'te yazdığımız metod)
            GenerateElevationTree(projectId, basementCount, baseFloorCount, towerA_Floors, towerB_Floors);
            
            // Veritabanına iskeletin (katların) kaydedildiğinden emin ol.
            
            // 2. Projenin yeni kurulan tüm katlarını (Elevation) çek
            var projectElevations = _context.ProjectElevations.Where(e => e.ProjectId == projectId).ToList();
            
            // 3. Her bir kat için KARAR MOTORUNU çalıştır ve Görevleri (Yaprakları) diz!
            foreach (var elevation in projectElevations)
            {
                // Adım 2'de yazdığımız metod (İçinde WorkGroupCategory aktarımını da yapmayı unutma!)
                GenerateMatrixForElevation(projectId, elevation.Id); 
            }
            
            // İŞTE ŞİMDİ PROJE (MATRİS) TAMAMEN HAZIR VE SAHAYA İNMEYE UYGUN!
        }


        // =================================================================================
        // ÇEVİK YÖNLENDİRME (AGILE REROUTING) ALGORİTMASI
        // =================================================================================

        // =================================================================================
        // DXF METRAJ KÖPRÜSÜ: ÇİZİMİ ŞANTİYE GERÇEKLİĞİNE ÇEVİREN ALGORİTMA
        // =================================================================================

        // =================================================================================
        // HAKEDİŞ (PROGRESS) ONAY MOTORU VE FİNANS TETİĞİ
        // =================================================================================

        // =================================================================================
        // CRM TETİĞİ: SEÇİMİ UYGULA VE FARK FATURASINI HESAPLA
        // =================================================================================
        public void ApplyCustomerSelection(int customerSelectionId)
        {
            // 1. Müşterinin seçim kaydını (Standart ve Seçilen malzemeleri) getir
            var selection = _context.CustomerSelections
                .Include(s => s.CustomerUnit)
                .Include(s => s.StandardMasterMaterial)
                .FirstOrDefault(s => s.Id == customerSelectionId);
                
            if (selection == null || !selection.IsApprovedByCustomer) return;

            // 2. Bu daireye ait, bu kategorideki kilitli ("Müşteri Bekliyor") görevi bul
            var task = _context.ProjectTasks
                .FirstOrDefault(t => t.CustomerUnitId == selection.CustomerUnitId 
                                  && t.WorkGroupCategory == selection.SelectionCategory 
                                  && t.Status == "Müşteri Bekliyor");

            if (task != null)
            {
                // 3. KİLİDİ AÇ: Görevi başlatılabilir duruma getir
                task.Status = "Bekliyor";
                
                // 4. Görevin içine, müşterinin seçtiği malzemenin kodunu yaz (Usta ne yapacağını bilsin)
                var material = _context.MasterMaterials.FirstOrDefault(m => m.Id == selection.SelectedMasterMaterialId);
                if (material != null)
                {
                    task.WorkItemCode = material.MaterialCode; 
                    
                    // 5. SATINALMAYA SİPARİŞİ GEÇ! (Daire m2'si kadar)
                    var demand = new GMK360.Core.Entities.Construction.Workflows.ProjectMaterialDemand
                    {
                        ProjectId = selection.CustomerUnit.ProjectId,
                        ElevationId = selection.CustomerUnit.ElevationId,
                        MasterMaterialId = material.Id,
                        TotalRequiredQuantity = selection.CustomerUnit.NetAreaSquareMeters, 
                        IsOrdered = false,
                        IsDeliveredToSite = false
                    };
                    _context.ProjectMaterialDemands.Add(demand);
                }
                
                // 6. FİNANS/CRM BİLDİRİMİ: Fark faturası var mı?
                if (selection.UpgradePriceDifference > 0)
                {
                    // İleride Satış departmanının paneline "Ahmet Bey'den 25.000 TL tahsil edilecek" DTO'su atılır.
                    // Şimdilik sadece sistemi kilitlenmekten kurtardık!
                }
                
                _context.SaveChanges();
            }
        }

        public void ApproveTaskProgress(int progressLogId)
        {
            // 1. Ustanın girdiği günlük raporu bul
            var log = _context.TaskProgressLogs.FirstOrDefault(l => l.Id == progressLogId);
            if (log == null || log.IsApprovedByChief) return;

            // 2. İlgili görevi ve Taşeron Sözleşmesini bul
            var task = _context.ProjectTasks.FirstOrDefault(t => t.Id == log.ProjectTaskId);
            if (task == null) return;
            
            var contract = _context.WorkItemSubcontractorContracts
                .FirstOrDefault(c => c.ProjectId == task.ProjectId 
                                  && c.SubcontractorId == log.SubcontractorId 
                                  && c.WorkItemCode == task.WorkItemCode);

            if (contract != null)
            {
                // 3. FİNANSAL HESAPLAMA: (Yapılan Miktar) * (Sözleşmedeki Birim Fiyat)
                // Örn: 200 m2 * 150 TL = 30.000 TL Hakediş doğdu!
                log.CalculatedEarnedValue = log.CompletedQuantity * contract.UnitPrice;
                log.IsApprovedByChief = true;

                // 4. GÖREV İLERLEMESİNİ GÜNCELLE
                task.CompletedQuantity += log.CompletedQuantity;

                // 5. EĞER YAPILAN MİKTAR, DXF'TEN GELEN HEDEFİ BULDUYSA İŞİ BİTİR!
                if (task.CompletedQuantity >= task.TotalQuantity)
                {
                    task.Status = "Bitti";
                }
                else
                {
                    task.Status = "Aktif";
                }

                _context.SaveChanges();
            }
        }

        public void ProcessDxfTakeoffData(int projectId, List<GMK360.Core.DTOs.Construction.DxfTakeoffDto> dxfDataList)
        {
            foreach (var dxfData in dxfDataList)
            {
                // 1. İlgili Kat ve Malzeme Koduna ait GÖREVİ (Task) bul
                // Örn: 3. Katta, DXF'ten gelen "MAT-YTONG-20" işini bul.
                var task = _context.ProjectTasks
                    .FirstOrDefault(t => t.ProjectId == projectId 
                                      && t.ElevationId == dxfData.ElevationId 
                                      && t.WorkItemCode == dxfData.MaterialCode);

                if (task != null)
                {
                    // ŞEFE GİDEN VERİ (Hakediş Temeli): Görevin net metrajını (m2) güncelle
                    task.TotalQuantity = dxfData.Quantity;

                    // 2. REÇETEYİ BUL VE SATINALMA TALEBİ OLUŞTUR
                    // Sistem 500 m2 Ytong'u okudu, peki bunun içine ne kadar kum, ne kadar blok girer?
                    var recipe = _context.MaterialRecipes
                        .Include(r => r.Ingredients)
                        .FirstOrDefault(r => r.WorkItemCode == dxfData.MaterialCode);

                    if (recipe != null)
                    {
                        foreach (var ingredient in recipe.Ingredients)
                        {
                            // Çarpım: 500 m2 (DXF) * 6.5 (Reçete) = 3250 Adet Ytong Blok
                            decimal calculatedNeed = dxfData.Quantity * ingredient.Quantity;

                            var demand = new ProjectMaterialDemand
                            {
                                ProjectId = projectId,
                                ElevationId = dxfData.ElevationId,
                                MasterMaterialId = ingredient.MasterMaterialId,
                                TotalRequiredQuantity = calculatedNeed,
                                IsOrdered = false,
                                IsDeliveredToSite = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            _context.ProjectMaterialDemands.Add(demand);
                        }
                    }
                }
            }
            
            _context.SaveChanges();
        }

        public List<ProjectTask> GetSmartAlternativeTasks(int projectId, string blockedWorkGroup)
        {
            // Örn: blockedWorkGroup = "Kaba_Demir" (Demirci ekibi boşta kaldı)
            
            var alternativeTasks = _context.ProjectTasks
                .Include(t => t.Elevation) 
                .Where(t => t.ProjectId == projectId
                         && t.Status == "Bekliyor" // Başlanmamış
                         && t.WorkGroupCategory == blockedWorkGroup // Uzmanlık eşleşiyor
                         && t.IsMaterialReady == true) // **MALZEMESİ HAZIR (En kritik şart)**
                .ToList();

            // İSG Kilitli katları tavsiye etme!
            alternativeTasks = alternativeTasks.Where(t => t.Elevation == null || !t.Elevation.IsLockedByIsg).ToList();

            // Alt katlardaki işlere (Örn: Zemin kat istinat duvarı vb.) öncelik ver
            return alternativeTasks.OrderBy(t => t.Elevation?.ElevationLevel ?? -999).ToList(); 
        }

        public void GenerateMatrixForElevation(int projectId, int elevationId)
        {
            // 1. Projenin DNA'sını (Künyesini) Getir
            var project = _context.ConstructionProjects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return;

            // 2. Havuzdaki Tüm Şablonları (WorkflowTemplates) Getir
            var allTemplates = _context.WorkflowTemplates.OrderBy(w => w.StepOrder).ToList();

            // 3. Matris'e eklenecek "Geçerli" görevler listesi
            var activeTasks = new List<ProjectTask>();

            foreach (var template in allTemplates)
            {
                // KARAR MOTORU: FİLTRELEME MANTIĞI
                bool isEligible = false;

                // Eğer şablonun özel bir şartı (RequiredCondition) YOKSA, herkese uyar (Örn: Kolon Kalıbı)
                if (string.IsNullOrEmpty(template.RequiredCondition))
                {
                    isEligible = true;
                }
                // Eğer şablonun şartı VARSA, Proje DNA'sı ile eşleşiyor mu kontrol et!
                else
                {
                    switch (template.RequiredCondition)
                    {
                        case "OuterWallType_Ytong":
                            if (project.OuterWallType == "Ytong") isEligible = true;
                            break;
                        case "OuterWallType_Tugla":
                            if (project.OuterWallType == "Tugla") isEligible = true;
                            break;
                        case "PlumbingType_Gomme":
                            if (project.PlumbingType == "Gomme") isEligible = true;
                            break;
                        case "ElevatorType_Klasik":
                            if (project.ElevatorType == "Klasik") isEligible = true;
                            break;
                        
                        case "FoundationInsulation_XPS_Köpük":
                            if (project.FoundationInsulation == "XPS_Köpük") isEligible = true;
                            break;
                        case "FacadeInsulation_Dış_Mantolama":
                            if (project.FacadeInsulation == "Dış_Mantolama") isEligible = true;
                            break;
                        case "RoofMaterial_OSB_Şingıl":
                            if (project.RoofMaterial == "OSB_Şingıl") isEligible = true;
                            break;
                        case "RoofMaterial_Çelik_SandviçPanel":
                            if (project.RoofMaterial == "Çelik_SandviçPanel") isEligible = true;
                            break;
                        // Yeni varyasyonlar buraya eklenecek...
                    }
                }

                // 4. Şablon DNA'yı geçtiyse, Aktif Görev (ProjectTask) olarak Matris'e ekle!
                if (isEligible)
                {
                    var newTask = new ProjectTask
                    {
                        ProjectId = project.Id,
                        ElevationId = elevationId,
                        TaskName = template.TaskName,
                        Status = "Bekliyor",
                        WorkflowType = template.WorkflowType,
                        WorkGroupCategory = template.WorkGroupCategory
                    };
                    
                    activeTasks.Add(newTask);
                }
            }

            // 5. Hazırlanan Terzi İşi Görevleri Veritabanına Bas
            _context.ProjectTasks.AddRange(activeTasks);
            _context.SaveChanges();
            
            // BİTTİ! Şefin önüne çıkacak olan hatasız, eksiksiz, projeye özel iş matrisi hazır.
        }
    }
}
