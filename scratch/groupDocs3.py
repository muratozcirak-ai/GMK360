import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

pattern = r'<tbody>\s*@if \(\(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\) == null.*?\{\s*foreach \(var doc in \(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\)\.OrderByDescending\(x => x\.IsCustom\)\.ThenBy\(x => x\.Id\)\)\s*\{\s*<tr>'

replace_block = '''<tbody>
                              @if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                              {
                                  <tr>
                                      <td colspan="7" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</td>
                                  </tr>
                              }
                              else
                              {
                                  var docs = (List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs;
                                  var groupedDocs = docs.GroupBy(d => d.Stage ?? "Diğer Evraklar ve Kriterler").ToList();
                                  
                                  foreach (var group in groupedDocs)
                                  {
                                      <tr class="table-light">
                                          <td colspan="7" class="fw-bold text-dark py-3 ps-3">
                                              <i class="bi bi-folder2-open text-primary me-2"></i> @group.Key
                                          </td>
                                      </tr>
                                      foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))
                                      {
                                      <tr>'''

html = re.sub(pattern, replace_block, html, count=1, flags=re.DOTALL)

# Add the closing bracket for the outer loop
html = re.sub(r'(\s*)\}\n(\s*)\}\n(\s*)</tbody>', r'\1}\n\2}\n\3}\n\3</tbody>', html, count=1)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("SUCCESS")
