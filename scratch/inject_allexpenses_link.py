import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<a href="/CompanyGarage/AllTasks"[^>]+>.*?</a>'
replacement = '''<a href="/CompanyGarage/AllExpenses" class="btn btn-outline-danger fw-bold rounded-pill px-4 shadow-sm me-2">
                <i class="bi bi-funnel-fill me-1"></i> Tüm Masraflar (Filtreli)
            </a>
            <a href="/CompanyGarage/AllTasks" class="btn btn-outline-primary fw-bold rounded-pill px-4 shadow-sm me-2">
                <i class="bi bi-list-task me-1"></i> Tüm Görev Listesi
            </a>'''

content = re.sub(target, replacement, content, count=1, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('AllExpenses link added to Index.cshtml.')