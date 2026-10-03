import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the H5 heading
target = r'<h5 class="fw-bold text-dark mb-3"><i class="bi bi-diagram-3-fill text-primary me-2"></i> Proje Fazlar.*?\s*Ynetimi\)</h5>'
replacement = '''<div class="d-flex justify-content-between align-items-center mb-3">
    <h5 class="fw-bold text-dark mb-0"><i class="bi bi-diagram-3-fill text-primary me-2"></i> Proje Fazları (Aşama Yönetimi)</h5>
    <form action="/ConstructionProject/SyncAllPhases" method="post" class="m-0 p-0">
        <input type="hidden" name="projectId" value="@Model.Id" />
        <button type="submit" class="btn btn-sm btn-primary rounded-pill shadow-sm fw-bold">
            <i class="bi bi-cloud-download me-1"></i> Tüm Fazları Havuzdan Çek
        </button>
    </form>
</div>'''

# Fallback string replace if regex fails due to encoding
if not re.search(target, content):
    print("Regex failed, trying direct replace...")
    content = content.replace('<h5 class="fw-bold text-dark mb-3"><i class="bi bi-diagram-3-fill text-primary me-2"></i> Proje Fazlar (Aama Ynetimi)</h5>', replacement)
else:
    content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)