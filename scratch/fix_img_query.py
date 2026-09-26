import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

pattern1 = r'var sahaGorseli = await _context\.DocumentArchives\s*\.Where\(d => d\.SourceModule == "Construction" && d\.ProjectId == id && d\.Category == "Saha G.rseli"\)'
replacement1 = r'var sahaGorseli = await _context.DocumentArchives.Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut")))'
content = re.sub(pattern1, replacement1, content)

pattern2 = r'var projeGorseli = await _context\.DocumentArchives\s*\.Where\(d => d\.SourceModule == "Construction" && d\.ProjectId == id && \(d\.Category == "Bina G.rselleri"\s*\|\|\s*d\.Category == "Proje G.rseli"\)\)'
replacement2 = r'var projeGorseli = await _context.DocumentArchives.Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Proje") || (d.Category.Contains("Bina") && !d.Title.Contains("Mevcut"))))'
content = re.sub(pattern2, replacement2, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Queries updated.")
