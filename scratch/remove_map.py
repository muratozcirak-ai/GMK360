import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# We want to remove from <div class="card-body p-4 d-flex justify-content-between align-items-center flex-wrap gap-3">
# all the way down to the start of <!-- ÖZET ALANLARI (COLLAPSIBLE) -->

pattern = re.compile(
    r'<div class="card-body p-4 d-flex justify-content-between align-items-center flex-wrap gap-3">.*?<!-- ZET ALANLARI \(COLLAPSIBLE\) -->',
    re.DOTALL
)

# wait, the comment might be missing turkish characters or have different ones.
# let's just search for accordionOzet

pattern2 = re.compile(
    r'<div class="card-body p-4 d-flex justify-content-between align-items-center flex-wrap gap-3">.*?id="accordionOzet">',
    re.DOTALL
)

match = pattern2.search(content)
if match:
    # We remove the map, and preserve the accordionOzet div opening
    new_content = content[:match.start()] + '<!-- ÖZET ALANLARI (COLLAPSIBLE) -->\n<div class="accordion mb-4 shadow-sm" id="accordionOzet">' + content[match.end():]
    
    with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(new_content)
    print("Map removed!")
else:
    print("Map not found!")

