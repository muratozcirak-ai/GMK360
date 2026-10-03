import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will replace all instances of:
# <button class="btn btn-sm btn-light border rounded-pill w-100 fw-bold text-muted" disabled>Yakında</button>
# With:
# <a href="#" class="btn btn-sm btn-outline-primary rounded-pill w-100 fw-bold">Masaya Git <i class="bi bi-arrow-right ms-1"></i></a>

target = r'<button class="btn btn-sm btn-light border rounded-pill w-100 fw-bold text-muted" disabled>Yakında</button>'
replacement = '<a href="#" class="btn btn-sm btn-outline-primary rounded-pill w-100 fw-bold">Masaya Git <i class="bi bi-arrow-right ms-1"></i></a>'

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)