using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class ComplexApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComplexApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- YENİ: Sokağa Göre Bina Getir ---
        [HttpGet("GetByStreet/{streetId}")]
        public async Task<IActionResult> GetByStreet(int streetId, string q = null)
        {
            var query = _context.Buildings
                .Where(c => c.StreetId == streetId);

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(c => c.Name.Contains(q) || c.BuildingNumber.Contains(q));
            }

            var buildings = await query
                .Select(c => new { 
                    id = c.Id, 
                    text = string.IsNullOrEmpty(c.BuildingNumber) ? c.Name : $"{c.Name} - No: {c.BuildingNumber}"
                })
                .ToListAsync();

            return Ok(new { results = buildings });
        }

        // ESKİ: Geriye dönük uyumluluk için
        [HttpGet("GetByNeighborhood/{neighborhoodId}")]
        public async Task<IActionResult> GetByNeighborhood(int neighborhoodId, string q = null)
        {
            var query = _context.Buildings
                .Where(c => c.NeighborhoodId == neighborhoodId);

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(c => c.Name.Contains(q));
            }

            var buildings = await query
                .Select(c => new { 
                    id = c.Id, 
                    text = c.Street != null ? $"{c.Name} ({c.Street})" : c.Name 
                })
                .ToListAsync();

            return Ok(new { results = buildings });
        }

        // --- YENİ: Binaya Ait Daireleri Getir ---
        [HttpGet("GetUnits/{buildingId}")]
        public async Task<IActionResult> GetUnits(int buildingId)
        {
            var units = await _context.BuildingUnits
                .Where(u => u.BuildingId == buildingId)
                .OrderBy(u => u.Id)
                .Select(u => new { 
                    id = u.Id, 
                    doorNumber = u.DoorNumber,
                    isEmpty = u.IsEmpty,
                    isRented = !string.IsNullOrEmpty(u.TenantName) || u.TenantUserId != null
                })
                .ToListAsync();

            return Ok(units);
        }

        // POST: api/ComplexApi/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ComplexCreateDto dto)
        {
            if (dto == null) return BadRequest("Gönderilen veri formatı geçersiz.");

            if (string.IsNullOrEmpty(dto.Name) || !dto.CityId.HasValue || !dto.DistrictId.HasValue || !dto.NeighborhoodId.HasValue)
            {
                return BadRequest("Eksik bilgi gönderildi. Lütfen İl, İlçe, Mahalle ve Bina Adı alanlarını doldurun.");
            }
            if (!dto.StreetId.HasValue)
            {
                return BadRequest("Lütfen bir Sokak seçiniz.");
            }
            if (string.IsNullOrEmpty(dto.BuildingNumber))
            {
                return BadRequest("Lütfen Dış Kapı Numarasını giriniz.");
            }

            // Sokak adını al
            var street = await _context.Streets.FindAsync(dto.StreetId.Value);
            string streetName = street?.Name ?? "";

            // Toplam Kat sayısını bul (FloorDetails'den)
            int totalFloors = dto.FloorDetails != null ? dto.FloorDetails.Count : (dto.TotalFloors ?? 1);
            int totalUnits = 0;

            var building = new Building
            {
                Name = dto.Name,
                CityId = dto.CityId.Value,
                DistrictId = dto.DistrictId.Value,
                NeighborhoodId = dto.NeighborhoodId.Value,
                StreetId = dto.StreetId.Value,
                StreetName = streetName,
                BuildingNumber = dto.BuildingNumber,
                TotalFloors = totalFloors,
                HasBlock = dto.HasBlocks,
                BlockName = dto.Blocks != null && dto.Blocks.Any() ? dto.Blocks.First().Name : null,
                Latitude = dto.Latitude ?? 0,
                Longitude = dto.Longitude ?? 0,
                CreatedAt = System.DateTime.Now,
                OnboardingStep = 2 // Bina yaratıldı, daireler ekleniyor
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync(); // Building ID almak için

            // MATRIX: Daireleri Katlara Göre Üret
            if (dto.FloorDetails != null && dto.FloorDetails.Any())
            {
                foreach (var floor in dto.FloorDetails)
                {
                    string floorName = floor.FloorIndex == 0 ? "Zemin Kat" : $"{floor.FloorIndex}. Kat";
                    
                    // Dükkanları üret
                    for (int i = 1; i <= floor.ShopCount; i++)
                    {
                        var unit = new BuildingUnit
                        {
                            BuildingId = building.Id,
                            DoorNumber = $"{floorName} - Dükkan {i}",
                            IsEmpty = true
                        };
                        _context.BuildingUnits.Add(unit);
                        totalUnits++;
                    }

                    // Daireleri üret
                    for (int i = 1; i <= floor.FlatCount; i++)
                    {
                        var unit = new BuildingUnit
                        {
                            BuildingId = building.Id,
                            DoorNumber = $"{floorName} - Daire {i}",
                            IsEmpty = true
                        };
                        _context.BuildingUnits.Add(unit);
                        totalUnits++;
                    }
                }
            }
            else if (dto.TotalUnits.HasValue && dto.TotalUnits.Value > 0)
            {
                // Fallback: Matrix yoksa düz üret
                for (int i = 1; i <= dto.TotalUnits.Value; i++)
                {
                    var unit = new BuildingUnit
                    {
                        BuildingId = building.Id,
                        DoorNumber = $"Daire {i}",
                        IsEmpty = true
                    };
                    _context.BuildingUnits.Add(unit);
                    totalUnits++;
                }
            }

            building.TotalUnits = totalUnits;
            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                id = building.Id, 
                message = "Bina başarıyla mühürlendi ve dijital ikizleri oluşturuldu." 
            });
        }

        public async Task<IActionResult> GetComplexDetails(int complexId)
        {
            var complex = await _context.Buildings
                .Include(c => c.Blocks)
                .FirstOrDefaultAsync(c => c.Id == complexId);

            if (complex == null)
            {
                return NotFound();
            }

            var result = new
            {
                id = complex.Id,
                name = complex.Name,
                hasBlocks = complex.HasBlocks,
                totalFloors = complex.TotalFloors,
                buildingNumber = complex.BuildingNumber,
                blocks = complex.Blocks.Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    totalFloors = b.TotalFloors
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost("AddBlockToComplex")]
        public async Task<IActionResult> AddBlockToComplex([FromBody] AddBlockDto dto)
        {
            var complex = await _context.Buildings.FindAsync(dto.ComplexId);
            if (complex == null) return NotFound("Bina/Site bulunamadı.");

            if (!complex.HasBlocks)
            {
                complex.HasBlocks = true;
            }

            var newBlock = new ComplexBlock
            {
                ComplexId = dto.ComplexId,
                Name = dto.Name,
                TotalFloors = dto.TotalFloors
            };

            _context.ComplexBlocks.Add(newBlock);
            await _context.SaveChangesAsync();

            return Ok(new { id = newBlock.Id, name = newBlock.Name, totalFloors = newBlock.TotalFloors });
        }
    
        // --- YENİ: Sokağımı Bulamadım (On-the-fly Street Creation + Ticket) ---
        [HttpPost("CreateUnapprovedStreet")]
        public async Task<IActionResult> CreateUnapprovedStreet([FromBody] CreateStreetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StreetName) || request.NeighborhoodId <= 0)
            {
                return BadRequest("Sokak adı ve Mahalle ID zorunludur.");
            }

            var street = new Street
            {
                Name = request.StreetName.Trim(),
                NeighborhoodId = request.NeighborhoodId
            };
            _context.Streets.Add(street);
            await _context.SaveChangesAsync();

            // Acil Durum Çağrısı (Ticket) Oluştur
            var ticket = new SystemIssueTicket
            {
                IssueType = "MissingStreet",
                Description = $"Kullanıcı tarafından yeni bir sokak eklendi: {street.Name}",
                CityId = request.CityId,
                DistrictId = request.DistrictId,
                NeighborhoodId = request.NeighborhoodId,
                ContextData = $"StreetId: {street.Id}",
                IsResolved = false
            };
            _context.SystemIssueTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { id = street.Id, name = street.Name });
        }

        // --- YENİ: Bina Bilgisi Hatalı (Typo Reporting + Ticket) ---
        [HttpPost("ReportBuildingTypo")]
        public async Task<IActionResult> ReportBuildingTypo([FromBody] ReportBuildingRequest request)
        {
            if (request.BuildingId <= 0 || string.IsNullOrWhiteSpace(request.CorrectName))
            {
                return BadRequest("Bina ID ve Doğru İsim zorunludur.");
            }

            var ticket = new SystemIssueTicket
            {
                IssueType = "BuildingTypo",
                Description = $"Kullanıcı bina adında hata bildirdi. Önerilen isim: {request.CorrectName}",
                ContextData = $"BuildingId: {request.BuildingId}",
                IsResolved = false
            };
            _context.SystemIssueTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

}

    public class AddBlockDto
    {
        public int ComplexId { get; set; }
        public string Name { get; set; }
        public int TotalFloors { get; set; }
    }

    public class ComplexCreateDto
    {
        public string? Name { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasPool { get; set; }
        public int? TotalUnits { get; set; }
        public int? TotalFloors { get; set; }
        public bool IsSite { get; set; } = true;
        public string? BuildingNumber { get; set; }
        public string? NewStreetName { get; set; }
        public bool HasBlocks { get; set; }
        public List<ComplexBlockDto>? Blocks { get; set; }
        
        // MATRIS MODELI
        public List<FloorDetailDto>? FloorDetails { get; set; }
    }

    public class ComplexBlockDto
    {
        public string? Name { get; set; }
        public int TotalFloors { get; set; }
    }
    
    public class FloorDetailDto
    {
        public int FloorIndex { get; set; } // 0 = Zemin, 1 = 1.Kat vs.
        public int FlatCount { get; set; } // Daire Sayısı
        public int ShopCount { get; set; } // Dükkan Sayısı
    }
}

    public class CreateStreetRequest
    {
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int NeighborhoodId { get; set; }
        public string StreetName { get; set; }
    }

    public class ReportBuildingRequest
    {
        public int BuildingId { get; set; }
        public string CorrectName { get; set; }
    }
