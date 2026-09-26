import sys

filepath = 'GMK360.Core/Entities/Construction/AgencyPhonebook.cs'

with open(filepath, 'r', encoding='windows-1254', errors='ignore') as f:
    content = f.read()

target = "public string? LinkedUserId { get; set; }"
replacement = """public string? LinkedUserId { get; set; }

        // --- ADRES VE KONUM ---
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GoogleMapsUrl { get; set; }

        // --- ILETISIM DETAYLARI (COKLU) ---
        public string? MobilePhone2 { get; set; } 
        public string? LandlinePhone { get; set; } 
        public string? ExtensionNumber { get; set; } 
        public string? WebsiteUrl { get; set; }

        // --- KURUMSAL BILGILER ---
        public string? AuthorizedPerson { get; set; } 
        public string? AuthorizedPersonRole { get; set; } 
        public string? TaxOffice { get; set; }
        public string? TaxNumber { get; set; }
        public string? Iban { get; set; }"""

if "MobilePhone2" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AgencyPhonebook updated.")
else:
    print("Already updated.")
