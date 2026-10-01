import codecs

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# We need to find the stakeholder tr block
start_marker = 'foreach (var st in stakeholdersList)'
end_marker = '} // End of foreach'

import re
# Regex to find the whole foreach block body for stakeholders
match = re.search(r'(foreach\s*\(var st in stakeholdersList\)\s*\{)(.*?)(?:<td colspan="5"|<tr colspan)', content, flags=re.DOTALL)

if match:
    prefix = match.group(1)
    # the existing row HTML is basically group(2), but it's tricky to replace perfectly. Let's just find the exact <tr>...</tr> inside foreach.
    row_regex = re.search(r'<tr>\s*<td.*?</tr>', match.group(2), flags=re.DOTALL)
    if row_regex:
        old_row = row_regex.group(0)
        new_row = '''<tr>
                                          <td class="fw-bold align-middle">
                                              <i class="bi bi-person-circle text-secondary me-2"></i> @st.User.FirstName @st.User.LastName
                                          </td>
                                          <td class="align-middle">
                                              <span class="d-block mb-1">@(string.IsNullOrEmpty(st.User.TcIdentityNo) ? "Kayıtlı Değil" : st.User.TcIdentityNo)</span>
                                              @if (st.User.EmailConfirmed || st.User.PhoneNumberConfirmed)
                                              {
                                                  <span class="badge bg-success bg-opacity-10 text-success border border-success"><i class="bi bi-check-circle-fill me-1"></i> Onaylı / Girdi</span>
                                              }
                                              else
                                              {
                                                  <span class="badge bg-warning bg-opacity-10 text-warning border border-warning" title="Sisteme henüz giriş yapmadı"><i class="bi bi-hourglass-split me-1"></i> SMS Bekliyor</span>
                                              }
                                          </td>
                                          <td class="align-middle">
                                              @if (st.Role == GMK360.Core.Entities.Construction.StakeholderRole.Landowner)
                                              {
                                                  <span class="badge bg-primary px-3 py-2">Arsa Sahibi</span>
                                              }
                                              else if (st.Role == GMK360.Core.Entities.Construction.StakeholderRole.Representative)
                                              {
                                                  <span class="badge bg-secondary px-3 py-2">Temsilci / Avukat</span>
                                              }
                                              else
                                              {
                                                  <span class="badge bg-info text-dark px-3 py-2">@st.Role.ToString()</span>
                                              }
                                          </td>
                                          <td class="fw-bold text-primary align-middle fs-5">
                                              @(st.SharePercentage.HasValue ? $"% {st.SharePercentage.Value.ToString("N2")}" : "-")
                                          </td>
                                          <td class="align-middle text-end pe-4">
                                              <button type="button" class="btn btn-sm btn-outline-warning rounded-pill shadow-sm me-1" onclick="editStakeholder('@st.Id', '@st.Role', '@(st.SharePercentage.HasValue ? st.SharePercentage.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "")')" title="Düzenle">
                                                  <i class="bi bi-pencil-square"></i>
                                              </button>
                                              <button class="btn btn-sm btn-outline-primary rounded-pill shadow-sm" title="Mesaj Gönder"><i class="bi bi-chat-dots"></i></button>
                                          </td>
                                      </tr>'''
        
        content = content.replace(old_row, new_row)
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print("Replaced row HTML successfully!")
    else:
        print("Row not found")
else:
    print("Foreach not found")