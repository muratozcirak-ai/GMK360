import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Let's fix the mangled button
# Find where the button is
pattern = re.compile(r'<button type="button" class="btn btn-dark rounded-pill px-4 fw-bold" onclick="alert[^>]*>.*?Binaları Birleştir</button>', re.DOTALL)
match = pattern.search(content)

fixed_btn = '''<button type="button" class="btn btn-dark rounded-pill px-4 fw-bold" onclick="Swal.fire('Başarılı', 'Geliştirme aşamasında. Yakında C# backend\\'e bağlanacak.', 'info'); var m = bootstrap.Modal.getInstance(document.getElementById('mergeBlocksModal')); if(m) m.hide();"><i class="bi bi-intersect me-2"></i> Binaları Birleştir</button>'''

if match:
    content = content[:match.start()] + fixed_btn + content[match.end():]
else:
    # try finding the original again if the previous script failed
    target_btn = '<button type="button" class="btn btn-dark rounded-pill px-4 fw-bold"><i class="bi bi-intersect me-2"></i> Binaları Birleştir</button>'
    content = content.replace(target_btn, fixed_btn)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Modal fixed properly")
