import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix Faz 8 link
content = content.replace('<a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-success border-start border-3">',
                          '<a href="/PhaseEight/Index/@Model.Id" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-success border-start border-3">')

# Add Global Sync Button next to 'Durum: Projelendirme / Teklif'
target_btn = r'<button class="btn btn-warning rounded-pill shadow-sm px-4 fw-bold">.*?Yeni Paydaş / Temsilci Ekle.*?</button>'
replacement_btn = '''<button class="btn btn-warning rounded-pill shadow-sm px-4 fw-bold">
                        <i class="bi bi-person-plus-fill me-1"></i> Yeni Paydaş / Temsilci Ekle
                    </button>
                    <form action="/ConstructionProject/SyncAllPhases" method="post" class="d-inline ms-2">
                        <input type="hidden" name="projectId" value="@Model.Id" />
                        <button type="submit" class="btn btn-primary rounded-pill shadow-sm px-4 fw-bold">
                            <i class="bi bi-cloud-download me-1"></i> Tüm Fazları Havuzdan Çek
                        </button>
                    </form>'''
content = re.sub(target_btn, replacement_btn, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)