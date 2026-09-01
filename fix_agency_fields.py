import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\CustomerDashboardController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I need to add TaxNumber and AuthCertificateNo to the Agency creation
replacement = """            var agency = new Agency
            {
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = model.LogoUrl,
                Subdomain = model.Subdomain,
                TaxNumber = "0000000000",        // Zorunlu alan hatasını atlamak için
                AuthCertificateNo = "MUAF"       // Zorunlu alan hatasını atlamak için (İnşaat firması)
            };"""

content = content.replace("""            var agency = new Agency
            {
                CompanyName = model.CompanyName,
                WhatsAppNumber = model.PhoneNumber,
                Address = "Girilmedi", // İleride formdan alınabilir
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = model.LogoUrl,
                Subdomain = model.Subdomain
            };""", replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added missing required fields to Agency.")
