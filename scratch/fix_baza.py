with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

new_js = r"""function handleLayoutPatternChange(selectElem) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var techDetails = container.querySelector('.tech-details');
            var subBlocksContainer = container.querySelector('.sub-blocks-container');
            
            if (val.includes('Tekil') || val.includes('Bağımsız')) {
                if(techDetails) techDetails.style.display = 'flex';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            } else if (val.includes('Biti')) {
                // Bitişik Nizam: Ana kartın teknik detayları GİZLENECEK
                if(techDetails) techDetails.style.display = 'none';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = 'Kaç Blok Var?';
                }
            } else if (val.includes('Ortak Baza')) {
                // Ortak Baza: Ana kartın teknik detayları AÇIK KALACAK
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
            
            // Eğer varsa sub-blocks'ları tekrar oluştur (isimleri güncellemek için)
            var inputElem = subBlocksContainer ? subBlocksContainer.querySelector('input[type="number"]') : null;
            if (inputElem && parseInt(inputElem.value) > 0) {
                var isOld = container.closest('.block-item-old') !== null;
                var prefix = isOld ? 'ExistingBlocks' : 'TargetBlocks';
                var nameAttr = selectElem.getAttribute('name'); // e.g. ExistingBlocks[0].LayoutPattern
                var parentIndexMatch = nameAttr.match(/\[(\d+)\]/);
                var parentIndex = parentIndexMatch ? parentIndexMatch[1] : 0;
                generateSubBlocks(inputElem, prefix, parentIndex);
            }

            if (typeof autoSaveStep2 === 'function') autoSaveStep2();
        }
        
        function generateSubBlocks(inputElem, prefix, parentIndex) {
            const count = parseInt(inputElem.value) || 0;
            const container = inputElem.closest('.sub-blocks-container').querySelector('.sub-blocks-list');
            container.innerHTML = ''; 
            const letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            
            const layoutPattern = inputElem.closest('.card-body').querySelector('select[name$=".LayoutPattern"]').value;
            const isKule = layoutPattern.includes('Ortak Baza');
            const labelName = isKule ? 'Kule' : 'Blok';
            
            for (let j = 0; j < count; j++) {
                const defaultName = letters[j] + " " + labelName;
                const namePrefix = prefix + "[" + parentIndex + "].SubBlocks[" + j + "]";
                
                const html = `
                <div class="card border border-secondary border-opacity-25 shadow-sm rounded-4 mt-3 bg-light">
                    <div class="card-body p-3">
                        <h6 class="fw-bold text-secondary mb-3"><i class="ph ph-buildings me-2"></i> ${j+1}. ${labelName}</h6>
                        <div class="row g-2">
                            <div class="col-6 col-lg-3">
                                <label class="form-label small fw-bold">Adı</label>
                                <input type="text" name="${namePrefix}.BlockName" class="form-control form-control-sm" value="${defaultName}" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Kat Sayısı</label>
                                <input type="number" name="${namePrefix}.TotalFloors" class="form-control form-control-sm" min="1" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Daire</label>
                                <input type="number" name="${namePrefix}.TotalApartments" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Dükkan</label>
                                <input type="number" name="${namePrefix}.TotalShops" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-3">
                                <label class="form-label small fw-bold">Bodrum Kat</label>
                                <input type="number" name="${namePrefix}.BasementFloors" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-2 mt-3">
                                <label class="form-label small fw-bold">Zemin Kat</label>
                                <div class="form-check form-switch">
                                    <input class="form-check-input" type="checkbox" name="${namePrefix}.HasGroundFloor" value="true" checked>
                                </div>
                            </div>
                            <div class="col-6 col-lg-2 mt-3">
                                <label class="form-label small fw-bold">Çatı Katı</label>
                                <div class="form-check form-switch">
                                    <input class="form-check-input" type="checkbox" name="${namePrefix}.HasRoof" value="true" checked>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                `;
                container.insertAdjacentHTML('beforeend', html);
            }
            if (typeof autoSaveStep2 === 'function') autoSaveStep2();
        }"""

def replace_func(match):
    return new_js + '\n\n        '

content = re.sub(r'function handleLayoutPatternChange\(selectElem\) \{.*?(?=function generateNewBlocks)', replace_func, content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated JS functions safely.")
