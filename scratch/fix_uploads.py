with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

start_idx = content.find('string baseFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}");')
end_idx = content.find('await _context.SaveChangesAsync();', start_idx)

if start_idx != -1 and end_idx != -1:
    new_code = '''string baseFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", $"Agency_{agencyId.Value}", $"Project_{project.Id}");
                string imagesFolder = Path.Combine(baseFolder, "images");
                string docsFolder = Path.Combine(baseFolder, "documents");
                Directory.CreateDirectory(imagesFolder);
                Directory.CreateDirectory(docsFolder);

                // Helper to sanitize project name for filenames
                string safeProjName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
                safeProjName = safeProjName.Replace(" ", "_");

                // --- 1. COVER IMAGE (GELECEGİ HALİ) ---
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    // Find and remove old archive and file if exists
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Proje Görseli (Geleceği Hali)");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.CoverImageFile.FileName);
                    string standardizedName = $"{safeProjName}_GelecekHali_1{extension}";
                    string filePath = Path.Combine(imagesFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImageFile.CopyToAsync(fileStream);
                    }
                    var archive = new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Proje Görseli (Geleceği Hali)",
                        Category = "Bina Görselleri",
                        SourceModule = "Construction",
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",
                        FileName = standardizedName,
                        FileExtension = extension,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                    project.CoverImageUrl = archive.DocumentUrl;
                }

                // --- 2. CURRENT STATE IMAGE (MEVCUT DURUM / İLK HALİ) ---
                if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)
                {
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Mevcut Durum Görseli (İlk Hali)");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.CurrentStateImageFile.FileName);
                    string standardizedName = $"{safeProjName}_EskiHali_1{extension}";
                    string filePath = Path.Combine(imagesFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CurrentStateImageFile.CopyToAsync(fileStream);
                    }
                    var archive = new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Mevcut Durum Görseli (İlk Hali)",
                        Category = "Bina Görselleri",
                        SourceModule = "Construction",
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{standardizedName}",
                        FileName = standardizedName,
                        FileExtension = extension,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                    project.CurrentStateImageUrl = archive.DocumentUrl;
                }

                // --- 3. TAPU DOCUMENT (TAPU BELGESİ) ---
                if (model.TapuDocumentFile != null && model.TapuDocumentFile.Length > 0)
                {
                    var oldArchive = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.Title == "Tapu Belgesi");
                    if (oldArchive != null)
                    {
                        var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldArchive.DocumentUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        _context.DocumentArchives.Remove(oldArchive);
                    }

                    string extension = Path.GetExtension(model.TapuDocumentFile.FileName);
                    string standardizedName = $"{safeProjName}_TapuBelgesi{extension}";
                    string filePath = Path.Combine(docsFolder, standardizedName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.TapuDocumentFile.CopyToAsync(fileStream);
                    }
                    var archive = new GMK360.Core.Entities.DocumentArchive
                    {
                        AgencyId = agencyId.Value,
                        ProjectId = project.Id,
                        Title = "Tapu Belgesi",
                        Category = "Resmi Evraklar",
                        SourceModule = "Construction",
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/documents/{standardizedName}",
                        FileName = standardizedName,
                        FileExtension = extension,
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                }
                
                '''
    content = content[:start_idx] + new_code + content[end_idx:]
    with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Rewritten successfully!")
else:
    print("Could not find the indices!")
