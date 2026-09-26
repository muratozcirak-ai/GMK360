import re

with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# Find the Details method
pattern = re.compile(r'(public async Task<IActionResult> Details\(int\? id\).*?)(var project = await _context\.ConstructionProjects\s*\.Include)', re.DOTALL)

insertion = '''
            // Fetch images from DocumentArchive dynamically if they exist (in case user uploaded manually)
            var sahaGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && d.Category == "Saha Görseli")
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (sahaGorseli != null) {
                ViewBag.CurrentStateImageUrl = sahaGorseli.DocumentUrl;
            }
            
            var projeGorseli = await _context.DocumentArchives
                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category == "Bina Görselleri" || d.Category == "Proje Görseli"))
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
            if (projeGorseli != null) {
                ViewBag.CoverImageUrl = projeGorseli.DocumentUrl;
            }
            
            '''

match = pattern.search(code)
if match:
    new_code = code[:match.end(1)] + insertion + match.group(2) + code[match.end(2):]
    with open('GMK360.Web/Controllers/ConstructionProjectController.cs', 'w', encoding='utf-8') as f:
        f.write(new_code)
    print("SUCCESS")
else:
    print("NOT FOUND")
