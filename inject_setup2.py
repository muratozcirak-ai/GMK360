import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerDashboardController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I need to completely replace the SetupCorporateProfile methods with the correct ones.
# First, remove the bad ones.
import re
content = re.sub(r'\[HttpGet\]\s*public IActionResult SetupCorporateProfile\(\)[\s\S]*?return RedirectToAction\("Index", "ConstructionProject"\);\s*\}', '', content)

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
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
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
                IsActive = true,
                Role = AgencyRole.Owner,
                StartDate = DateTime.UtcNow
            };

            _context.AgencyConsultants.Add(consultant);
            await _context.SaveChangesAsync();

            // Firma kurulumu bittiğinde yönlendir
            return RedirectToAction("Index", "ConstructionProject");
        }
"""

content = content.replace("public async Task<IActionResult> Referrals()", new_methods + "\n        public async Task<IActionResult> Referrals()")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed SetupCorporateProfile methods.")
