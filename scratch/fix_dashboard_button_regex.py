import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''<div class="d-flex justify-content-between align-items-center mb-3">
    <h5 class="fw-bold text-dark mb-0"><i class="bi bi-diagram-3-fill text-primary me-2"></i> Proje Fazları (Aşama Yönetimi)</h5>
    <form action="/ConstructionProject/SyncAllPhases" method="post" class="m-0 p-0">
        <input type="hidden" name="projectId" value="@Model.Id" />
        <button type="submit" class="btn btn-sm btn-primary rounded-pill shadow-sm fw-bold">
            <i class="bi bi-cloud-download me-1"></i> Tüm Fazları Havuzdan Çek
        </button>
    </form>
</div>'''

# Find <h5 class="fw-bold text-dark mb-3"> and replace it up to </h5>
content = re.sub(r'<h5 class="fw-bold text-dark mb-3">.*?</h5>', replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)