import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Remove the Stage count badge
target1 = """<span class="badge bg-secondary ms-auto rounded-pill me-3">@group.Count() Evrak</span>"""
content = content.replace(target1, "")

# 2. Simplify the "Başvurulabilir" / "Bağımlı" badges
target_success = """<span class="badge bg-success ms-2"><i class="bi bi-unlock-fill"></i> Başvurulabilir</span>"""
replacement_success = """<span class="text-success ms-2 fw-bold" style="font-size:0.85em;"><i class="bi bi-unlock-fill"></i> Başvurulabilir</span>"""
content = content.replace(target_success, replacement_success)

target_danger = """<span class="badge bg-danger ms-2"><i class="bi bi-lock-fill"></i> Bağımlı</span>"""
replacement_danger = """<span class="text-danger ms-2 fw-bold" style="font-size:0.85em;"><i class="bi bi-lock-fill"></i> Bağımlı</span>"""
content = content.replace(target_danger, replacement_danger)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
