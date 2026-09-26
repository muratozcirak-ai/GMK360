import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

target = '''@if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                              {
                                  <tr>
                                      <td colspan="7" class="text-center text-muted py-4">Bu projeye henüz evrak/kriter eklenmemiş.</td>
                                  </tr>
                              }
                              else
                              {
                                  foreach (var doc in ((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))
                                  {'''

replace = '''@if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
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

# I will find the EXACT match, taking into account it might have the  characters!
# Wait, I already ran fixEncoding.py, so it has properly formatted Turkish characters now.
if target in html:
    html = html.replace(target, replace)
    print("TARGET REPLACED")
else:
    print("TARGET NOT FOUND EXACTLY")

# Now I must find the end of the loop to add the closing bracket.
# The end is:
#                                 }
#                             }
#                         </tbody>

end_target = '''                                }
                            }
                        </tbody>'''

end_replace = '''                                }
                                  }
                              }
                          </tbody>'''

if end_target in html:
    html = html.replace(end_target, end_replace)
    print("END TARGET REPLACED")
else:
    print("END TARGET NOT FOUND EXACTLY")

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
