import codecs

path = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with codecs.open(path, 'r', 'utf-8') as f:
    content = f.read()

# 1. Inject ViewBag.Stakeholders in the Details method
target_details = '''            ViewBag.B2bConnections = await _context.B2BNetworkConnections
                .Include(c => c.B2bCompany)
                .Where(c => c.AgencyId == agencyId)
                .ToListAsync();'''

replacement_details = target_details + '''
            ViewBag.Stakeholders = await _context.ProjectStakeholders
                .Include(s => s.User)
                .Where(s => s.ProjectId == id && !s.IsDeleted)
                .ToListAsync();'''

content = content.replace(target_details, replacement_details)

# 2. Append AddStakeholder method at the end
add_stakeholder_method = '''

        // --- YENİ PAYDAŞ (GÖLGE KULLANICI) EKLEME ---
        [HttpPost]
        public async Task<IActionResult> AddStakeholder(int projectId, int role, decimal? sharePercentage, string firstName, string lastName, string phone, string email)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) 
                {
                    TempData["ErrorMessage"] = "Yetki Hatası: Şantiye yöneticisi veya admin yetkiniz bulunamadı.";
                    return RedirectToAction("Details", new { id = projectId });
                }

                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone || (!string.IsNullOrEmpty(email) && u.Email == email));

                if (existingUser == null)
                {
                    string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                    if (!cleanPhone.StartsWith("+90") && !cleanPhone.StartsWith("0")) cleanPhone = "0" + cleanPhone;
                    
                    string generatedUserName = string.IsNullOrEmpty(email) ? cleanPhone : email;
                    string generatedEmail = string.IsNullOrEmpty(email) ? $"{cleanPhone}@gmk360.local" : email;
                    
                    existingUser = new ApplicationUser
                    {
                        UserName = generatedUserName,
                        Email = generatedEmail,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = phone,
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false
                    };

                    var result = await _userManager.CreateAsync(existingUser, "Shadow.User2026!");
                    if (!result.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Gölge kullanıcı oluşturulurken hata oluştu.";
                        return RedirectToAction("Details", new { id = projectId });
                    }
                    await _userManager.AddToRoleAsync(existingUser, "Musteri");
                }

                var existingStakeholder = await _context.ProjectStakeholders
                    .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.UserId == existingUser.Id);

                if (existingStakeholder == null)
                {
                    var stakeholder = new GMK360.Core.Entities.Construction.ProjectStakeholder
                    {
                        ProjectId = projectId,
                        UserId = existingUser.Id,
                        Role = (GMK360.Core.Entities.Construction.StakeholderRole)role,
                        SharePercentage = (role == 1) ? sharePercentage : null,
                        Notes = "Firma tarafından davet edildi (Gölge Onay Bekliyor)"
                    };
                    
                    _context.ProjectStakeholders.Add(stakeholder);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"{firstName} {lastName} projeye başarıyla eklendi! Sisteme giriş yapması için SMS gönderimi tetiklendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bu kişi zaten bu projenin paydaşı!";
                }

                return RedirectToAction("Details", new { id = projectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sistem Hatası: Lütfen bilgileri kontrol edin.";
                return RedirectToAction("Details", new { id = projectId });
            }
        }
'''

if 'public async Task<IActionResult> AddStakeholder' not in content:
    content = content.replace('    }\r\n}', add_stakeholder_method + '    }\r\n}')
    content = content.replace('    }\n}', add_stakeholder_method + '    }\n}')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Successfully fixed controller!')