import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<div class="text-end">\s*<h4 class="fw-bold text-primary mb-0">@Model\.Sum\(x => x\.PlannedTotalCost\)\.ToString\("N2"\) ₺</h4>'
replacement = '''<div class="text-end">
            <form action="/PhaseOne/SyncFromPool/@projectId" method="post" class="d-inline me-3">
                <button type="submit" class="btn btn-warning fw-bold shadow-sm rounded-pill px-4"><i class="bi bi-cloud-download me-1"></i> Havuzdan Senkronize Et</button>
            </form>
            <h4 class="fw-bold text-primary mb-0 d-inline-block">@Model.Sum(x => x.PlannedTotalCost).ToString("N2") ₺</h4>'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)