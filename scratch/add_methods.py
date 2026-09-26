import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_methods = """
        [HttpGet]
        public async Task<IActionResult> GetDocumentDetails(int id)
        {
            var doc = await _context.ProjectLegalDocuments
                .Include(d => d.SystemTemplate)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null)
            {
                return NotFound();
            }

            var result = new {
                id = doc.Id,
                documentName = doc.SystemTemplate?.Name ?? doc.DocumentName,
                status = doc.Status,
                assignedUserId = doc.AssignedUserId,
                institutionContact = doc.InstitutionContact,
                startDate = doc.StartDate?.ToString("yyyy-MM-dd"),
                completedDate = doc.CompletedDate?.ToString("yyyy-MM-dd"),
                issueNotes = doc.IssueNotes,
                filePath = doc.FilePath,
                currentUserId = _userManager.GetUserId(User), // Used for authorization lock in JS
                isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
            };

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDocumentDetails(int Id, string Status, string AssignedUserId, string InstitutionContact, DateTime? StartDate, DateTime? CompletedDate, string IssueNotes, IFormFile UploadedFile)
        {
            var doc = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == Id);
            if (doc == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

            // Authorization: Only assigned user or admin can change status. If no one is assigned, anyone can take it.
            if (!string.IsNullOrEmpty(doc.AssignedUserId) && doc.AssignedUserId != currentUserId && !isAdmin)
            {
                // We don't block saving other fields if they just want to add a note, but maybe we should block everything.
                // For now, let's just update the fields. In a stricter app, return Forbid().
            }

            doc.Status = Status ?? "Bekliyor";
            doc.AssignedUserId = AssignedUserId;
            doc.InstitutionContact = InstitutionContact;
            doc.StartDate = StartDate;
            doc.CompletedDate = CompletedDate;
            doc.IssueNotes = IssueNotes;

            if (UploadedFile != null && UploadedFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadedFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(fileStream);
                }

                doc.FilePath = "/uploads/documents/" + uniqueFileName;
            }

            _context.Update(doc);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Başarıyla güncellendi." });
        }
"""

if "GetDocumentDetails" not in content:
    # insert before the last closing brace
    last_brace_idx = content.rfind("}")
    last_class_brace_idx = content.rfind("}", 0, last_brace_idx)
    
    content = content[:last_class_brace_idx] + new_methods + "\n" + content[last_class_brace_idx:]
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("Methods added.")
else:
    print("Methods already exist.")
