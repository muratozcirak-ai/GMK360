import codecs

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

new_action = '''
        [HttpPost]
        public async Task<IActionResult> EditStakeholder(int stakeholderId, GMK360.Core.Entities.Construction.StakeholderRole role, decimal? sharePercentage)
        {
            int projectId = 0;
            try
            {
                var st = await _context.ProjectStakeholders.FirstOrDefaultAsync(s => s.Id == stakeholderId);
                if (st == null)
                {
                    TempData["ErrorMessage"] = "Paydaş bulunamadı.";
                    return RedirectToAction("Index");
                }
                
                projectId = st.ProjectId;
                st.Role = role;
                st.SharePercentage = sharePercentage;
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Paydaş bilgileri başarıyla güncellendi.";
                return RedirectToAction("Details", new { id = projectId });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Paydaş düzenlenirken hata oluştu: " + ex.Message;
                return projectId > 0 ? RedirectToAction("Details", new { id = projectId }) : RedirectToAction("Index");
            }
        }
'''

if 'EditStakeholder' not in content:
    import re
    # Add it before the final closing brace of the class. The controller ends with two closing braces.
    content = re.sub(r'}\s*}\s*$', new_action + '\n    }\n}\n', content)
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Added EditStakeholder POST action!")
else:
    print("Already exists")