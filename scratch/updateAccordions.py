import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Wrap ESKİ BİNA accordion in an if statement
old_accordion_pattern = re.compile(r'(<!-- ESKİ BİNA \(MEVCUT DURUM\) -->\s*<div class="accordion-item border-0 rounded-4 overflow-hidden mb-3 shadow-sm">.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>)', re.DOTALL)
html = old_accordion_pattern.sub(r'@if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding))\n    {\n    \1\n    }', html)

# Do the same for YENİ BİNA, just in case (though there usually is a new building)
new_accordion_pattern = re.compile(r'(<!-- YENİ BİNA \(HEDEF DURUM\) -->\s*<div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm">.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>)', re.DOTALL)
html = new_accordion_pattern.sub(r'@if (Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))\n    {\n    \1\n    }', html)


with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("Done")
