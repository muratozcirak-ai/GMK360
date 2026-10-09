with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re
content = re.sub(r'var invalidElements = step\.querySelectorAll\(\':invalid\'\);.*?return;\s*\}', '', content, flags=re.DOTALL)

new_js = r"""function handleLayoutPatternChange(selectElem, skipSave) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var cardHeader = container.closest('.card').querySelector('.card-header h5, .card-body > h5');
            var techDetails = container.querySelector('.tech-details');
            var subBlocksContainer = container.querySelector('.sub-blocks-container');
            
            var headerText = cardHeader ? cardHeader.innerText : "";
            var numberPrefixMatch = headerText.match(/^\d+\./);
            var numberPrefix = numberPrefixMatch ? numberPrefixMatch[0] : "";
            var isOld = container.closest('.block-item-old') !== null;
            var defaultBaseName = isOld ? "Eski Yapı" : "Yeni Yapı";

            function setTechDetailsVisible(isVisible) {
                if(techDetails) {
                    techDetails.style.display = isVisible ? 'flex' : 'none';
                    var inputs = techDetails.querySelectorAll('input');
                    inputs.forEach(inp => {
                        inp.removeAttribute('required');
                    });
                }
            }

            if (val.includes('Tekil') || val.includes('Bağımsız')) {
                setTechDetailsVisible(true);
                if(cardHeader) cardHeader.innerHTML = '<i class="ph ph-building me-2"></i> ' + numberPrefix + ' ' + defaultBaseName + ' <span class="badge bg-secondary ms-2">Tekil Yapı</span>';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            } else if (val.includes('Biti')) {
                setTechDetailsVisible(false);
                if(cardHeader) cardHeader.innerHTML = '<i class="ph ph-buildings me-2"></i> ' + numberPrefix + ' Bitişik Nizam Tabanı';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Blok Var?';
                }
            } else if (val.includes('Ortak Baza')) {
                setTechDetailsVisible(true);
                if(cardHeader) cardHeader.innerHTML = '<i class="ph ph-intersect me-2"></i> ' + numberPrefix + ' Ortak Baza';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Kule Var?';
                }
            } else {
                setTechDetailsVisible(false);
                if(cardHeader) cardHeader.innerHTML = '<i class="ph ph-building me-2"></i> ' + numberPrefix + ' ' + defaultBaseName;
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            }
            
            var inputElem = subBlocksContainer ? subBlocksContainer.querySelector('input[type="number"]') : null;
            if (!skipSave && inputElem && parseInt(inputElem.value) > 0) {
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

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Removed native validation completely!")
