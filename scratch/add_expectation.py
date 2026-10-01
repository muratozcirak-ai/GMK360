import io
import re

filepath = r'GMK360.Core\Entities\Construction\ConstructionProject.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

new_fields = """
        public double? TotalLandArea { get; set; } // Toplam Arsa Alanı (m2)
        public double? LandscapeArea { get; set; } // Peyzaj Alanı (m2)

        // Bütçe / Satış Beklenti (Fizibilite)
        public decimal AverageFlatSalePrice { get; set; } = 0; // Ortalama Daire Fiyatı
        public decimal ExpectedTotalShopRevenue { get; set; } = 0; // Toplam Dükkan Beklentisi
"""

content = content.replace("        public double? TotalLandArea { get; set; } // Toplam Arsa Alanı (m2)\n        public double? LandscapeArea { get; set; } // Peyzaj Alanı (m2)", new_fields.strip('\n'))

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
