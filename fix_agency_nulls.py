import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerDashboardController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """            var agency = new Agency
            {
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = model.LogoUrl ?? "https://via.placeholder.com/150",
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
            };"""

content = re.sub(r'var agency = new Agency\s*\{[^}]+\};', replacement, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Controller with dummy values.")
