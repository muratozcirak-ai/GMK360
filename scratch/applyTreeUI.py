import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Update Model declaration
text = re.sub(
    r'@model\s+IEnumerable<GMK360\.Core\.Entities\.SystemLegalDocumentTemplate>',
    '@model List<GMK360.Web.Models.DocumentTreeDto>',
    text
)

# 2. Update Table Header (Remove Prerequisite column as it is now a tree)
text = text.replace('<th>Aşama (Faz)</th>\n                              <th>Ön Koşullar</th>', '<th>Aşama (Faz)</th>')

# 3. Replace the foreach loop and table body
new_body = """
                          <tbody>
                          @if (!Model.Any())
                          {
                              <tr>
                                  <td colspan="6" class="text-center text-muted py-4">Sistemde henüz evrak şablonu bulunmuyor.</td>
                              </tr>
                          }
                          else
                          {
                              @foreach (var item in Model)
                              {
                                  var doc = item.Document;
                                  var paddingLeft = item.Level * 40;
                                  var isRoot = item.IsRoot;
                                  
                                  <tr class="@(isRoot ? "table-light fw-bold" : "")">
                                      <td class="ps-4">
                                          <div style="padding-left: @(paddingLeft)px;">
                                              @if(!isRoot) {
                                                  <i class="ph ph-arrow-elbow-down-right text-danger me-2"></i>
                                              }
                                              @doc.Name
                                              @if (doc.IsMandatory)
                                              {
                                                  <span class="badge bg-danger ms-2"><i class="ph ph-warning-circle me-1"></i>Zorunlu</span>
                                              }
                                          </div>
                                      </td>
                                      <td>
                                          <span class="badge bg-secondary">
                                              @(doc.TargetModule switch {
                                                  "Construction" => "İnşaat / Şantiye",
                                                  "RealEstate" => "Emlak",
                                                  "ServiceProvider" => "Taşeron",
                                                  _ => doc.TargetModule
                                              })
                                          </span>
                                      </td>
                                      <td><span class="badge bg-info text-dark">@(doc.Stage ?? "-")</span></td>
                                      <td>@(doc.IssuedBy ?? "-")</td>
                                      <td class="text-muted small">@(doc.LegalReference ?? "-")</td>
                                      <td class="text-end pe-4">
                                          <button type="button" class="btn btn-sm btn-outline-primary rounded-circle me-1 edit-btn"
                                                  data-id="@doc.Id"
                                                  data-name="@doc.Name"
                                                  data-stage="@doc.Stage"
                                                  data-module="@doc.TargetModule"
                                                  data-issuedby="@doc.IssuedBy"
                                                  data-ismandatory="@(doc.IsMandatory ? "true" : "false")"
                                                  data-prereqs="@(doc.Prerequisites != null ? string.Join(",", doc.Prerequisites.Select(p => p.PrerequisiteDocumentId)) : "")">
                                              <i class="ph ph-pencil"></i>
                                          </button>
                                          <form asp-action="Delete" method="post" class="d-inline" onsubmit="return confirm('Bu evrak şablonunu silmek istediğinize emin misiniz?');">
                                              <input type="hidden" name="id" value="@doc.Id" />
                                              <button type="submit" class="btn btn-sm btn-outline-danger rounded-circle"><i class="ph ph-trash"></i></button>
                                          </form>
                                      </td>
                                  </tr>
                              }
                          }
                          </tbody>
"""

# Match everything between <tbody> and </tbody>
text = re.sub(r'<tbody>.*?</tbody>', new_body, text, flags=re.DOTALL)

# 4. Update the Javascript part for Edit button click
new_script = """@section Scripts {
    <script>
        $(document).ready(function() {
            $('.select2-multiple').select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: $('#addModal')
            });
            
            $('#edit-prereqs').select2({
                placeholder: "Ön koşul belgelerini seçiniz...",
                allowClear: true,
                width: '100%',
                dropdownParent: $('#editModal')
            });
            
            $('.edit-btn').on('click', function() {
                var btn = $(this);
                $('#edit-id').val(btn.data('id'));
                $('#edit-name').val(btn.data('name'));
                $('#edit-stage').val(btn.data('stage'));
                $('#edit-module').val(btn.data('module'));
                $('#edit-issuedby').val(btn.data('issuedby'));
                $('#edit-mand').prop('checked', btn.data('ismandatory') === true || btn.data('ismandatory') === 'true');
                
                var prereqsStr = btn.data('prereqs');
                if(prereqsStr && prereqsStr.toString().trim() !== '') {
                    $('#edit-prereqs').val(prereqsStr.toString().split(',')).trigger('change');
                } else {
                    $('#edit-prereqs').val(null).trigger('change');
                }
                
                $('#editModal').modal('show');
            });
        });
    </script>
}"""

text = re.sub(r'@section Scripts \{.*?\}', new_script, text, flags=re.DOTALL)

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print('Tree UI Applied')
