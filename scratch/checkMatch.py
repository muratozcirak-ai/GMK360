import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

pattern_blocks = re.compile(r'<h5 class="fw-bold mb-0"><i class="bi bi-buildings text-warning me-2"></i> Projedeki Bloklar / Binalar</h5>.*?<div class="row g-4">\s*@if \(Model\.Blocks != null && Model\.Blocks\.Any\(\)\)\s*\{.*?\}\s*\}\s*else.*?</div>', re.DOTALL)
match = pattern_blocks.search(html)
if match:
    print("MATCH FOUND!")
else:
    print("NO MATCH!")
