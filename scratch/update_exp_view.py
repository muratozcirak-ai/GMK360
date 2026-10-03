import codecs
import re

path = 'GMK360.Web/Views/ConstructionProjectExpenses/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# 1. Change Title
content = content.replace('Proje Genel Giderleri (Akaryakıt, İaşe, Ağırlama)', 'Şirket Genel Giderleri (Şantiye & Ofis)')

# 2. Add enctype to form
content = content.replace('<form asp-action="Create" method="post">', '<form asp-action="Create" method="post" enctype="multipart/form-data">')

# 3. Update the Project Select in the Modal
target_modal = r'<div class="mb-3">\s*<label class="form-label fw-bold">Proje</label>\s*<select name="ProjectId" class="form-select" required>\s*<option value="">-- Proje Se.iniz --</option>\s*@foreach\s*\(\s*var p in ViewBag.Projects\s*\)\s*\{\s*<option value="@p.Value">@p.Text</option>\s*\}\s*</select>\s*</div>'
replacement_modal = '''<div class="mb-3">
                            <label class="form-label fw-bold text-dark">Giderin Ait Olduğu Yer</label>
                            <select name="ProjectId" class="form-select">
                                <option value="">Şirket İçi (Merkez / Ofis / Genel)</option>
                                @foreach (var p in ViewBag.Projects)
                                {
                                    <option value="@p.Value">Proje: @p.Text</option>
                                }
                            </select>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold text-dark">Fiş / Fatura Fotoğrafı (Kameradan Çek / Yükle)</label>
                            <input type="file" name="photo" class="form-control" accept="image/*" capture="environment" />
                            <small class="text-muted d-block mt-1" style="font-size:0.75rem;"><i class="bi bi-camera"></i> Telefondan direkt fotoğraf çekebilirsiniz.</small>
                        </div>'''
content = re.sub(target_modal, replacement_modal, content, flags=re.DOTALL)

# 4. In the list, show "Şirket İçi" if Project is null, and show Photo icon.
content = content.replace('<td>@item.Project.Name</td>', '<td>@(item.Project?.Name ?? "Şirket İçi (Genel Merkez)")</td>')

content = content.replace('<td>@item.DocumentNo</td>', '''<td>
                                    @item.DocumentNo
                                    @if(!string.IsNullOrEmpty(item.PhotoPath)) { 
                                        <br/><a href="@item.PhotoPath" target="_blank" class="badge bg-primary text-decoration-none mt-1"><i class="bi bi-image"></i> Fiş Görseli</a> 
                                    }
                                </td>''')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('View updated.')