import re

filepath = r'GMK360.Web\Controllers\B2BPurchasingController.cs'
with open(filepath, 'r', encoding='latin1') as f:
    content = f.read()

# Update Index action to pass Projects to ViewBag
old_index = """        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var quotes = await _context.B2BQuoteRequests
                .Include(q => q.Items)
                    .ThenInclude(i => i.MaterialCatalog)
                .Include(q => q.Invites)
                .Where(q => q.RequesterAgencyId == agencyId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            ViewBag.Catalogs = await _context.MaterialCatalogs
                .Where(c => c.AgencyId == agencyId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(quotes);
        }"""

new_index = """        public async Task<IActionResult> Index()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var quotes = await _context.B2BQuoteRequests
                .Include(q => q.Items)
                    .ThenInclude(i => i.MaterialCatalog)
                .Include(q => q.Invites)
                .Where(q => q.RequesterAgencyId == agencyId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            ViewBag.Catalogs = await _context.MaterialCatalogs
                .Where(c => c.AgencyId == agencyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            ViewBag.Projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(quotes);
        }"""

content = content.replace(old_index, new_index)

# Update CreateQuote signature
old_sig = "public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int[] materialCatalogIds, decimal[] quantities)"
new_sig = "public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int[] materialCatalogIds, decimal[] quantities, int? projectId)"
content = content.replace(old_sig, new_sig)

# Update CreateQuote body for projectId
old_body = """                var quote = new B2BQuoteRequest
                {
                    RequesterAgencyId = agencyId.Value,
                    RequesterUserId = user?.Id ?? "",
                    SourceModule = "Direct",
                    SourceReferenceId = 0,
                    Title = finalTitle ?? "Ýsimsiz Alým",
                    Description = description ?? "",
                    Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };"""

new_body = """                var quote = new B2BQuoteRequest
                {
                    RequesterAgencyId = agencyId.Value,
                    RequesterUserId = user?.Id ?? "",
                    SourceModule = (projectId.HasValue && projectId.Value > 0) ? "ConstructionProject" : "Direct",
                    SourceReferenceId = projectId ?? 0,
                    Title = finalTitle ?? "Ýsimsiz Alým",
                    Description = description ?? "",
                    Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };"""

content = content.replace(old_body, new_body)

with open(filepath, 'w', encoding='latin1') as f:
    f.write(content)
print("Controller updated for Project selection.")
