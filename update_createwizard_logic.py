import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Update the method signature to use [FromForm] instead of [FromBody]
content = content.replace("public async Task<IActionResult> CreateWizard([FromBody] GMK360.Web.Models.CreateProjectWizardViewModel model)", "public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)")
content = content.replace("public async Task<IActionResult> CreateWizard(GMK360.Web.Models.CreateProjectWizardViewModel model)", "public async Task<IActionResult> CreateWizard([FromForm] GMK360.Web.Models.CreateProjectWizardViewModel model)")

# Now inject the image upload logic before _context.Add(project)
new_logic = """
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            string uploadedImageUrl = "";

            if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "projects", "covers");
                Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageFile.CopyToAsync(fileStream);
                }
                
                uploadedImageUrl = "/uploads/projects/covers/" + uniqueFileName;
            }

            var project = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                Name = model.Name,
                Description = model.Description,
                Address = model.Address,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CoverImageUrl = uploadedImageUrl,
                AgencyId = agencyId.Value,
                Status = 0, // 0 = Upcoming
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(project);
            await _context.SaveChangesAsync();

            // DMS Sistemine Belge Ekleme (Proje Görseli)
            if (!string.IsNullOrEmpty(uploadedImageUrl))
            {
                var dmsDoc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = "Proje Görseli",
                    DocumentUrl = uploadedImageUrl,
                    FileExtension = Path.GetExtension(model.CoverImageFile.FileName),
                    FileSizeBytes = model.CoverImageFile.Length,
                    EntityType = "ConstructionProject",
                    EntityId = project.Id,
                    UploadDate = DateTime.UtcNow,
                    UploadedByUserId = _userManager.GetUserId(User),
                    FolderId = 1 // Varsayılan veya DB'den gelen bir klasör
                };
                
                var defaultFolder = _context.DmsFolders.FirstOrDefault();
                if (defaultFolder != null)
                {
                    dmsDoc.FolderId = defaultFolder.Id;
                    _context.DmsDocuments.Add(dmsDoc);
                    await _context.SaveChangesAsync();
                }
            }
"""

# The existing CreateWizard logic looks something like this:
#             var agencyId = await GetUserAgencyIdAsync();
#             if (agencyId == null) return Unauthorized();
#             
#             var project = new GMK360.Core.Entities.Construction.ConstructionProject
#             { ... }
#             project.AgencyId = agencyId.Value; ... _context.Add(project); await _context.SaveChangesAsync();
# I'll use regex to replace this entire block up to the block creation loop.

content = re.sub(r'var agencyId = await GetUserAgencyIdAsync\(\);\s*if \(agencyId == null\) return Unauthorized\(\);\s*var project = new GMK360\.Core\.Entities\.Construction\.ConstructionProject.*?await _context\.SaveChangesAsync\(\);', new_logic, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated CreateWizard controller logic.")
