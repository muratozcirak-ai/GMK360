using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Update SaveStep1
        string oldSaveStep1 = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim. Oturumunuz kapanmış olabilir."" });

            
            // AYNI ADRESTE BİNA/PROJE VAR MI KONTROLÜ (DİJİTAL İKİZ KURALI)
            // Not: İleride doğrudan Building veya Address tablolarından kontrol edilecek.
            bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                p.Address == model.Address && 
                p.Name != model.Name // Şimdilik basit Address metni üzerinden kontrol
            );
            
            // Eğer daha hassas bir kontrol isteniyorsa İl, İlçe vs. Building üzerinden yapılabilir
            // Şimdilik test için Address stringi veya Name üzerinden sembolik bir kalkan koyuyoruz
            if (addressExists)
            {
                return Json(new { success = false, message = ""Bu adreste halihazırda kayıtlı bir yapı/proje bulunmaktadır. Aynı adrese ikinci bir bina eklenemez!"" });
            }
            var project = new ConstructionProject
            {
                Name = model.Name,
                Description = model.Description,
                AgencyId = agencyId.Value,
                Address = model.Address ?? """",
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                TotalLandArea = model.TotalLandArea,
                Status = 0 // Draft / Upcoming
            };

            _context.ConstructionProjects.Add(project);
            await _context.SaveChangesAsync();

            return Json(new { success = true, projectId = project.Id });
        }";

        string newSaveStep1 = @"
        [HttpPost]
        public async Task<IActionResult> SaveStep1([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Json(new { success = false, message = ""Yetkisiz erişim. Oturumunuz kapanmış olabilir."" });

                bool addressExists = await _context.ConstructionProjects.AnyAsync(p => 
                    p.Address == model.Address && 
                    p.Name != model.Name 
                );
                
                if (addressExists)
                {
                    return Json(new { success = false, message = ""Bu adreste halihazırda kayıtlı bir yapı/proje bulunmaktadır. Aynı adrese ikinci bir bina eklenemez!"" });
                }

                var project = new ConstructionProject
                {
                    Name = model.Name,
                    Description = model.Description,
                    AgencyId = agencyId.Value,
                    Address = model.Address ?? """",
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    TotalLandArea = model.TotalLandArea,
                    Status = 0 // Draft
                };

                _context.ConstructionProjects.Add(project);
                await _context.SaveChangesAsync();

                return Json(new { success = true, projectId = project.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ""Kayıt sırasında sistemsel bir hata oluştu: "" + ex.Message + (ex.InnerException != null ? "" - "" + ex.InnerException.Message : """") });
            }
        }";

        if (code.Contains("public async Task<IActionResult> SaveStep1"))
        {
            int startIdx = code.IndexOf("[HttpPost]\r\n        public async Task<IActionResult> SaveStep1");
            if (startIdx == -1) startIdx = code.IndexOf("[HttpPost]\n        public async Task<IActionResult> SaveStep1");

            if (startIdx != -1) {
                int endIdx = code.IndexOf("return Json(new { success = true, projectId = project.Id });", startIdx);
                if (endIdx != -1) {
                    endIdx = code.IndexOf("}", endIdx) + 1;
                    code = code.Remove(startIdx, endIdx - startIdx);
                    code = code.Insert(startIdx, newSaveStep1);
                    Console.WriteLine("Wrapped SaveStep1 in try/catch.");
                }
            }
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
    }
}
