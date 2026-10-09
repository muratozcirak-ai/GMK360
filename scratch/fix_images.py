import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix CurrentStateImageFile block: change CoverImageUrl to CurrentStateImageUrl
pattern = r'(if\s*\(model\.CurrentStateImageFile\s*!=\s*null\s*&&\s*model\.CurrentStateImageFile\.Length\s*>\s*0\).*?_context\.DocumentArchives\.Add\(archive\);\s*)project\.CoverImageUrl(\s*=\s*archive\.DocumentUrl;)'
content = re.sub(pattern, r'\1project.CurrentStateImageUrl\2', content, flags=re.DOTALL)

# Now, we need to ADD the CoverImageFile block BEFORE CurrentStateImageFile block
cover_image_block = '''
                if (model.CoverImageFile != null && model.CoverImageFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.CoverImageFile.FileName);
                    string filePath = Path.Combine(imagesFolder, uniqueFileName);
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
                        DocumentUrl = $"/uploads/Agency_{agencyId.Value}/Project_{project.Id}/images/{uniqueFileName}",
                        FileName = Path.GetFileName(model.CoverImageFile.FileName),
                        FileExtension = Path.GetExtension(model.CoverImageFile.FileName),
                        UploadDate = DateTime.UtcNow,
                        Status = "Tamamlandı"
                    };
                    _context.DocumentArchives.Add(archive);
                    project.CoverImageUrl = archive.DocumentUrl;
                }

'''

# Insert it before CurrentStateImageFile block
content = content.replace('if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)', cover_image_block + 'if (model.CurrentStateImageFile != null && model.CurrentStateImageFile.Length > 0)')

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("Added CoverImageFile handling and fixed CurrentStateImageFile bug!")
