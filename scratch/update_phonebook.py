import sys

filepath = 'GMK360.Core/Entities/Construction/AgencyPhonebook.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public string? LinkedUserId { get; set; }"
replacement = """public string? LinkedUserId { get; set; }

        // --- ADRES VE KONUM ---
        [MaxLength(500)]
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GoogleMapsUrl { get; set; }

        // --- İLETİŞİM DETAYLARI (ÇOKLU) ---
        [MaxLength(20)]
        public string? MobilePhone2 { get; set; } 
        [MaxLength(20)]
        public string? LandlinePhone { get; set; } 
        [MaxLength(10)]
        public string? ExtensionNumber { get; set; } 
        [MaxLength(100)]
        public string? WebsiteUrl { get; set; }

        // --- KURUMSAL BİLGİLER ---
        [MaxLength(100)]
        public string? AuthorizedPerson { get; set; } 
        [MaxLength(50)]
        public string? AuthorizedPersonRole { get; set; } 
        [MaxLength(100)]
        public string? TaxOffice { get; set; }
        [MaxLength(50)]
        public string? TaxNumber { get; set; }
        [MaxLength(50)]
        public string? Iban { get; set; }"""

if "MobilePhone2" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AgencyPhonebook updated.")
else:
    print("Already updated.")
