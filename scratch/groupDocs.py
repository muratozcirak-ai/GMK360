import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace the tbody loop with a grouped one
old_tbody = '''<tbody>
                              @if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                              {
                                  <tr>
                                      <td colspan="7" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</td>
                                  </tr>
                              }
                              else
                              {
                                  foreach (var doc in ((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))
                                  {
                                      <tr>'''

new_tbody = '''<tbody>
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

if old_tbody not in html:
    print("WARNING: Could not find exact tbody to replace.")
    
    # Try regex
    import re
    html = re.sub(
        r'<tbody>\s*@if \(\(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\) == null.*?\{\s*foreach \(var doc in \(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\)\.OrderByDescending\(x => x\.IsCustom\)\.ThenBy\(x => x\.Id\)\)\s*\{\s*<tr>',
        new_tbody,
        html,
        flags=re.DOTALL
    )
else:
    html = html.replace(old_tbody, new_tbody)

# Since we opened a group loop, we must close it at the end of the if block
html = html.replace(
    '                                  }\n                              }',
    '                                  }\n                                  }\n                              }'
)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
