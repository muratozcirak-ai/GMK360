import codecs

path = 'GMK360.Web/Controllers/InventoryController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

import re

# We need to completely rewrite the Index method to fix this fatal logic error.
match = re.search(r'public async Task<IActionResult> Index\(int\? id\).*?return View\(warehouses\);\s*}', content, flags=re.DOTALL)
if match:
    new_index = '''public async Task<IActionResult> Index(int? id)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

            // 1. Önce eksik depoları tespit edip oluşturalım (Tüm DB'ye bakarak)
            if (agencyId.HasValue)
            {
                var allAgencyWarehouses = await _context.Warehouses
                    .Where(w => w.AgencyId == agencyId.Value)
                    .ToListAsync();

                // Merkez Depo kontrolü
                if (!allAgencyWarehouses.Any(w => w.Type == WarehouseType.Merkez && !w.IsDeleted))
                {
                    var merkezDepo = new Warehouse
                    {
                        AgencyId = agencyId.Value,
                        Name = "Merkez Depo",
                        Type = WarehouseType.Merkez,
                        Address = "",
                        Description = "Merkez Depo (Ana Merkez)",
                        CreatedAt = System.DateTime.UtcNow
                    };
                    _context.Warehouses.Add(merkezDepo);
                    await _context.SaveChangesAsync();
                }

                // Tüm aktif ve planlanan projeler için şantiye deposu kontrolü (Sadece bitmiş veya iptal olanları es geç)
                var projects = await _context.ConstructionProjects
                    .Where(p => p.AgencyId == agencyId.Value && !p.IsDeleted && 
                           p.Status != GMK360.Core.Entities.Construction.ProjectStatus.Teslim_Edildi && 
                           p.Status != GMK360.Core.Entities.Construction.ProjectStatus.Durduruldu)
                    .ToListAsync();

                foreach (var proj in projects)
                {
                    if (!allAgencyWarehouses.Any(w => w.ConstructionProjectId == proj.Id && !w.IsDeleted))
                    {
                        var santiyeDepo = new Warehouse
                        {
                            AgencyId = agencyId.Value,
                            ConstructionProjectId = proj.Id,
                            Name = proj.Name + " Şantiyesi Deposu",
                            Type = WarehouseType.Santiye,
                            Address = proj.Address ?? "",
                            Description = proj.Name + " Şantiye sahası genel deposu.",
                            CreatedAt = System.DateTime.UtcNow
                        };
                        _context.Warehouses.Add(santiyeDepo);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // 2. Şimdi listelemek üzere filtreli sorguyu oluşturalım
            var query = _context.Warehouses.Include(w => w.Items).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(w => w.AgencyId == agencyId);
            }

            if (id.HasValue)
            {
                query = query.Where(w => w.ConstructionProjectId == id.Value);
                var project = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == id.Value);
                if (project != null)
                {
                    ViewData["ProjectId"] = project.Id;
                    ViewData["ProjectName"] = project.Name;
                }
            }

            var finalWarehouses = await query.ToListAsync();
            return View(finalWarehouses);
        }'''
        
    content = content.replace(match.group(0), new_index)
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Fixed InventoryController Index!")
else:
    print("Not found")