import io

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8', errors='replace') as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if 'var sahaGorseli = await _context.DocumentArchives' in line:
        lines[i+1] = '                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut")))\n'
    if 'var projeGorseli = await _context.DocumentArchives' in line:
        lines[i+1] = '                .Where(d => d.SourceModule == "Construction" && d.ProjectId == id && (d.Category.Contains("Proje") || (d.Category.Contains("Bina") && !d.Title.Contains("Mevcut"))))\n'

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(lines)

print("Queries updated successfully.")
