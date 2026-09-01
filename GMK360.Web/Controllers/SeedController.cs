using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    // Yalnızca geçici veri aktarımı içindir.
    public class SeedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> SeedDynamicForms()
        {
            var categories = new[]
            {
                new { Name = "Oda Sayısı", Code = "DROPDOWN_ROOM_COUNT", Values = new[] { "1+0 (Stüdyo)", "1+1", "2+1", "3+1", "4+1", "5+1", "5+2" } },
                new { Name = "Banyo Sayısı", Code = "DROPDOWN_BATHROOM_COUNT", Values = new[] { "1", "2", "3", "4" } },
                new { Name = "Daire Tipi", Code = "DROPDOWN_APARTMENT_TYPE", Values = new[] { "Standart", "Dubleks", "Dubleks (Ters)", "Dubleks (Çatı)", "Tripleks", "Dörtlex", "Suit" } },
                new { Name = "Bina Yaşı", Code = "DROPDOWN_BUILDING_AGE", Values = new[] { "0 (Yeni)", "1-5", "6-10", "11-20", "21 ve üzeri" } }
            };

            foreach (var c in categories)
            {
                var cat = await _context.DefinitionCategories.FirstOrDefaultAsync(x => x.SystemCode == c.Code);
                if (cat == null)
                {
                    cat = new DefinitionCategory { Name = c.Name, SystemCode = c.Code };
                    _context.DefinitionCategories.Add(cat);
                    await _context.SaveChangesAsync();

                    int order = 1;
                    foreach (var val in c.Values)
                    {
                        _context.DefinitionValues.Add(new DefinitionValue { CategoryId = cat.Id, Name = val, SystemCode = "VAL_" + order, Order = order++ });
                    }
                }
            }
            await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();
            return Content("Seed Ok");
        }

        public async Task<IActionResult> SeedEcosystemCatalogs()
        {
            var categories = new[]
            {
                new { Name = "Eşya / Demirbaş Listesi (Konut)", Code = "DROPDOWN_FURNITURE_LIST", Values = new[] { "Koltuk Takımı", "TV & Ünite", "Buzdolabı", "Çamaşır Makinesi", "Bulaşık Makinesi", "Çift Kişilik Yatak", "Gardırop", "Yemek Masası" } },
                new { Name = "B2B Malzeme (Tedarikçi) Türleri", Code = "DROPDOWN_B2B_MATERIALS", Values = new[] { "Çimento & Harç", "İnşaat Demiri", "Boya & Yalıtım", "Seramik & Fayans", "Hırdavat & El Aletleri" } },
                new { Name = "İlan Şikayet Sebepleri", Code = "DROPDOWN_COMPLAINT_REASONS", Values = new[] { "Yanıltıcı Fiyat", "Yanlış Konum/Adres", "Fotoğraflar Gerçek Değil", "İlan Sahibi Ulaşılamıyor", "Kapora Dolandırıcılığı Şüphesi" } },
                new { Name = "Sözleşme Fesih Sebepleri", Code = "DROPDOWN_CONTRACT_TERMINATION", Values = new[] { "Ödeme Gecikmesi", "Mülkün Satılması", "Hasar/Kötü Kullanım", "Karşılıklı Anlaşma" } }
            };

            foreach (var c in categories)
            {
                var cat = await _context.DefinitionCategories.FirstOrDefaultAsync(x => x.SystemCode == c.Code);
                if (cat == null)
                {
                    cat = new DefinitionCategory { Name = c.Name, SystemCode = c.Code };
                    _context.DefinitionCategories.Add(cat);
                    await _context.SaveChangesAsync();

                    int order = 1;
                    foreach (var val in c.Values)
                    {
                        _context.DefinitionValues.Add(new DefinitionValue { CategoryId = cat.Id, Name = val, SystemCode = c.Code + "_VAL_" + order, Order = order++ });
                    }
                }
            }
            await _context.SaveChangesAsync();

            // LiabilityType Seed
            if (!await _context.LiabilityTypes.AnyAsync())
            {
                var liabilities = new List<LiabilityType>
                {
                    new LiabilityType { Name = "Emlak Vergisi", Category = "Vergi", IsSystemType = true },
                    new LiabilityType { Name = "Çevre ve Temizlik Vergisi", Category = "Vergi", IsSystemType = true },
                    new LiabilityType { Name = "Tabela/Reklam Vergisi", Category = "Vergi", IsSystemType = true },
                    new LiabilityType { Name = "DASK", Category = "Sigorta", IsSystemType = true },
                    new LiabilityType { Name = "Konut/İşyeri Sigortası", Category = "Sigorta", IsSystemType = true },
                    new LiabilityType { Name = "Bina Aidatı", Category = "Sabit Gider", IsSystemType = true },
                    new LiabilityType { Name = "Tadilat / Bakım", Category = "Özel Gider", IsSystemType = false }
                };
                _context.LiabilityTypes.AddRange(liabilities);
                await _context.SaveChangesAsync();
            }

            return Content("Ecosystem & Financial Catalogs Seeded Successfully!");
        }

        public async Task<IActionResult> ImportSqlData()
        {
            string sqlFilePath = @"C:\Users\murat\source\repos\GMK360\TempAddressDb\tr-address-db-main\data.sql";
            if (!System.IO.File.Exists(sqlFilePath))
                return Content("SQL dosyası bulunamadı.");

            // Clear old data to prevent conflicts (except Country)
            _context.Neighborhoods.RemoveRange(_context.Neighborhoods);
            _context.Districts.RemoveRange(_context.Districts);
            _context.Cities.RemoveRange(_context.Cities);
            await _context.SaveChangesAsync();

            string[] lines = await System.IO.File.ReadAllLinesAsync(sqlFilePath);
            
            var cities = new List<City>();
            var districts = new List<District>();
            var neighborhoods = new List<Neighborhood>();
            var semtToIlce = new Dictionary<int, int>(); // semt_id -> ilce_id

            string currentTable = "";
            
            // Regex patterns based on standard mysqldump
            // (1, 'Adana')
            var cityRegex = new Regex(@"^\((\d+),\s*'([^']+)'\)");
            // (1, 1, 'Aladağ')
            var districtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Karatas Semti')
            var semtRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']+)'\)");
            // (1, 1, 'Akpınar Mah', '01720')
            var neighborhoodRegex = new Regex(@"^\((\d+),\s*(\d+),\s*'([^']*)',\s*'([^']*)'\)");

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("INSERT INTO `volt_iller`"))
                {
                    currentTable = "iller";
                    continue;
                }
                else if (trimmed.StartsWith("INSERT INTO `volt_ilceler`"))
                {
                    currentTable = "ilceler";
                    continue;
                }
                else if (trimmed.StartsWith("INSERT INTO `volt_semtler`"))
                {
                    currentTable = "semtler";
                    continue;
                }
                else if (trimmed.StartsWith("INSERT INTO `volt_mahalleler`"))
                {
                    currentTable = "mahalleler";
                    continue;
                }

                if (string.IsNullOrEmpty(currentTable) || !trimmed.StartsWith("("))
                    continue;

                // Remove trailing commas or semicolons for regex
                var cleanLine = trimmed;
                if (cleanLine.EndsWith(",") || cleanLine.EndsWith(";"))
                {
                    cleanLine = cleanLine.Substring(0, cleanLine.Length - 1);
                }

                if (currentTable == "iller")
                {
                    var match = cityRegex.Match(cleanLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        string name = match.Groups[2].Value;
                        // Add to memory
                        cities.Add(new City { Id = id, CountryId = 1, Name = name, PlateCode = id.ToString().PadLeft(2, '0') });
                    }
                }
                else if (currentTable == "ilceler")
                {
                    var match = districtRegex.Match(cleanLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        int ilId = int.Parse(match.Groups[2].Value);
                        string name = match.Groups[3].Value;
                        districts.Add(new District { Id = id, CityId = ilId, Name = name });
                    }
                }
                else if (currentTable == "semtler")
                {
                    var match = semtRegex.Match(cleanLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        int ilceId = int.Parse(match.Groups[2].Value);
                        semtToIlce[id] = ilceId;
                    }
                }
                else if (currentTable == "mahalleler")
                {
                    var match = neighborhoodRegex.Match(cleanLine);
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[1].Value);
                        int semtId = int.Parse(match.Groups[2].Value);
                        string name = match.Groups[3].Value;
                        string zipCode = match.Groups[4].Value;

                        if (semtToIlce.TryGetValue(semtId, out int ilceId))
                        {
                            neighborhoods.Add(new Neighborhood { Id = id, DistrictId = ilceId, Name = name, ZipCode = zipCode });
                        }
                    }
                }
            }

            // In EF Core, inserting with explicit identity can be tricky. We use raw SQL or set Identity Insert.
            // A simpler way: map to our domain without enforcing identical IDs, or let EF handle it.
            // To preserve IDs for foreign keys, we must enable IDENTITY_INSERT.
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Cities ON");
                _context.Cities.AddRange(cities);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Cities OFF");

                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Districts ON");
                _context.Districts.AddRange(districts);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Districts OFF");

                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Neighborhoods ON");
                
                // Save neighborhoods in chunks to avoid large memory spikes
                int chunkSize = 5000;
                for (int i = 0; i < neighborhoods.Count; i += chunkSize)
                {
                    var chunk = neighborhoods.Skip(i).Take(chunkSize);
                    _context.Neighborhoods.AddRange(chunk);
                    await _context.SaveChangesAsync();
                }
                
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Neighborhoods OFF");
                await transaction.CommitAsync();

                return Content($"Başarılı: {cities.Count} Şehir, {districts.Count} İlçe, {neighborhoods.Count} Mahalle eklendi.");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Content("Hata oluştu: " + ex.Message + " | Inner: " + ex.InnerException?.Message);
            }
        }

        [HttpGet("Seed/Definitions")]
        public async Task<IActionResult> SeedDefinitions()
        {
            if (await _context.DefinitionCategories.AnyAsync())
            {
                return Content("Tanımlamalar zaten eklenmiş.");
            }

            var categories = new List<DefinitionCategory>
            {
                new DefinitionCategory { Name = "İlan Durumu", SystemCode = "PropertyStatus" },
                new DefinitionCategory { Name = "Konut Tipi", SystemCode = "PropertyType" },
                new DefinitionCategory { Name = "İşyeri Tipi", SystemCode = "CommercialType" },
                new DefinitionCategory { Name = "Arsa Tipi", SystemCode = "LandType" },
                new DefinitionCategory { Name = "Isıtma Tipi", SystemCode = "HeatingType" },
                new DefinitionCategory { Name = "Bina Yaşı", SystemCode = "BuildingAge" },
                new DefinitionCategory { Name = "Cephe", SystemCode = "Facade" }
            };

            _context.DefinitionCategories.AddRange(categories);
            await _context.SaveChangesAsync();

            var statusCat = categories.First(c => c.SystemCode == "PropertyStatus");
            var resTypeCat = categories.First(c => c.SystemCode == "PropertyType");
            var comTypeCat = categories.First(c => c.SystemCode == "CommercialType");
            var landTypeCat = categories.First(c => c.SystemCode == "LandType");
            var heatCat = categories.First(c => c.SystemCode == "HeatingType");
            var ageCat = categories.First(c => c.SystemCode == "BuildingAge");
            var facadeCat = categories.First(c => c.SystemCode == "Facade");

            var definitions = new List<DefinitionValue>
            {
                // Status
                new DefinitionValue { CategoryId = statusCat.Id, Name = "Satılık", SystemCode = "Sale" },
                new DefinitionValue { CategoryId = statusCat.Id, Name = "Kiralık", SystemCode = "Rent" },
                new DefinitionValue { CategoryId = statusCat.Id, Name = "Günlük Kiralık", SystemCode = "DailyRent" },
                
                // Property Type
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Daire", SystemCode = "Apartment" },
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Müstakil Ev", SystemCode = "DetachedHouse" },
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Villa", SystemCode = "Villa" },
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Yalı", SystemCode = "Waterside" },
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Rezidans", SystemCode = "Residence" },
                new DefinitionValue { CategoryId = resTypeCat.Id, Name = "Prefabrik", SystemCode = "Prefabricated" },

                // Commercial Type
                new DefinitionValue { CategoryId = comTypeCat.Id, Name = "Dükkan", SystemCode = "Shop" },
                new DefinitionValue { CategoryId = comTypeCat.Id, Name = "Ofis", SystemCode = "Office" },
                new DefinitionValue { CategoryId = comTypeCat.Id, Name = "Plaza Katı", SystemCode = "Plaza" },
                new DefinitionValue { CategoryId = comTypeCat.Id, Name = "Depo", SystemCode = "Warehouse" },
                new DefinitionValue { CategoryId = comTypeCat.Id, Name = "Fabrika", SystemCode = "Factory" },

                // Land Type
                new DefinitionValue { CategoryId = landTypeCat.Id, Name = "İmarlı", SystemCode = "Zoned" },
                new DefinitionValue { CategoryId = landTypeCat.Id, Name = "Tarla", SystemCode = "Field" },
                new DefinitionValue { CategoryId = landTypeCat.Id, Name = "Zeytinlik", SystemCode = "OliveGrove" },
                new DefinitionValue { CategoryId = landTypeCat.Id, Name = "Bağ & Bahçe", SystemCode = "Orchard" },

                // Heating
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Yok", SystemCode = "None" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Soba", SystemCode = "Stove" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Doğalgaz (Kombi)", SystemCode = "Combi" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Merkezi", SystemCode = "Central" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Merkezi (Pay Ölçer)", SystemCode = "CentralShareMeter" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Yerden Isıtma", SystemCode = "Underfloor" },
                new DefinitionValue { CategoryId = heatCat.Id, Name = "Klima", SystemCode = "AC" },

                // Age
                new DefinitionValue { CategoryId = ageCat.Id, Name = "0 (Sıfır)", SystemCode = "Age0" },
                new DefinitionValue { CategoryId = ageCat.Id, Name = "1-5 Arası", SystemCode = "Age1_5" },
                new DefinitionValue { CategoryId = ageCat.Id, Name = "6-10 Arası", SystemCode = "Age6_10" },
                new DefinitionValue { CategoryId = ageCat.Id, Name = "11-15 Arası", SystemCode = "Age11_15" },
                new DefinitionValue { CategoryId = ageCat.Id, Name = "16-20 Arası", SystemCode = "Age16_20" },
                new DefinitionValue { CategoryId = ageCat.Id, Name = "21 ve Üzeri", SystemCode = "Age21Plus" },

                // Facade
                new DefinitionValue { CategoryId = facadeCat.Id, Name = "Kuzey", SystemCode = "North" },
                new DefinitionValue { CategoryId = facadeCat.Id, Name = "Güney", SystemCode = "South" },
                new DefinitionValue { CategoryId = facadeCat.Id, Name = "Doğu", SystemCode = "East" },
                new DefinitionValue { CategoryId = facadeCat.Id, Name = "Batı", SystemCode = "West" }
            };

            _context.DefinitionValues.AddRange(definitions);
            await _context.SaveChangesAsync();

            return Content("Emlak tanımlamaları başarıyla veritabanına eklendi!");
        }

        [HttpGet("Seed/LocationIntelligence")]
        public async Task<IActionResult> SeedLocationIntelligence()
        {
            if (await _context.NeighborhoodPOIs.AnyAsync() || await _context.LocalProfessionals.AnyAsync())
            {
                return Content("Lokasyon Zekası verileri (Okul, Usta, Komşu) zaten eklenmiş.");
            }

            var acibademId = 1;
            var bostanciId = 2;
            var gulSokakId = 1;

            var street = await _context.Streets.Include(s => s.Neighborhood).FirstOrDefaultAsync();
            if (street != null)
            {
                gulSokakId = street.Id;
                acibademId = street.NeighborhoodId;
                var neighbor = await _context.Neighborhoods.Where(n => n.Id != acibademId && n.DistrictId == street.Neighborhood.DistrictId).FirstOrDefaultAsync();
                bostanciId = neighbor != null ? neighbor.Id : acibademId + 1; // Fallback
            }
            else
            {
                // Veritabanı tamamen boşsa geçici bir tane ekleyelim
                var dist = await _context.Districts.FirstOrDefaultAsync();
                var nb1 = new Neighborhood { Name = "Seed Mahalle 1", DistrictId = dist?.Id ?? 1 };
                var nb2 = new Neighborhood { Name = "Seed Mahalle 2", DistrictId = dist?.Id ?? 1 };
                _context.Neighborhoods.Add(nb1);
                _context.Neighborhoods.Add(nb2);
                await _context.SaveChangesAsync();

                var newStreet = new Street { Name = "Seed Sokak", NeighborhoodId = nb1.Id };
                _context.Streets.Add(newStreet);
                await _context.SaveChangesAsync();

                acibademId = nb1.Id;
                bostanciId = nb2.Id;
                gulSokakId = newStreet.Id;
            }

            var neighborhoodPois = new List<NeighborhoodPOI>
            {
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Acıbadem Lisesi", PoiCategory = GMK360.Core.Enums.POICategory.Education, DistanceInMeters = 300 },
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Acıbadem Hastanesi", PoiCategory = GMK360.Core.Enums.POICategory.Health, DistanceInMeters = 850 },
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Kadıköy Metro İstasyonu", PoiCategory = GMK360.Core.Enums.POICategory.Transport, DistanceInMeters = 1200 },
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Acıbadem Polis Merkezi", PoiCategory = GMK360.Core.Enums.POICategory.Security, DistanceInMeters = 500 },
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Acıbadem Parkı", PoiCategory = GMK360.Core.Enums.POICategory.SocialAndPark, DistanceInMeters = 200 },
                new NeighborhoodPOI { NeighborhoodId = acibademId, PoiName = "Tepe Nautilus AVM", PoiCategory = GMK360.Core.Enums.POICategory.Shopping, DistanceInMeters = 1500 }
            };

            var professionals = new List<LocalProfessional>
            {
                new LocalProfessional { Name = "Çelik Kardeşler Nakliyat", ProfessionType = "Nakliyat", NeighborhoodId = acibademId, IsPremium = true, PhoneNumber = "0532 111 22 33" },
                new LocalProfessional { Name = "Ahmet Usta Boya & Badana", ProfessionType = "Boyacı", NeighborhoodId = acibademId, IsPremium = false, PhoneNumber = "0555 444 55 66" },
                new LocalProfessional { Name = "Acıbadem Yapı Market", ProfessionType = "Nalbur", NeighborhoodId = acibademId, IsPremium = true, PhoneNumber = "0216 333 44 55" },
                new LocalProfessional { Name = "Güven Tesisat", ProfessionType = "Tesisatçı", NeighborhoodId = acibademId, IsPremium = false, PhoneNumber = "0505 666 77 88" }
            };

            var neighbors = new List<NeighboringArea>
            {
                new NeighboringArea { BaseNeighborhoodId = acibademId, NeighborNeighborhoodId = bostanciId, DistanceInKilometers = 4.5 }
            };

            _context.NeighborhoodPOIs.AddRange(neighborhoodPois);
            _context.LocalProfessionals.AddRange(professionals);
            _context.NeighboringAreas.AddRange(neighbors);

            await _context.SaveChangesAsync();

            return Content("Lokasyon Zekası (Okullar, Esnaflar, Komşu Mahalleler) başarıyla eklendi!");
        }
    }
}
