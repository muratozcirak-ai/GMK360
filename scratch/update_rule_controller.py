import sys

filepath = 'GMK360.Web/Controllers/ModuleDocumentRuleController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the Index fetching
index_old = """            var rules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .OrderBy(r => r.TargetModule)
                .ThenBy(r => r.Stage)
                .ThenBy(r => r.DisplayOrder)
                .ToListAsync();"""

index_new = """            var rules = await _context.ModuleDocumentRules
                .Include(r => r.SystemLegalDocumentTemplate)
                .Include(r => r.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteTemplate)
                .OrderBy(r => r.TargetModule)
                .ThenBy(r => r.Stage)
                .ThenBy(r => r.DisplayOrder)
                .ToListAsync();"""
content = content.replace(index_old, index_new)

# Replace the Create action body
create_old = """        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("PrerequisiteTemplate");
            
            if (ModelState.IsValid)
            {
                if (PrerequisiteTemplateIdsList != null && PrerequisiteTemplateIdsList.Length > 0)
                {
                    var validIds = PrerequisiteTemplateIdsList.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                    if (validIds.Any())
                    {
                        model.PrerequisiteTemplateIds = string.Join(",", validIds);
                    }
                    else
                    {
                        model.PrerequisiteTemplateIds = null;
                    }
                }

                _context.ModuleDocumentRules.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kural başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kural eklenirken bir hata oluştu. Tüm alanları doldurduğunuzdan emin olun.";
            }
            return RedirectToAction(nameof(Index));
        }"""

create_new = """        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("Prerequisites");
            
            if (ModelState.IsValid)
            {
                _context.ModuleDocumentRules.Add(model);
                await _context.SaveChangesAsync(); // First save to get Rule Id

                if (PrerequisiteTemplateIdsList != null && PrerequisiteTemplateIdsList.Length > 0)
                {
                    var validIds = PrerequisiteTemplateIdsList.Where(id => !string.IsNullOrWhiteSpace(id)).Select(int.Parse).Distinct().ToList();
                    foreach(var prId in validIds)
                    {
                        _context.ModuleDocumentRulePrerequisites.Add(new ModuleDocumentRulePrerequisite {
                            ModuleDocumentRuleId = model.Id,
                            PrerequisiteTemplateId = prId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Kural ve ön koşulları başarıyla eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kural eklenirken bir hata oluştu. Tüm alanları doldurduğunuzdan emin olun.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePrerequisite(int id)
        {
            var pr = await _context.ModuleDocumentRulePrerequisites.FindAsync(id);
            if(pr != null)
            {
                _context.ModuleDocumentRulePrerequisites.Remove(pr);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bağımlılık (Ön Koşul) başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }"""
content = content.replace(create_old, create_new)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
