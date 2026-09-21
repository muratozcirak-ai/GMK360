import re

with open(r"GMK360.Web\Controllers\DocumentArchiveController.cs", "r", encoding="utf-8") as f:
    content = f.read()

# Replace GetCurrentAgencyId
find_agency = r'private int GetCurrentAgencyId\(\)\s*\{\s*var claim = User\.FindFirst\("AgencyId"\);\s*return claim != null \? int\.Parse\(claim\.Value\) : 0;\s*\}'
replace_agency = """private async Task<int> GetCurrentAgencyIdAsync()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return 0;
            
            return await _context.AgencyConsultants
                .Where(a => a.UserId == currentUserId)
                .Select(a => a.AgencyId)
                .FirstOrDefaultAsync();
        }"""
content = re.sub(find_agency, replace_agency, content)

# Replace Index method signature and agency call
find_index = r'public async Task<IActionResult> Index\(string context = "construction", int\? projectId = null, string category = null, int\? year = null\)\s*\{\s*var agencyId = GetCurrentAgencyId\(\);'
replace_index = """public async Task<IActionResult> Index(string context = "construction", int? projectId = null, string category = null, int? year = null)
        {
            var agencyId = await GetCurrentAgencyIdAsync();"""
content = re.sub(find_index, replace_index, content)

with open(r"GMK360.Web\Controllers\DocumentArchiveController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed AgencyId bug in DocumentArchiveController.")
