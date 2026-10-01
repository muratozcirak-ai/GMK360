import io
import re

filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update GetDoc to return filePath
old_getdoc = """additionalCost = doc.AdditionalCost ?? 0
            });"""
new_getdoc = """additionalCost = doc.AdditionalCost ?? 0,
                filePath = doc.FilePath
            });"""
content = content.replace(old_getdoc, new_getdoc)

# 2. Update UpdateDoc signature and logic to handle file upload
old_updatedoc_sig = """public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal documentFee, decimal additionalCost)"""
new_updatedoc_sig = """public async Task<IActionResult> UpdateDoc(int id, string status, string assignedUserId, string institutionContact, decimal documentFee, decimal additionalCost, Microsoft.AspNetCore.Http.IFormFile uploadedFile)"""
content = content.replace(old_updatedoc_sig, new_updatedoc_sig)

old_updatedoc_body = """doc.DocumentFee = documentFee;
            doc.AdditionalCost = additionalCost;

            await _context.SaveChangesAsync();"""
new_updatedoc_body = """doc.DocumentFee = documentFee;
            doc.AdditionalCost = additionalCost;

            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faz0");
                if (!System.IO.Directory.Exists(uploadsFolder))
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = System.Guid.NewGuid().ToString() + "_" + uploadedFile.FileName;
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                doc.FilePath = "/uploads/faz0/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();"""
content = content.replace(old_updatedoc_body, new_updatedoc_body)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZeroController.cs successfully.")
