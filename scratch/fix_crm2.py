import codecs
import re

path = 'GMK360.Web/Controllers/AdminCRMController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

new_action = '''
        // --- TODO 3: KAPSAMLI PROFİL VE İLİŞKİLER EKRANI ---
        public async Task<IActionResult> UserDetails(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return NotFound();

                var user = await _context.Users
                    .Include(u => u.Subscriptions)
                        .ThenInclude(s => s.Package)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Kullanıcının Paydaş (Stakeholder) olduğu tüm inşaat projeleri
                var projects = await _context.ProjectStakeholders
                    .Include(ps => ps.Project)
                    .Where(ps => ps.UserId == id && !ps.IsDeleted)
                    .ToListAsync();

                // Kullanıcının bağlı olduğu Taşeron/Ajans rolleri
                var agencyRoles = await _context.AgencyConsultants
                    .Include(ac => ac.Agency)
                    .Where(ac => ac.UserId == id && !ac.IsDeleted)
                    .ToListAsync();

                ViewBag.Projects = projects;
                ViewBag.AgencyRoles = agencyRoles;

                return View(user);
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Kullanıcı detayları yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
'''

content = re.sub(r'}\s*}\s*$', new_action + '\n    }\n}\n', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed!')