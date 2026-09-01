import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerDashboardController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add standard using for IWebHostEnvironment
if "using Microsoft.AspNetCore.Hosting;" not in content:
    content = content.replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.Hosting;\nusing System.IO;\nusing Microsoft.AspNetCore.Http;")
    
# Inject IWebHostEnvironment into the constructor
if "IWebHostEnvironment" not in content:
    content = content.replace("public CustomerDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)", "private readonly IWebHostEnvironment _hostEnvironment;\n\n        public CustomerDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)")
    content = content.replace("_userManager = userManager;", "_userManager = userManager;\n            _hostEnvironment = hostEnvironment;")

# Replace SetupCorporateProfile POST method logic to handle LogoFile
new_logic = """
            string uploadedLogoUrl = "-"; // Default
            
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                // Upload dizinini ayarla
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agencies", "logos");
                Directory.CreateDirectory(uploadsFolder);
                
                // Dosya adını benzersiz yap
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.LogoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.LogoFile.CopyToAsync(fileStream);
                }
                
                uploadedLogoUrl = "/uploads/agencies/logos/" + uniqueFileName;
            }

            // 1. Yeni Ajans/Firma Oluştur
            var agency = new Agency
            {
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = uploadedLogoUrl, // DOSYADAN GELEN URL
                Subdomain = model.Subdomain,
                TaxNumber = model.TaxNumber, // Yeni eklenen Vergi Numarası
                AuthCertificateNo = "MUAF",
                
                // NOT NULL Kısıtlamasını aşmak için dummy değerler
                BannerUrl = "-",
                CustomDomain = "-",
                ThemePrimaryColor = "#f97316",
                ThemeSecondaryColor = "#1e3a8a",
                FontFamily = "Inter",
                FooterLogoUrl = "-",
                FaviconUrl = "-",
                ThemeAccentColor = "#ffffff"
            };
"""

content = re.sub(r'// 1\. Yeni Ajans/Firma Oluştur\s*var agency = new Agency\s*\{[^}]+\};', new_logic, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Controller for file upload.")
