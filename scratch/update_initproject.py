import re

# 1. Update Controller
with open(r'GMK360.Web\Controllers\ConstructionProjectController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace(
    'public async Task<IActionResult> InitProject(string projectName, int statusId, string projectType)',
    'public async Task<IActionResult> InitProject(string projectName, int statusId, string projectType, int projectOriginId)'
)

content = content.replace(
    'ProjectType = projectType,',
    'ProjectType = projectType,\n                ProjectOriginId = projectOriginId,'
)

with open(r'GMK360.Web\Controllers\ConstructionProjectController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)


# 2. Update Index.cshtml
with open(r'GMK360.Web\Views\ConstructionProject\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    html = f.read()

dropdown_html = '''
                        <label for="projectOriginId" class="form-label fw-bold">Projenin Doğası <span class="text-danger">*</span></label>
                        <select id="projectOriginId" name="projectOriginId" class="form-select form-select-lg rounded-3 mb-3" required>
                            <option value="1">Kentsel Dönüşüm (Yıkılacak Eski Yapı Var)</option>
                            <option value="0">Sıfırdan İnşaat (Boş Arazi)</option>
                        </select>
'''

html = html.replace('<label for="projectType"', dropdown_html + '\n                        <label for="projectType"')

with open(r'GMK360.Web\Views\ConstructionProject\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(html)

print("InitProject Modal updated successfully.")
