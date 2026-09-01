import re
import os

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_methods = """
        // --- MİMARİ GÖRSELLER VE KAT PLANLARI YÖNETİMİ ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, string entityIdStr, string title, IFormFile file)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null || (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "architecture");
                Directory.CreateDirectory(uploadsFolder);
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = "/uploads/architecture/" + uniqueFileName,
                    DocumentType = file.ContentType,
                    EntityType = entityType, // "Building" (Dış Cephe) veya "BuildingFloor" (Kat Planı)
                    EntityId = entityIdStr,  // buildingId veya buildingId_floorLevel
                    UploadedByUserId = _userManager.GetUserId(User),
                    UploadDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.DmsDocuments.Add(doc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{title} başarıyla yüklendi.";
            }

            return RedirectToAction("ManageBlock", new { id = buildingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArchitectureMedia(int documentId, int buildingId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .FirstOrDefaultAsync(b => b.Id == buildingId);

            if (block == null || (!User.IsInRole("Admin") && block.ConstructionProject.AgencyId != agencyId))
            {
                return Unauthorized();
            }

            var doc = await _context.DmsDocuments.FindAsync(documentId);
            if (doc != null)
            {
                // Dosyayı sunucudan silme (opsiyonel)
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, doc.DocumentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.DmsDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Görsel başarıyla silindi.";
            }

            return RedirectToAction("ManageBlock", new { id = buildingId });
        }
"""

# Insert before the last closing brace of the class
# We'll replace the GenerateUnits method block ending and insert it there.
content = content.replace('// GET: ConstructionProject/Edit/5', new_methods + '\n        // GET: ConstructionProject/Edit/5')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added Architecture Media methods.")
