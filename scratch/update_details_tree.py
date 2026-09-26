import sys
filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will replace the inner rendering of the table rows.
# The table rows start at `<tr class="table-light">` for the group, and then `foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.Id))`

start_marker = "var groupedDocs = docs.GroupBy(d => d.Stage ?? \"Diğer Evraklar ve Kriterler\").ToList();"
end_marker = "</table>"

if start_marker in content and end_marker in content:
    start_idx = content.find(start_marker)
    end_idx = content.find(end_marker, start_idx)
    
    old_block = content[start_idx:end_idx]
    
    new_block = """var groupedDocs = docs.GroupBy(d => d.Stage ?? "Diğer Evraklar ve Kriterler").ToList();
                                  var globalRules = ViewBag.GlobalRules as List<GMK360.Core.Entities.ModuleDocumentRule> ?? new List<GMK360.Core.Entities.ModuleDocumentRule>();
                                  var renderedDocIds = new HashSet<int>();

                                  foreach(var group in groupedDocs)
                                  {
                                      <tr class="table-light">
                                          <td colspan="7" class="fw-bold text-dark py-3 ps-3">
                                              <i class="bi bi-folder2-open text-primary me-2"></i> @group.Key
                                          </td>
                                      </tr>
                                      foreach (var doc in group.OrderByDescending(x => x.IsCustom).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id))
                                      {
                                          if (renderedDocIds.Contains(doc.Id)) continue;
                                          
                                          // Ana kural mı?
                                          var rule = globalRules.FirstOrDefault(r => r.SystemLegalDocumentTemplateId == doc.SystemTemplateId);
                                          
                                          // Başka bir kuralın alt ön koşulu mu? (Eğer öyleyse, ve ana kural değilse atla, ağacın içinde render edilecek)
                                          bool isOnlyPrereq = globalRules.Any(r => r.Prerequisites != null && r.Prerequisites.Any(p => p.PrerequisiteTemplateId == doc.SystemTemplateId)) && rule == null;
                                          
                                          if (isOnlyPrereq) continue; // Parent'ın altında render edeceğiz
                                          
                                          // ANA EVRAK RENDER (ya da sahipsiz düz evrak)
                                          <tr>
                                              <td class="fw-bold ps-3">
                                                  @(doc.SystemTemplate?.Name ?? doc.DocumentName)
                                                  @if(doc.IsCustom) { <span class="badge bg-info ms-1" title="Sizin Eklediğiniz Özel Evrak">Özel</span> }
                                              </td>
                                              <td><span class="px-2 py-1 bg-light text-dark border rounded small"><i class="bi bi-building"></i> @(doc.InstitutionContact ?? doc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")</span></td>
                                              <td>
                                                  @if(!string.IsNullOrEmpty(doc.InstitutionContact)) { <span class="text-primary"><i class="bi bi-person-lines-fill"></i> @doc.InstitutionContact</span> } else { <span class="text-muted fst-italic">Bilinmiyor</span> }
                                              </td>
                                              <td>
                                                  @if(string.IsNullOrEmpty(doc.AssignedUserId)) { <span class="text-danger fw-bold"><i class="bi bi-exclamation-circle"></i> Atanmadı</span> } else { <span><i class="bi bi-person-badge text-primary"></i> @doc.AssignedUserId</span> }
                                              </td>
                                              <td>
                                                  @if (doc.Status == "Bekliyor")
                                                  {
                                                      <span class="px-3 py-1 bg-danger text-white rounded-pill small fw-bold"><i class="bi bi-x-circle"></i> Yok</span>
                                                  }
                                                  else if (doc.Status == "İşlemde")
                                                  {
                                                      <span class="px-3 py-1 bg-warning text-dark rounded-pill small fw-bold"><i class="bi bi-hourglass-split"></i> Başvuruldu</span>
                                                  }
                                                  else if (doc.Status == "Sorunlu")
                                                  {
                                                      <span class="px-3 py-1 bg-secondary text-white rounded-pill small fw-bold"><i class="bi bi-exclamation-triangle"></i> Sorun Var</span>
                                                  }
                                                  else
                                                  {
                                                      <span class="px-3 py-1 bg-success text-white rounded-pill small fw-bold"><i class="bi bi-check-circle"></i> Alındı</span>
                                                  }
                                              </td>
                                              <td>@(doc.Notes ?? "-")</td>
                                              <td class="text-end pe-3">
                                                  <a href="#" class="btn btn-sm btn-outline-primary py-0 px-2 rounded-3 text-decoration-none" title="Düzenle"><i class="bi bi-pencil-square me-1"></i> Yönet</a>
                                              </td>
                                          </tr>
                                          renderedDocIds.Add(doc.Id);

                                          // EĞER ANA EVRAKSA VE ÖN KOŞULLARI VARSA AĞAÇ YAPISINI RENDER ET
                                          if (rule != null && rule.Prerequisites != null && rule.Prerequisites.Any())
                                          {
                                              foreach (var pr in rule.Prerequisites)
                                              {
                                                  var childDoc = docs.FirstOrDefault(d => d.SystemTemplateId == pr.PrerequisiteTemplateId);
                                                  if (childDoc != null && !renderedDocIds.Contains(childDoc.Id))
                                                  {
                                                      <tr style="background-color: #fafafa;">
                                                          <td class="ps-4" style="border-left: 3px solid #ffc107;">
                                                              <i class="bi bi-arrow-return-right text-muted me-2"></i>
                                                              <span class="fw-semibold text-dark">@(childDoc.SystemTemplate?.Name ?? childDoc.DocumentName)</span>
                                                          </td>
                                                          <td><span class="px-2 py-1 text-dark border rounded small" style="background-color: #fff;"><i class="bi bi-building"></i> @(childDoc.InstitutionContact ?? childDoc.SystemTemplate?.IssuedBy ?? "Belirtilmedi")</span></td>
                                                          <td>
                                                              @if(!string.IsNullOrEmpty(childDoc.InstitutionContact)) { <span class="text-primary"><i class="bi bi-person-lines-fill"></i> @childDoc.InstitutionContact</span> } else { <span class="text-muted fst-italic">Bilinmiyor</span> }
                                                          </td>
                                                          <td>
                                                              @if(string.IsNullOrEmpty(childDoc.AssignedUserId)) { <span class="text-danger fw-bold"><i class="bi bi-exclamation-circle"></i> Atanmadı</span> } else { <span><i class="bi bi-person-badge text-primary"></i> @childDoc.AssignedUserId</span> }
                                                          </td>
                                                          <td>
                                                              @if (childDoc.Status == "Bekliyor")
                                                              {
                                                                  <span class="px-3 py-1 bg-danger text-white rounded-pill small fw-bold"><i class="bi bi-x-circle"></i> Yok</span>
                                                              }
                                                              else if (childDoc.Status == "İşlemde")
                                                              {
                                                                  <span class="px-3 py-1 bg-warning text-dark rounded-pill small fw-bold"><i class="bi bi-hourglass-split"></i> Başvuruldu</span>
                                                              }
                                                              else if (childDoc.Status == "Sorunlu")
                                                              {
                                                                  <span class="px-3 py-1 bg-secondary text-white rounded-pill small fw-bold"><i class="bi bi-exclamation-triangle"></i> Sorun Var</span>
                                                              }
                                                              else
                                                              {
                                                                  <span class="px-3 py-1 bg-success text-white rounded-pill small fw-bold"><i class="bi bi-check-circle"></i> Alındı</span>
                                                              }
                                                          </td>
                                                          <td>@(childDoc.Notes ?? "-")</td>
                                                          <td class="text-end pe-3">
                                                              <a href="#" class="btn btn-sm btn-outline-primary py-0 px-2 rounded-3 text-decoration-none" title="Düzenle"><i class="bi bi-pencil-square me-1"></i> Yönet</a>
                                                          </td>
                                                      </tr>
                                                      renderedDocIds.Add(childDoc.Id);
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                        """
    content = content.replace(old_block, new_block)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
else:
    print("Could not find start/end markers")
