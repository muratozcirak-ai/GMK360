import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# Add Select2 to Create Modal
select2_html = '''
                    <div class="mb-3">
                        <label class="form-label fw-bold">Ön Koşul Evrakları (Bu evraktan ÖNCE alınması gerekenler)</label>
                        <select name="PrerequisiteIds" class="form-select select2-multiple" multiple="multiple">
                            @foreach (var tmp in ViewBag.AllTemplates)
                            {
                                <option value="@tmp.Id">@tmp.Name</option>
                            }
                        </select>
                    </div>
'''
if 'PrerequisiteIds' not in text:
    text = text.replace('<div class="mb-3">\n                        <label class="form-label fw-bold">Yasal Dayanak', select2_html + '<div class="mb-3">\n                        <label class="form-label fw-bold">Yasal Dayanak')

# Add Edit Modal and Scripts at the bottom
edit_modal = '''
  <!-- Edit Modal -->
  <div class="modal fade" id="editModal" tabindex="-1">
      <div class="modal-dialog">
          <div class="modal-content rounded-4 border-0 shadow">
              <form asp-action="Edit" method="post">
                  <input type="hidden" name="Id" id="edit-id" />
                  <div class="modal-header border-bottom-0">
                      <h5 class="modal-title fw-bold text-primary">Evrak Şablonu Düzenle</h5>
                      <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                  </div>
                  <div class="modal-body">
                      <div class="mb-3">
                          <label class="form-label fw-bold">Evrak Adı</label>
                          <input type="text" name="Name" id="edit-name" class="form-control" required>
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Aşama (Faz)</label>
                          <select name="Stage" id="edit-stage" class="form-select">
                              <option value="">(Yok - Genel)</option>
                              <option value="1. Yıkım Öncesi ve Yıkım Aşaması Evrakları">1. Yıkım Öncesi ve Yıkım Aşaması Evrakları</option>
                              <option value="2. Yapım (İnşaat) Aşaması Evrakları">2. Yapım (İnşaat) Aşaması Evrakları</option>
                              <option value="3. Satış ve Teslim Aşaması Evrakları">3. Satış ve Teslim Aşaması Evrakları</option>
                          </select>
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Hangi Modül İçin Geçerli?</label>
                          <select name="TargetModule" id="edit-module" class="form-select">
                              <option value="Construction">İnşaat / Şantiye</option>
                              <option value="RealEstate">Emlak / Gayrimenkul</option>
                              <option value="ServiceProvider">Usta / Taşeron / Tedarikçi</option>
                          </select>
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Kimden Alınır / Başvuru Makamı</label>
                          <input type="text" name="IssuedBy" id="edit-issuedby" class="form-control">
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Ön Koşul Evrakları</label>
                          <select name="PrerequisiteIds" id="edit-prereqs" class="form-select select2-multiple" multiple="multiple">
                              @foreach (var tmp in ViewBag.AllTemplates)
                              {
                                  <option value="@tmp.Id">@tmp.Name</option>
                              }
                          </select>
                      </div>
                      <div class="mb-3 form-check form-switch">
                          <input class="form-check-input" type="checkbox" name="IsMandatory" value="true" id="edit-mand">
                          <label class="form-check-label fw-bold" for="edit-mand">Zorunlu Evrak</label>
                      </div>
                  </div>
                  <div class="modal-footer bg-light border-top-0 rounded-bottom-4">
                      <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                      <button type="submit" class="btn btn-primary rounded-pill px-4 fw-bold">Güncelle</button>
                  </div>
              </form>
          </div>
      </div>
  </div>
'''

script_block = '''
@section Scripts {
    <script>
        .ready(function() {
            .select2-multiple.select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: #addModal
            });
            
            #edit-prereqs.select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: #editModal
            });
        });

        function openEditModal(id, name, stage, module, issuedBy, isMand, prereqsStr) {
            #edit-id.val(id);
            #edit-name.val(name);
            #edit-stage.val(stage);
            #edit-module.val(module);
            #edit-issuedby.val(issuedBy);
            #edit-mand.prop('checked', isMand === 'True');
            
            if(prereqsStr) {
                var arr = prereqsStr.split(',');
                #edit-prereqs.val(arr).trigger('change');
            } else {
                #edit-prereqs.val(null).trigger('change');
            }
            
            #editModal.modal('show');
        }
    </script>
}
'''
if 'editModal' not in text:
    text = text + '\n' + edit_modal + '\n' + script_block

# Add alerts and Edit button
alerts_html = '''
@if(TempData["ErrorMessage"] != null)
{
    <div class="alert alert-danger fw-bold">@TempData["ErrorMessage"]</div>
}
@if(TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success fw-bold">@TempData["SuccessMessage"]</div>
}
'''
if 'TempData["ErrorMessage"]' not in text:
    text = text.replace('<div class="d-flex justify-content-between align-items-center mb-4">', alerts_html + '\n<div class="d-flex justify-content-between align-items-center mb-4">')

# Modify table to include prerequisites column and edit button
if '<th>Ön Koşullar</th>' not in text:
    text = text.replace('<th>Aşama (Faz)</th>', '<th>Aşama (Faz)</th>\n                              <th>Ön Koşullar</th>')
    
    # We need to construct the prereq string for javascript
    row_edit_html = '''
                                  <td class="text-muted small">
                                      @if (item.Prerequisites != null && item.Prerequisites.Any())
                                      {
                                          <ul class="mb-0 ps-3">
                                              @foreach(var p in item.Prerequisites)
                                              {
                                                  <li>@p.PrerequisiteDocument?.Name</li>
                                              }
                                          </ul>
                                      }
                                      else
                                      {
                                          <span>-</span>
                                      }
                                  </td>'''
                                  
    text = text.replace('<td><span class="badge bg-info text-dark">@(item.Stage ?? "-")</span></td>', '<td><span class="badge bg-info text-dark">@(item.Stage ?? "-")</span></td>\n' + row_edit_html)
    
    # Edit button
    edit_btn = '''<button type="button" class="btn btn-sm btn-outline-primary rounded-circle me-1" onclick="openEditModal(@item.Id, '@Html.Raw(item.Name?.Replace("'", "\\'"))', '@item.Stage', '@item.TargetModule', '@Html.Raw(item.IssuedBy?.Replace("'", "\\'"))', '@item.IsMandatory', '@(string.Join(",", item.Prerequisites.Select(p => p.PrerequisiteDocumentId)))')"><i class="ph ph-pencil"></i></button>'''
    
    text = text.replace('<form asp-action="Delete"', edit_btn + '\n                                      <form asp-action="Delete"')

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print('UI Updated')
