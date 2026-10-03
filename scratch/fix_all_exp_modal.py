import codecs
import re

path = 'GMK360.Web/Views/ConstructionProjectExpenses/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix Modal Form Enctype
content = content.replace('<form asp-action="Create" method="post" class="modal-content">', '<form asp-action="Create" method="post" class="modal-content" enctype="multipart/form-data">')

# Fix Project Dropdown
target_project_dropdown = r'<div class="col-12">\s*<label class="form-label">.lgili Proje</label>\s*<select name="ProjectId" class="form-select" asp-items="ViewBag\.Projects" required></select>\s*</div>'
replacement_project_dropdown = '''<div class="col-12">
                        <label class="form-label fw-bold">Giderin Ait Olduğu Yer</label>
                        <select name="ProjectId" class="form-select">
                            <option value="">Şirket İçi (Merkez / Ofis / Genel)</option>
                            @foreach(var p in (IEnumerable<SelectListItem>)ViewBag.Projects) {
                                <option value="@p.Value">Proje: @p.Text</option>
                            }
                        </select>
                    </div>'''
content = re.sub(target_project_dropdown, replacement_project_dropdown, content)

# Fix ExpenseType Dropdown in Modal
target_exp_type = r'<select name="ExpenseType" class="form-select" required>.*?</select>'
replacement_exp_type = '''<select name="ExpenseType" class="form-select" required>
                            <option value="1">Şantiye İaşesi (Yemek, Çay, Su)</option>
                            <option value="2">Temsil & Ağırlama (Müşteri Yemek, Lansman)</option>
                            <option value="4">Şirket İçi Yemek</option>
                            <option value="5">İletişim (Telefon, İnternet)</option>
                            <option value="6">Demirbaş (Masa, Bilgisayar vs.)</option>
                            <option value="7">Sarf Malzeme (Kırtasiye, Kartuş vs.)</option>
                            <option value="8">Temizlik Malzemeleri</option>
                            <option value="3">Diğer Genel Gider (Akaryakıt, Ulaşım)</option>
                        </select>'''
content = re.sub(target_exp_type, replacement_exp_type, content, flags=re.DOTALL)

# Insert Photo Upload in Modal
target_photo = r'<div class="col-6">\s*<label class="form-label">Tarih</label>'
replacement_photo = '''<div class="col-12">
                          <label class="form-label fw-bold">Fiş / Fatura Fotoğrafı (Kameradan Çek / Yükle)</label>
                          <input type="file" name="photo" class="form-control" accept="image/*" capture="environment" />
                          <small class="text-muted d-block mt-1" style="font-size:0.75rem;"><i class="bi bi-camera"></i> Telefondan direkt fotoğraf çekebilirsiniz.</small>
                      </div>
                      <div class="col-6">
                          <label class="form-label">Tarih</label>'''
content = re.sub(target_photo, replacement_photo, content)

# Now, fix the FILTER section!
target_filter = r'<select name="expenseType" class="form-select">\s*<option value="">-- Tümü --</option>\s*<option value="1" selected="@\(ViewBag.SelectedType == 1\)">Şantiye İaşesi \(Yemek/Su\)</option>\s*<option value="2" selected="@\(ViewBag.SelectedType == 2\)">Temsil / Ağırlama</option>\s*<option value="3" selected="@\(ViewBag.SelectedType == 3\)">Diğer Genel Gider</option>\s*</select>'
replacement_filter = '''<select name="expenseType" class="form-select">
                        <option value="">-- Tümü --</option>
                        <option value="1" selected="@(ViewBag.SelectedType == 1)">Şantiye İaşesi</option>
                        <option value="2" selected="@(ViewBag.SelectedType == 2)">Temsil / Ağırlama</option>
                        <option value="4" selected="@(ViewBag.SelectedType == 4)">Şirket İçi Yemek</option>
                        <option value="5" selected="@(ViewBag.SelectedType == 5)">İletişim</option>
                        <option value="6" selected="@(ViewBag.SelectedType == 6)">Demirbaş</option>
                        <option value="7" selected="@(ViewBag.SelectedType == 7)">Sarf Malzeme</option>
                        <option value="8" selected="@(ViewBag.SelectedType == 8)">Temizlik</option>
                        <option value="3" selected="@(ViewBag.SelectedType == 3)">Diğer Genel Gider</option>
                    </select>'''
content = re.sub(target_filter, replacement_filter, content)

# And fix the list display mapping
target_list = r'<td>@item.ExpenseType.ToString\(\)</td>'
replacement_list = '''<td>
                                    @if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.SantiyeIasesi) { <span class="badge bg-warning text-dark">Şantiye İaşesi</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.TemsilAgirlama) { <span class="badge bg-danger">Temsil / Ağırlama</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.SirketIciYemek) { <span class="badge bg-info text-dark">Şirket İçi Yemek</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.Iletisim) { <span class="badge bg-secondary">İletişim</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.Demirbas) { <span class="badge bg-dark">Demirbaş</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.SarfMalzeme) { <span class="badge bg-light text-dark border">Sarf Malzeme</span> }
                                    else if(item.ExpenseType == GMK360.Core.Entities.Construction.ConstructionExpenseType.Temizlik) { <span class="badge bg-success">Temizlik</span> }
                                    else { <span class="badge bg-secondary">Diğer</span> }
                                </td>'''
content = content.replace('<td>@item.ExpenseType.ToString()</td>', replacement_list)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Modal rewritten perfectly.')