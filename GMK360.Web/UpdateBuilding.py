import re

with open(r"..\GMK360.Core\Entities\Building.cs", "r", encoding="utf-8") as f:
    content = f.read()

find_str = """        public string? InsulationType { get; set; } // Mantolama/Yalıtım
        public int? ElevatorCount { get; set; } // Asansör Sayısı
        public string? ParkingType { get; set; } // Otopark (Açık, Kapalı, Yok)
 // Örn: 2 Asansör, Yük Asansörü, vs."""

replace_str = """        public string? InsulationType { get; set; } // Mantolama/Yalıtım
        public int? ElevatorCount { get; set; } // Asansör Sayısı
        public string? ParkingType { get; set; } // Otopark (Açık, Kapalı, Yok)

        // Nizam (Layout) ve Bitişiklik Mantığı
        public string? LayoutPattern { get; set; } // Ayrık Nizam, Bitişik Nizam, İkiz Nizam, Blok Nizam
        public string? AttachedToBlock { get; set; } // Eğer Bitişik Nizam ise hangi bloğa bitişik? (Örn: A Blok)"""

content = content.replace(find_str, replace_str)

with open(r"..\GMK360.Core\Entities\Building.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated Building.cs")
