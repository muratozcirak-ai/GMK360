import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

phases = [
    ('3', 'Three', 'Kaba Yapı'),
    ('4', 'Four', 'Çatı ve İzolasyon'),
    ('5', 'Five', 'İnce İşler'),
    ('6', 'Six', 'Mekanik'),
    ('7', 'Seven', 'Elektrik'),
    ('8', 'Eight', 'Peyzaj ve Teslim')
]

for p_num, p_name, p_desc in phases:
    target = fr'<!-- FAZ {p_num} -->\s*<div class="col-md-4 col-lg-3">\s*<a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">'
    replacement = f'''<!-- FAZ {p_num} -->
            <div class="col-md-4 col-lg-3">
                <a href="/Phase{p_name}/Index/@Model.Id" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">'''
    content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)