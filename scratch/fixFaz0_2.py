import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

pattern = r'@if \(\(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\) == null \|\| !\(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\)\.Any\(\)\)\s*\{\s*<tr>\s*<td colspan="7" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş\.</td>\s*</tr>\s*\}\s*else\s*\{\s*foreach \(var doc in \(\(List<GMK360\.Core\.Entities\.Construction\.ProjectLegalDocument>\)ViewBag\.Phase0Docs\)\.OrderByDescending\(x => x\.IsCustom\)\.ThenBy\(x => x\.Id\)\)\s*\{'

replacement = '''@if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                              {
                                  <tr>
                                      <td colspan="7" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</td>
                                  </tr>
                              }
                              else
                              {
                                  var docs = (List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs;
                                  var groupedDocs = docs.GroupBy(d => d.Stage ?? "Diğer Evraklar ve Kriterler").ToList();
                                  foreach(var group in groupedDocs)
                                  {
                                      <tr class="table-light">
                                          <td colspan="7" class="fw-bold text-dark py-3 ps-3">
                                              <i class="bi bi-folder2-open text-primary me-2"></i> @group.Key
                                          </td>
                                      </tr>
                                      foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))
                                      {'''

if re.search(pattern, html, flags=re.DOTALL):
    html = re.sub(pattern, replacement, html, count=1, flags=re.DOTALL)
    print("MATCH 1 SUCCESS")
else:
    print("MATCH 1 NOT FOUND")

end_pattern_regex = r'                                          </td>\s*<td>\s*<a href="#" class="btn btn-sm btn-outline-primary rounded-pill px-3".*?<i class="bi bi-pencil me-1"></i> Yönet</a>\s*</td>\s*</tr>\s*\}\s*\}\s*</tbody>'

end_replacement = '''                                          </td>
                                          <td>
                                              <a href="#" class="btn btn-sm btn-outline-primary rounded-pill px-3" data-bs-toggle="modal" data-bs-target="#editDocModal" data-doc-id="@doc.Id"><i class="bi bi-pencil me-1"></i> Yönet</a>
                                          </td>
                                      </tr>
                                  }
                                  }
                              }
                          </tbody>'''

if re.search(end_pattern_regex, html, flags=re.DOTALL):
    html = re.sub(end_pattern_regex, end_replacement, html, count=1, flags=re.DOTALL)
    print("MATCH 2 SUCCESS")
else:
    print("MATCH 2 NOT FOUND")

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

