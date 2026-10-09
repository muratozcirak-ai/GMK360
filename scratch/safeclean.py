import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will just remove the merge button link
content = re.sub(r'<a href="#" class="btn btn-dark fw-bold ms-2" data-bs-toggle="modal" data-bs-target="#mergeBlocksModal">\s*<i class="bi bi-intersect"></i> Tevhit \(Birleştir\)\s*</a>', '', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Restored and safely cleaned.")
