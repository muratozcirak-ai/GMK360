import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# I will find the EXACT string block from <tbody> down to <tr>
search_block = '''<tbody>
                              @if (((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs) == null || !((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).Any())
                              {
                                  <tr>
                                      <td colspan="7" class="text-center text-muted py-4">Bu projeye henz evrak/kriter eklenmemi.</td>
                                  </tr>
                              }
                              else
                              {
                                  foreach (var doc in ((List<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.Phase0Docs).OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))
                                  {
                                      <tr>'''

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

if search_block in html:
    html = html.replace(search_block, replace_block)
    # The original loop closes with:
    #                                     </td>
    #                                 </tr>
    #                             }
    #                         }
    #                     </tbody>
    
    html = html.replace(
        '                                  }\n                              }\n                          </tbody>',
        '                                  }\n                                  }\n                              }\n                          </tbody>'
    )
    with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
        f.write(html)
    print("SUCCESS")
else:
    print("SEARCH BLOCK NOT FOUND")
