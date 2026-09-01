import re
import os

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix ManageBlock GET method
old_get_query = """            // Mimari görselleri ve kat planlarını getir
            var architectureDocs = await _context.DmsDocuments
                .Where(d => (d.EntityType == "Building" && d.EntityId == id.ToString()) ||
                            (d.EntityType == "BuildingFloor" && d.EntityId.StartsWith(id.ToString() + "_")))
                .ToListAsync();"""

new_get_query = """            // Mimari görselleri ve kat planlarını getir
            string floorEntityType = "BuildingFloor_" + id;
            var architectureDocs = await _context.DmsDocuments
                .Where(d => (d.EntityType == "Building" && d.EntityId == id) ||
                            (d.EntityType == floorEntityType))
                .ToListAsync();"""

if old_get_query in content:
    content = content.replace(old_get_query, new_get_query)
else:
    # try fallback regex
    content = re.sub(r'var architectureDocs = await _context.DmsDocuments.*?\.ToListAsync\(\);', new_get_query, content, flags=re.DOTALL)


# Fix UploadArchitectureMedia Method
old_upload = """        [HttpPost]
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
                };"""

new_upload = """        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadArchitectureMedia(int buildingId, string entityType, int entityId, string title, IFormFile file)
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
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User),
                    UploadDate = DateTime.UtcNow
                };"""

if old_upload in content:
    content = content.replace(old_upload, new_upload)
else:
    # Try regex again if exact match fails
    content = re.sub(
        r'\[HttpPost\].*?public async Task<IActionResult> UploadArchitectureMedia.*?IsActive = true.*?};', 
        new_upload, 
        content, 
        flags=re.DOTALL
    )

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ConstructionProjectController Dms fixes")
