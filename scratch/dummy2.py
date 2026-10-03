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
                            @foreach(var p in ViewBag.Projects) {
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

# Add Photo Upload to Modal
target_modal_body = r'<div class="col-6">\s*<label class="form-label">Tarih</label>\s*<input type="date" name="ExpenseDate" class="form-control" value="@DateTime.UtcNow.ToString\("yyyy-MM-dd"\)" required />\s*</div>'
replacement_modal_body = target_modal_body.replace('r\'', '').replace('\'', '') # Wait, I will just do a string replacement.