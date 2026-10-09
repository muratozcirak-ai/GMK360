import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Fix modal header text color
content = content.replace('<h5 class="modal-title fw-bold"', '<h5 class="modal-title fw-bold text-white"')

# Add an onclick event to the submit button
target_btn = '<button type="button" class="btn btn-dark rounded-pill px-4 fw-bold"><i class="bi bi-intersect me-2"></i> Binaları Birleştir</button>'
new_btn = '<button type="button" class="btn btn-dark rounded-pill px-4 fw-bold" onclick="alert(\'Sistem Mesajı: Geliştirme aşamasında. Veritabanı bağlantısı (C#) yapılınca bu bloklar tek bir hedef bina olarak güncellenecektir.\'); .modal(\'hide\');"><i class="bi bi-intersect me-2"></i> Binaları Birleştir</button>'

content = content.replace(target_btn, new_btn)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Modal fixed")
