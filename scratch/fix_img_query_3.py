import io

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8', errors='replace') as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if 'var currentStateDoc = _context.DocumentArchives.FirstOrDefault' in line:
        lines[i] = '                    var currentStateDoc = _context.DocumentArchives.FirstOrDefault(d => d.SourceModule == "Construction" && d.ProjectId == draft.Id && (d.Category.Contains("Saha") || d.Title.Contains("Mevcut")));\n'

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(lines)

print("currentStateDoc query updated.")
