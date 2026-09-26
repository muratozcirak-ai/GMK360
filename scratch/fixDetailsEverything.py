import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# 1. Eski Bina target (Amenities)
eski_target = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>'
eski_replace = '<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>\n                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="true" class="btn btn-sm btn-outline-danger rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
if 'asp-route-isExisting="true"' not in html:
    html = re.sub(r'<h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar \(Mevcut\)</h6>', eski_replace, html)

# 2. Yeni Bina button
yeni_target = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
yeni_replace = '<a asp-action="Amenities" asp-route-projectId="@Model.Id" asp-route-isExisting="false" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>'
if 'asp-route-isExisting="false"' not in html:
    html = html.replace(yeni_target, yeni_replace)

# 3. Add Stage to Custom Doc Modal
target_modal = '''<div class="mb-3">
                        <label class="form-label fw-bold">Evrak / Kriter Adı</label>'''

insertion_modal = '''<div class="mb-3">
                        <label class="form-label fw-bold text-danger">Aşama Kategorisi</label>
                        <select class="form-select border-danger" name="stage" required>
                            <option value="1. Yıkım Öncesi ve Yıkım Aşaması Evrakları">1. Yıkım Öncesi ve Yıkım Aşaması Evrakları</option>
                            <option value="2. Yapım (İnşaat) Aşaması Evrakları">2. Yapım (İnşaat) Aşaması Evrakları</option>
                            <option value="3. Satış ve Teslim Aşaması Evrakları">3. Satış ve Teslim Aşaması Evrakları</option>
                            <option value="Diğer Evraklar ve Kriterler">Diğer Evraklar ve Kriterler</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Evrak / Kriter Adı</label>'''
if 'name="stage"' not in html:
    html = html.replace(target_modal, insertion_modal)

# 4. Do the Grouping!
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

if old_tbody in html:
    html = html.replace(old_tbody, new_tbody)
    # The original loop closes with } } at the end.
    # The new structure has oreach(group) { foreach(doc) { ... } }
    # We need to add one more } where it closes.
    
    # We will search for the end of the foreach loop.
    # The end of the foreach loop is followed by </tbody>
    html = html.replace('}\n                          </tbody>', '}\n                                  }\n                              }\n                          </tbody>')

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
