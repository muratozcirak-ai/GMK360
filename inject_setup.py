import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerDashboardController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_methods = """
        [HttpGet]
        public IActionResult SetupCorporateProfile()
        {
            return View(new GMK360.Web.Models.SetupCorporateProfileViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupCorporateProfile(GMK360.Web.Models.SetupCorporateProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Yeni Ajans/Firma Oluştur
            var agency = new Agency
            {
                Name = model.CompanyName,
                Phone = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CityId = 1, // Şimdilik default
                DistrictId = 1, // Şimdilik default
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = model.LogoUrl,
                Subdomain = model.Subdomain
            };

            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            // 2. Kullanıcıyı Firmaya Bağla
            var consultant = new AgencyConsultant
            {
                AgencyId = agency.Id,
                UserId = user.Id,
                Status = 1,
                RoleInAgency = "Owner",
                JoinedAt = DateTime.UtcNow
            };

            _context.AgencyConsultants.Add(consultant);
            await _context.SaveChangesAsync();

            // Firma kurulumu bittiğinde yönlendir
            return RedirectToAction("Index", "ConstructionProject");
        }
"""

if "SetupCorporateProfile" not in content:
    # Insert before the last closing brace of the class
    content = content.replace("public async Task<IActionResult> Referrals()", new_methods + "\n        public async Task<IActionResult> Referrals()")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Injected SetupCorporateProfile methods.")
else:
    print("Already injected.")
