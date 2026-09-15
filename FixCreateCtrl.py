import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will replace the Create(int? id) GET and Create POST methods
get_create = '''        public async Task<IActionResult> Create()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var model = new GMK360.Core.Entities.Construction.ConstructionProject
            {
                StartDate = System.DateTime.UtcNow,
                Status = GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye
            };
            
            return View(model);
        }'''

post_create = '''        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSimple([Bind("Name,Address,StartDate,Status,TotalFloors,TargetTotalApartments,TargetTotalShops")] GMK360.Core.Entities.Construction.ConstructionProject project)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(project.Name)) ModelState.AddModelError("Name", "Proje adı zorunludur.");
            if (string.IsNullOrWhiteSpace(project.Address)) ModelState.AddModelError("Address", "Adres zorunludur.");

            if (ModelState.IsValid)
            {
                project.AgencyId = agencyId.Value;
                project.CreatedAt = System.DateTime.UtcNow;
                project.Description = project.Name + " projesi genel bilgileri"; // zorunlu olmasın diye dummy atanır
                
                _context.Add(project);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"{project.Name} isimli şantiye başarıyla açıldı.";
                return RedirectToAction(nameof(Index));
            }
            return View("Create", project);
        }'''

# Since we don't want to mess up the whole controller if regex fails, we can just search and replace precisely.
# The original GET Create:
# public async Task<IActionResult> Create(int? id = null)
# ... up to the next method

pattern = re.compile(r'public async Task<IActionResult> Create\(int\? id = null\).*?(?=public async Task<IActionResult> CreateWizard)', re.DOTALL)
if pattern.search(content):
    content = pattern.sub(get_create + '\r\n\r\n', content, count=1)
    print("Replaced GET Create")
else:
    print("Could not find GET Create")

pattern2 = re.compile(r'public async Task<IActionResult> Create\(\[Bind\("Name,Description,Address,StartDate,EndDate,CoverImageUrl"\)\] ConstructionProject project\).*?(?=// POST: ConstructionProject/AddConstructionTask)', re.DOTALL)
if pattern2.search(content):
    content = pattern2.sub(post_create + '\r\n\r\n        ', content, count=1)
    print("Replaced POST Create")
else:
    print("Could not find POST Create")


with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
