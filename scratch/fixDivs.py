import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# First, remove the bad if statements we added
html = re.sub(r'@if \(Model\.Blocks != null && Model\.Blocks\.Any\(b => b\.IsExistingBuilding\)\)\n\s*\{\n\s*<!-- ESKİ BİNA', '<!-- ESKİ BİNA', html)
html = re.sub(r'\}\n\s*</div>\n\s*@if \(Model\.Blocks != null && Model\.Blocks\.Any\(b => !b\.IsExistingBuilding\)\)\n\s*\{\n\s*<!-- YENİ BİNA', '</div>\n    <!-- YENİ BİNA', html)
html = re.sub(r'\}\n\s*</div>\n</div>\n\n<!-- TASLAK', '</div>\n</div>\n\n<!-- TASLAK', html)

# Now, properly wrap them.
# The accordion structure is:
# <div class="accordion mb-4 shadow-sm" id="accordionSummaries">
#    <!-- ESKİ BİNA -->
#    <div class="accordion-item ..."> ... </div>
#    <!-- YENİ BİNA -->
#    <div class="accordion-item ..."> ... </div>
# </div>

pattern_old = re.compile(r'(<!-- ESKİ BİNA \(MEVCUT DURUM\) -->\s*<div class="accordion-item border-0 rounded-4 overflow-hidden mb-3 shadow-sm">.*?<!-- YENİ BİNA \(HEDEF DURUM\) -->)', re.DOTALL)
def replacer_old(m):
    content = m.group(1)
    content = content.replace('<!-- YENİ BİNA (HEDEF DURUM) -->', '')
    return '@if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding))\n{\n' + content + '\n}\n<!-- YENİ BİNA (HEDEF DURUM) -->'

html = pattern_old.sub(replacer_old, html)

pattern_new = re.compile(r'(<!-- YENİ BİNA \(HEDEF DURUM\) -->\s*<div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm">.*?)(</div>\n</div>\n\n<!-- TASLAK)', re.DOTALL)
def replacer_new(m):
    content = m.group(1)
    tail = m.group(2)
    return '@if (Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))\n{\n' + content + '\n}\n' + tail

html = pattern_new.sub(replacer_new, html)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("Done")
