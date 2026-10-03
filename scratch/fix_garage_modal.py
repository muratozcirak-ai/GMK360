import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<option value="">-- Müşteri Gezdirme / Nalbur vb. --</option>'
replacement = '<option value="">Şirket İçi (Merkez / Genel / Nalbur vb.)</option>'
content = content.replace(target, replacement)

target_label = r'<label class="form-label fw-bold">Hedef Şantiye \(Opsiyonel\)</label>'
replacement_label = '<label class="form-label fw-bold text-dark">Görev Yeri (Şirket / Şantiye)</label>'
content = re.sub(target_label, replacement_label, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)