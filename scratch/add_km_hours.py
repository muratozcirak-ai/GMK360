import codecs

path = 'GMK360.Core/Entities/Logistics/CompanyVehicle.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''        public string Status { get; set; } = "Aktif"; // Aktif, Bakmda, Pasif
        
        // YEN EKLENEN: KM ve Saat Takibi
        public int CurrentKm { get; set; } = 0; // Gncel Kilometre
        public decimal CurrentWorkingHours { get; set; } = 0; //  Makinalar in Gncel alma Saati
'''
content = content.replace('        public string Status { get; set; } = "Aktif"; // Aktif, Bakmda, Pasif', replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)

path2 = 'GMK360.Core/Entities/Logistics/VehicleTask.cs'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    content2 = f.read()

replacement2 = '''        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Yolda, Tamamland
        
        // YEN EKLENEN: KM ve Saat Takibi
        public int? StartKm { get; set; }
        public int? EndKm { get; set; }
        public decimal? WorkingHours { get; set; } // Bu grev/sefer ka saat srd?
'''
content2 = content2.replace('        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Yolda, Tamamland', replacement2)

with codecs.open(path2, 'w', 'utf-8-sig') as f:
    f.write(content2)

print('Updated entities for KM and Hours tracking.')