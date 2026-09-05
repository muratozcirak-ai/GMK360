using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string saveMethods = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim."" });

            GMK360.Core.Entities.Construction.ConstructionProject project;
            if (model.DraftProjectId > 0)
            {
                project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
                if (project == null) return Json(new { success = false, message = ""Proje bulunamadı."" });
            }
            else
            {
                project = new GMK360.Core.Entities.Construction.ConstructionProject { AgencyId = agencyId.Value, Status = 0, CreatedAt = DateTime.UtcNow };
                _context.ConstructionProjects.Add(project);
            }

            project.Name = model.Name;
            project.Description = model.Description;
            project.Address = model.Address ?? ""Adres belirtilmedi"";
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;
            project.TotalLandArea = model.TotalLandArea;
            project.CityId = model.CityId;
            project.DistrictId = model.DistrictId;
            project.NeighborhoodId = model.NeighborhoodId;
            project.StreetId = model.StreetId;
            project.TargetTotalApartments = model.TargetTotalApartments;
            project.TargetTotalShops = model.TargetTotalShops;
            project.Latitude = model.Latitude;
            project.Longitude = model.Longitude;

            await _context.SaveChangesAsync();
            return Json(new { success = true, draftId = project.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SaveStep2([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            if (model.DraftProjectId == 0) return Json(new { success = false, message = ""Proje ID bulunamadı."" });
            var project = await _context.ConstructionProjects.Include(p => p.Blocks).FirstOrDefaultAsync(p => p.Id == model.DraftProjectId);
            if (project == null) return Json(new { success = false, message = ""Proje bulunamadı."" });

            if (project.Blocks != null && project.Blocks.Any())
            {
                _context.Buildings.RemoveRange(project.Blocks);
            }

            if (model.Blocks != null)
            {
                int blockCounter = 1;
                foreach (var b in model.Blocks)
                {
                    var building = new GMK360.Core.Entities.Building
                    {
                        Name = project.Name + "" - "" + b.BlockName,
                        BlockName = b.BlockName,
                        BuildingNumber = blockCounter.ToString(),
                        BaseArea = b.BaseArea,
                        BasementFloors = b.BasementFloors,
                        TotalFloors = b.TotalFloors,
                        TotalUnits = b.TotalApartments + b.TotalShops,
                        HasBlock = true,
                        HasRoof = b.HasRoof,
                        HasGroundFloor = b.HasGroundFloor,
                        ConstructionProjectId = project.Id,
                        CityId = project.CityId ?? 34,
                        DistrictId = project.DistrictId ?? 1,
                        NeighborhoodId = project.NeighborhoodId ?? 1,
                        StreetId = project.StreetId,
                        CreatedAt = DateTime.UtcNow,
                        ManagerUserId = _userManager.GetUserId(User) ?? """"
                    };
                    _context.Buildings.Add(building);
                    blockCounter++;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        ";

        if (!code.Contains("SaveStep1"))
        {
            int insertIndex = code.IndexOf("public async Task<IActionResult> CreateWizard");
            if (insertIndex != -1)
            {
                code = code.Insert(insertIndex, saveMethods);
                File.WriteAllText(path, code, new UTF8Encoding(true));
                Console.WriteLine("Restored SaveStep1 and SaveStep2 methods!");
            }
        }
    }
}
