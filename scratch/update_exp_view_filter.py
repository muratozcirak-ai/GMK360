import codecs
import re

path = 'GMK360.Web/Views/ConstructionProjectExpenses/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<form method="get" class="row g-3">.*?<div class="col-md-8">.*?<label class="form-label fw-bold">Projeye G.re Filtrele</label>.*?<select name="projectId" class="form-select" asp-items="ViewBag\.Projects" onchange="this\.form\.submit\(\)">.*?<option value="">-- T.m Projeler --</option>.*?</select>.*?</div>.*?</form>'

replacement = '''<form method="get" class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label fw-bold">Giderin Yeri (Proje / Şirket)</label>
                    <select name="projectId" class="form-select" asp-items="ViewBag.Projects">
                        <option value="">-- Tüm Projeler / Şirket --</option>
                    </select>
                </div>
                <div class="col-md-3">
                    <label class="form-label fw-bold">Masraf Türü</label>
                    <select name="expenseType" class="form-select">
                        <option value="">-- Tümü --</option>
                        <option value="1" selected="@(ViewBag.SelectedType == 1)">Şantiye İaşesi (Yemek/Su)</option>
                        <option value="2" selected="@(ViewBag.SelectedType == 2)">Temsil / Ağırlama</option>
                        <option value="3" selected="@(ViewBag.SelectedType == 3)">Diğer Genel Gider</option>
                    </select>
                </div>
                <div class="col-md-2">
                    <label class="form-label fw-bold">Başlangıç</label>
                    <input type="date" name="startDate" class="form-control" value="@ViewBag.StartDate" />
                </div>
                <div class="col-md-2">
                    <label class="form-label fw-bold">Bitiş</label>
                    <input type="date" name="endDate" class="form-control" value="@ViewBag.EndDate" />
                </div>
                <div class="col-md-2 text-end">
                    <button type="submit" class="btn btn-primary w-100 fw-bold"><i class="bi bi-search"></i> Filtrele</button>
                </div>
            </form>'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)