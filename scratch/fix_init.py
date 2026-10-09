with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# Update handleLayoutPatternChange to accept skipSave
new_js = r"""function handleLayoutPatternChange(selectElem, skipSave) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var techDetails = container.querySelector('.tech-details');
            var subBlocksContainer = container.querySelector('.sub-blocks-container');
            
            if (val.includes('Tekil') || val.includes('Bağımsız')) {
                if(techDetails) techDetails.style.display = 'flex';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            } else if (val.includes('Biti')) {
                if(techDetails) techDetails.style.display = 'none';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Blok Var?';
                }
            } else if (val.includes('Ortak Baza')) {
                if(techDetails) techDetails.style.display = 'flex';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Kule Var?';
                }
            } else {
                if(techDetails) techDetails.style.display = 'none';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            }
            
            var inputElem = subBlocksContainer ? subBlocksContainer.querySelector('input[type="number"]') : null;
            // DİKKAT: Sayfa yüklenirken zaten HTML MVC tarafından basılıyor, JS ile tekrar basmamak için skipSave kontrolü yapalım
            if (!skipSave && inputElem && parseInt(inputElem.value) > 0) {
                var isOld = container.closest('.block-item-old') !== null;
                var prefix = isOld ? 'ExistingBlocks' : 'TargetBlocks';
                var nameAttr = selectElem.getAttribute('name'); 
                var parentIndexMatch = nameAttr.match(/\[(\d+)\]/);
                var parentIndex = parentIndexMatch ? parentIndexMatch[1] : 0;
                generateSubBlocks(inputElem, prefix, parentIndex);
            }

            if (!skipSave && typeof autoSaveStep2 === 'function') autoSaveStep2();
        }"""

def replace_func(match):
    return new_js

content = re.sub(r'function handleLayoutPatternChange\(selectElem\).*?(?=function generateSubBlocks)', replace_func, content, flags=re.DOTALL)

# Add DOMContentLoaded to initialize all dropdowns!
init_script = r"""
        document.addEventListener('DOMContentLoaded', function() {
            var selects = document.querySelectorAll('select[name$=".LayoutPattern"]');
            selects.forEach(function(s) {
                handleLayoutPatternChange(s, true);
            });
        });
"""
if "handleLayoutPatternChange(s, true)" not in content:
    content = content.replace("function nextStep(step) {", init_script + "\n        function nextStep(step) {")

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed initial visibility!")
