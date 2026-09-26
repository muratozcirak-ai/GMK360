import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with io.open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

content = re.sub(r'<label class="form-label fw-bold">Planlanan Ba.lang.*?\s*Tarihi</label>', '<label class="form-label fw-bold">Proje Başlama Tarihi</label>', content)
content = content.replace('<label class="form-label fw-bold">Hedeflenen Teslim</label>', '<label class="form-label fw-bold">Tahmini Teslim Tarihi</label>')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated Create.cshtml")
