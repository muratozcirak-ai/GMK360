with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

content = re.sub(r'(name=".*?(TotalFloors)".*?)min="1"', r'\1min="0"', content)
content = re.sub(r'(name=".*?(TotalApartments|TotalShops|BasementFloors)".*?)required', r'\1', content) # Remove required from all of them inside sub-blocks too? No, just let the JS handle it.

new_js = r"""function handleLayoutPatternChange(selectElem, skipSave) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var techDetails = container.querySelector('.tech-details');
            var subBlocksContainer = container.querySelector('.sub-blocks-container');
            
            function setTechDetailsVisible(isVisible) {
                if(techDetails) {
                    techDetails.style.display = isVisible ? 'flex' : 'none';
                    var inputs = techDetails.querySelectorAll('input');
                    inputs.forEach(inp => {
                        if(!isVisible) {
                            inp.removeAttribute('required');
                        } else {
                            inp.setAttribute('required', 'required');
                        }
                    });
                }
            }

            if (val.includes('Tekil') || val.includes('Bağımsız')) {
                setTechDetailsVisible(true);
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            } else if (val.includes('Biti')) {
                setTechDetailsVisible(false);
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Blok Var?';
                }
            } else if (val.includes('Ortak Baza')) {
                setTechDetailsVisible(true);
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Kule Var?';
                }
            } else {
                setTechDetailsVisible(false);
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            }
            
            var inputElem = subBlocksContainer ? subBlocksContainer.querySelector('input[type="number"]') : null;
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
    return new_js + "\n\n        "

content = re.sub(r'function handleLayoutPatternChange\(selectElem, skipSave\) \{.*?(?=function generateSubBlocks)', replace_func, content, flags=re.DOTALL)

# In JS generateSubBlocks, remove min="1" and required where appropriate
content = content.replace('min="1" required />', 'min="0" />')
content = content.replace('required />', '/>')

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed validation successfully!")
