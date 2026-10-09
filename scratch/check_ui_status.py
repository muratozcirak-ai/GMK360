import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update handleLayoutPatternChange
new_func = '''function handleLayoutPatternChange(selectElem) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var techDetails = container.querySelector('.tech-details');
            var subBlocksContainer = container.querySelector('.sub-blocks-container');
            
            if (val === 'Tekil Yapı' || val === 'Bağımsız Temel') {
                if(techDetails) techDetails.style.display = 'flex';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            } else if (val === 'Ortak Baza' || val === 'Bitişik Nizam') {
                if(techDetails) techDetails.style.display = 'none';
                if(subBlocksContainer) {
                    subBlocksContainer.style.display = 'block';
                    var label = subBlocksContainer.querySelector('.sub-block-label');
                    if(label) label.innerText = val === 'Ortak Baza' ? 'Kaç Kule Var?' : 'Kaç Blok Var?';
                }
            } else {
                if(techDetails) techDetails.style.display = 'none';
                if(subBlocksContainer) subBlocksContainer.style.display = 'none';
            }
            if (typeof autoSaveStep2 === 'function') autoSaveStep2();
        }
        
        function generateSubBlocks(inputElem, prefix, parentIndex) {
            const count = parseInt(inputElem.value) || 0;
            const container = inputElem.closest('.sub-blocks-container').querySelector('.sub-blocks-list');
            container.innerHTML = ''; 
            const letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            
            const layoutPattern = inputElem.closest('.card-body').querySelector('select[name$=".LayoutPattern"]').value;
            const isKule = layoutPattern === 'Ortak Baza';
            const labelName = isKule ? 'Kule' : 'Blok';
            
            for (let j = 0; j < count; j++) {
                const defaultName = letters[j] + " " + labelName;
                const namePrefix = prefix + "[" + parentIndex + "].SubBlocks[" + j + "]";
                
                const html = 
                <div class="card border border-secondary border-opacity-25 shadow-sm rounded-4 mt-3 bg-light">
                    <div class="card-body p-3">
                        <h6 class="fw-bold text-secondary mb-3"><i class="ph ph-buildings me-2"></i> . </h6>
                        <div class="row g-2">
                            <div class="col-6 col-lg-3">
                                <label class="form-label small fw-bold">Adı</label>
                                <input type="text" name=".BlockName" class="form-control form-control-sm" value="" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Kat Sayısı</label>
                                <input type="number" name=".TotalFloors" class="form-control form-control-sm" min="1" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Daire</label>
                                <input type="number" name=".TotalApartments" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-2">
                                <label class="form-label small fw-bold">Dükkan</label>
                                <input type="number" name=".TotalShops" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-3">
                                <label class="form-label small fw-bold">Bodrum Kat</label>
                                <input type="number" name=".BasementFloors" class="form-control form-control-sm" value="0" required />
                            </div>
                            <div class="col-6 col-lg-2 mt-3">
                                <label class="form-label small fw-bold">Zemin Kat</label>
                                <div class="form-check form-switch">
                                    <input class="form-check-input" type="checkbox" name=".HasGroundFloor" value="true" checked>
                                </div>
                            </div>
                            <div class="col-6 col-lg-2 mt-3">
                                <label class="form-label small fw-bold">Çatı Katı</label>
                                <div class="form-check form-switch">
                                    <input class="form-check-input" type="checkbox" name=".HasRoof" value="true" checked>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                ;
                container.insertAdjacentHTML('beforeend', html);
            }
            if (typeof autoSaveStep2 === 'function') autoSaveStep2();
        }'''

# Replace old func
content = re.sub(r'function handleLayoutPatternChange\(selectElem\) \{.*?(?=\n\n\s*function generateNewBlocks)', new_func, content, flags=re.DOTALL)

# Now carefully insert the sub-blocks-container right AFTER the 	ech-details div but INSIDE the card-body.
# To do this safely, we will find <!-- SATIR 2: TEKNİK DETAYLAR (ŞİMDİLİK GİZLİ) --> block, skip to its closing </div>, and insert.
# But wait, we didn't insert the SATIR 2 comments into JS string yet, only Razor!
# Actually, the last time I tried to replace the UI layout, it failed and I reverted!
# So currently the UI layout is STILL the old one! (Row 1: 6 columns, Row 2: 4 columns and Baza Durumu).
