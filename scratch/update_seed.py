import re

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# The array of 81 cities
cities = [
    'Adana', 'Adıyaman', 'Afyonkarahisar', 'Ağrı', 'Amasya', 'Ankara', 'Antalya', 'Artvin', 'Aydın', 'Balıkesir',
    'Bilecik', 'Bingöl', 'Bitlis', 'Bolu', 'Burdur', 'Bursa', 'Çanakkale', 'Çankırı', 'Çorum', 'Denizli',
    'Diyarbakır', 'Edirne', 'Elazığ', 'Erzincan', 'Erzurum', 'Eskişehir', 'Gaziantep', 'Giresun', 'Gümüşhane', 'Hakkari',
    'Hatay', 'Isparta', 'Mersin', 'İstanbul', 'İzmir', 'Kars', 'Kastamonu', 'Kayseri', 'Kırklareli', 'Kırşehir',
    'Kocaeli', 'Konya', 'Kütahya', 'Malatya', 'Manisa', 'Kahramanmaraş', 'Mardin', 'Muğla', 'Muş', 'Nevşehir',
    'Niğde', 'Ordu', 'Rize', 'Sakarya', 'Samsun', 'Siirt', 'Sinop', 'Sivas', 'Tekirdağ', 'Tokat',
    'Trabzon', 'Tunceli', 'Şanlıurfa', 'Uşak', 'Van', 'Yozgat', 'Zonguldak', 'Aksaray', 'Bayburt', 'Karaman',
    'Kırıkkale', 'Batman', 'Şırnak', 'Bartın', 'Ardahan', 'Iğdır', 'Yalova', 'Karabük', 'Kilis', 'Osmaniye',
    'Düzce'
]

# Generate C# seed objects for 81 cities
city_seeds = []
for idx, name in enumerate(cities, start=1):
    plate = str(idx).zfill(2)
    city_seeds.append(f'                new City {{ Id = {idx}, CountryId = 1, Name = "{name}", PlateCode = "{plate}", CreatedAt = new System.DateTime(2024, 1, 1) }}')

cities_csharp = ',\n'.join(city_seeds)

new_seed_block = f'''
            // --- 81 İL SEED DATA ---
            builder.Entity<City>().HasData(
{cities_csharp}
            );

            // =========================================================================================
            // UYARI / NOT: 
            // İlçe, Mahalle ve Sokak verileri Türkiye geneli için on binlerce satır tuttuğundan dolayı
            // Entity Framework Seed Data (Migration) içine gömülmemiştir. (Sistemi hantallaştırmamak adına)
            // LÜTFEN İLK KURULUMDAN SONRA BU VERİLERİ HARİCİ BİR SQL SCRİPTİ VEYA EXCEL İLE YÜKLEYİNİZ!
            // =========================================================================================
'''

# Find the old Seed Data block and replace it
# Pattern to match builder.Entity<City>().HasData(...) to the end of Neighborhoods HasData
pattern = r'builder\.Entity<City>\(\)\.HasData\(.*?\);\s*builder\.Entity<District>\(\)\.HasData\(.*?\);\s*builder\.Entity<Neighborhood>\(\)\.HasData\(.*?\);'
content = re.sub(pattern, new_seed_block, content, flags=re.DOTALL)

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("ApplicationDbContext seed data updated to 81 cities with UTF-8.")
