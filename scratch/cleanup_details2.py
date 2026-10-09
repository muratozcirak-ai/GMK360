import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Remove Tevhit button at the top
content = re.sub(r'<a href="#" class="btn btn-dark fw-bold ms-2" data-bs-toggle="modal" data-bs-target="#mergeBlocksModal">\s*<i class="bi bi-intersect"></i> Tevhit \(Birleştir\)\s*</a>', '', content, flags=re.DOTALL)

# Remove the Tevhit Modal completely
content = re.sub(r'<!-- TEVHİT.*?</div>\s*</div>\s*</div>\s*</div>', '', content, flags=re.DOTALL)

# Let's ensure the yellow alert is gone
content = re.sub(r'<div class="alert alert-warning.*?Tevhit \(Birleştirme\).*?</button>\s*</div>', '', content, flags=re.DOTALL)


with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Details.cshtml deeply cleaned.")
