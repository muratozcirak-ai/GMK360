import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_status_logic = """<td class="text-center">
                                                @if (isDependent && !isReadyToApply) { <span class="badge bg-danger rounded-pill px-3 shadow-sm"><i class="bi bi-lock-fill"></i> Kilitli (Önkoşul)</span> }
                                                else if (doc.Status == "Fiyat Araştırılıyor") { <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> Fiyat Araştırılıyor</span> }"""

new_status_logic = """<td class="text-center">
                                                @if (doc.Status == "Fiyat Araştırılıyor") { <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> Fiyat Araştırılıyor</span> }
                                                else if (isDependent && !isReadyToApply) { <span class="badge bg-danger rounded-pill px-3 shadow-sm"><i class="bi bi-lock-fill"></i> Kilitli (Önkoşul)</span> }"""
content = content.replace(old_status_logic, new_status_logic)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed Status condition order.")
