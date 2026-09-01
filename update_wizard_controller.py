import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add missing using statements if not present
if "using Microsoft.AspNetCore.Hosting;" not in content:
    content = content.replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.Hosting;\nusing System.IO;\nusing Microsoft.AspNetCore.Http;")

# Inject IWebHostEnvironment
if "IWebHostEnvironment" not in content:
    content = content.replace("public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)", "private readonly IWebHostEnvironment _hostEnvironment;\n\n        public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)")
    content = content.replace("_userManager = userManager;", "_userManager = userManager;\n            _hostEnvironment = hostEnvironment;")

# Replace CreateWizard POST method logic to handle CoverImageFile and DMS saving
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
                Status = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(project);
            await _context.SaveChangesAsync();

            // Eğer dosya yüklendiyse DMS sistemine (Belge Arşivi) kaydedelim
            if (!string.IsNullOrEmpty(uploadedImageUrl))
            {
                var dmsDoc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = "Proje Görseli / Mimari Çizim",
                    DocumentUrl = uploadedImageUrl,
                    FileExtension = Path.GetExtension(model.CoverImageFile.FileName),
                    FileSizeBytes = model.CoverImageFile.Length,
                    EntityType = "ConstructionProject",
                    EntityId = project.Id,
                    UploadDate = DateTime.UtcNow,
                    UploadedByUserId = _userManager.GetUserId(User),
                    FolderId = 1 // Şimdilik varsayılan bir klasör id (ya da nullable ise null kalabilir)
                };
                
                // NOT: FolderId yabancı anahtar kısıtlamasına takılabilir, eğer veritabanında 1 id'li klasör yoksa
                // Daha güvenlisi FolderId'yi ayırmamak veya opsiyonel yapmak. Şimdilik DocumentRegistry'ye de eklenebilir.
                // FolderId'yi null yapabilmek için DmsDocument'te int? olmalı, ancak değil. 
                // O yüzden bir Dummy Folder oluşturalım:
                var folder = _context.DmsFolders.FirstOrDefault();
                if (folder == null)
                {
                    folder = new GMK360.Core.Entities.DmsFolder { Name = "Genel Arşiv", CreatedAt = DateTime.UtcNow, ShelfId = 1 };
                    // Shelf de yoksa yaratmak zorundayız. Bu karmaşayı önlemek için DMS'i şimdilik pas geçip SystemDocument veya salt Property'de tutabiliriz.
                }
            }
"""

content = re.sub(r'var agencyId = await GetUserAgencyIdAsync\(\);\s*if \(agencyId == null\) return Unauthorized\(\);\s*var project = new GMK360\.Core\.Entities\.Construction\.ConstructionProject\s*\{[^}]+\};\s*var agencyId2 = await GetUserAgencyIdAsync\(\);\s*if \(agencyId2 == null\) return Unauthorized\(\);\s*project\.AgencyId = agencyId2\.Value;.*?_context\.Add\(project\);\s*await _context\.SaveChangesAsync\(\);', new_logic, content, flags=re.DOTALL)
# wait, my regex target is messy because it has double agencyId checks from previous patches.
# Let's use a simpler target.
