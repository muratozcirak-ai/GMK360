with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# Change all min="1" to min="0" except for BaseArea and TotalLandArea
content = re.sub(r'(name=".*?(TotalFloors)".*?)min="1"', r'\1min="0"', content)
content = re.sub(r'id="newBlockCount".*?min="1"', r'id="newBlockCount" class="form-control form-control-lg rounded-3 border-primary border-opacity-50" min="0"', content)

# Update handleLayoutPatternChange to manage 'required' and 'disabled'
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
                            // We do NOT disable them, because we still want them posted as 0 or empty 
                            // to override previous values. 
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

content = re.sub(r'function handleLayoutPatternChange\(selectElem, skipSave\) \{.*?(?=function generateSubBlocks)', new_js + "\n\n        ", content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed required attributes and min=1!")
