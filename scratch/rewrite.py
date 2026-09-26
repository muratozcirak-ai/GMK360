import re

with open('scratch/Details.txt', 'r', encoding='utf-8') as f:
    html = f.read()

# 1. Extract the map button from Proje Özeti
map_btn_pattern = r'@if\(Model\.Latitude != null && Model\.Longitude != null\)\s*\{\s*<div class="mb-3">\s*<a href="https://www\.google\.com/maps[^>]+>\s*<i class="bi bi-geo-alt-fill me-1"></i> Lojistik ve Harita Konumu \(Google Maps\)\s*</a>\s*</div>\s*\}'
map_btn_match = re.search(map_btn_pattern, html)
map_btn_html = map_btn_match.group(0) if map_btn_match else ''
html = re.sub(map_btn_pattern, '', html)

# 2. Extract 'Çevre Özellikleri' block
env_pattern = r'<div class="mt-4 pt-3 border-top position-relative">\s*<div class="d-flex justify-content-between align-items-center mb-3">\s*<h6 class="fw-bold mb-0 text-muted"><i class="bi bi-geo-alt text-danger me-1"></i> Çevre Özellikleri ve Yakın Konumlar</h6>\s*<button type="button" class="btn btn-sm btn-outline-danger rounded-pill"><i class="bi bi-arrow-repeat me-1"></i> Haritadan Güncelle</button>\s*</div>\s*<div class="p-4 bg-light rounded-3 border">\s*<div class="d-flex align-items-center">\s*<i class="bi bi-geo text-secondary fs-3 me-3"></i>\s*<div>\s*<h6 class="fw-bold mb-1">Konum Verileri Bekleniyor</h6>\s*<div class="small text-muted">Sistem canlıya alındığında Google Haritalar \[Places API\] üzerinden hastane, okul, park gibi çevre özellikleri buraya otomatik çekilecektir\.</div>\s*</div>\s*</div>\s*</div>\s*</div>'
env_match = re.search(env_pattern, html)
env_html = env_match.group(0) if env_match else ''
html = re.sub(env_pattern, '', html)

# If regex for env_pattern fails because of Turkish characters or formatting, let's just use string replace.
