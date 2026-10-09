import re

with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

js_snippet = '''
            // Populate Add Unit Modal automatically when clicking "Bu Kata Yeni Birim Ekle"
            .btn-add-unit-to-floor.click(function() {
                var floorLevel = .data('floorlevel');
                var floorName = .data('floorname');
                if (floorLevel !== undefined) {
                    #addUnitModal input[name="FloorLevel"].val(floorLevel);
                }
                if (floorName !== undefined) {
                    #addUnitModal input[name="FloorName"].val(floorName);
                }
            });
'''

idx_ready = content.find('.ready(function() {')
if idx_ready != -1:
    idx_insert = content.find('{', idx_ready) + 1
    content = content[:idx_insert] + js_snippet + content[idx_insert:]
    
with open(r'GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Add Unit modal JS injected")
