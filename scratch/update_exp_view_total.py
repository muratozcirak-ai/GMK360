import codecs
import re

path = 'GMK360.Web/Views/ConstructionProjectExpenses/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<div class="card shadow-sm border-0">'
replacement = '''<div class="row mb-4">
        <div class="col-12">
            <div class="card border-0 shadow-sm bg-danger text-white rounded-4">
                <div class="card-body p-4 d-flex justify-content-between align-items-center">
                    <h5 class="mb-0 fw-bold">Seçili Filtreye Göre Toplam Masraf:</h5>
                    <h2 class="mb-0 fw-bold">@Model.Sum(x => x.Amount).ToString("C2")</h2>
                </div>
            </div>
        </div>
    </div>

    <div class="card shadow-sm border-0">'''

content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)