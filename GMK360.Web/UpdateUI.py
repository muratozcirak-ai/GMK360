import re

with open(r"Views\ConstructionProject\Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# Replace HTML for existing blocks (C# Razor part)
find_html1 = """                                          <label class="form-label small fw-bold mt-2 text-primary">Yapı Karakteri</label>
                                          <select name="Blocks[@i].StructureType" class="form-select b-type border-primary" onchange="toggleParent(this)">
                                              <option value="independent" @(b.StructureType == "independent" ? "selected" : "")>Müstakil (Kendi Temeli)</option>
                                              <option value="podium" @(b.StructureType == "podium" ? "selected" : "")>Ortak Baza (Alt Yapı / Otopark)</option>
                                              <option value="tower" @(b.StructureType == "tower" ? "selected" : "")>Baza Üzerinde Kule</option>
                                          </select>
                                          
                                          <select name="Blocks[@i].ParentIndex" class="form-select b-parent mt-2 border-warning" style="@(b.StructureType == "tower" ? "" : "display:none;")">
                                              <option value="">Hangi Bazaya Bağlı?</option>
                                          </select>"""

replace_html1 = """                                          <label class="form-label small fw-bold mt-2 text-primary">Yapı Karakteri</label>
                                          <select name="Blocks[@i].StructureType" class="form-select b-type border-primary" onchange="toggleParent(this)">
                                              <option value="independent" @(b.StructureType == "independent" ? "selected" : "")>Müstakil (Kendi Temeli)</option>
                                              <option value="podium" @(b.StructureType == "podium" ? "selected" : "")>Ortak Baza (Alt Yapı / Otopark)</option>
                                              <option value="tower" @(b.StructureType == "tower" ? "selected" : "")>Baza Üzerinde Kule</option>
                                          </select>
                                          <select name="Blocks[@i].ParentIndex" class="form-select b-parent mt-2 border-warning" style="@(b.StructureType == "tower" ? "" : "display:none;")">
                                              <option value="">Hangi Bazaya Bağlı?</option>
                                          </select>

                                          <label class="form-label small fw-bold mt-2 text-info">Blok Nizamı (Yerleşim)</label>
                                          <select name="Blocks[@i].LayoutPattern" class="form-select b-layout border-info" onchange="toggleLayout(this)">
                                              <option value="Ayrık" @(b.LayoutPattern == "Ayrık" ? "selected" : "")>Ayrık Nizam</option>
                                              <option value="Bitişik" @(b.LayoutPattern == "Bitişik" ? "selected" : "")>Bitişik Nizam</option>
                                              <option value="İkiz" @(b.LayoutPattern == "İkiz" ? "selected" : "")>İkiz Nizam</option>
                                              <option value="Blok" @(b.LayoutPattern == "Blok" ? "selected" : "")>Blok Nizam</option>
                                          </select>
                                          
                                          <select name="Blocks[@i].AttachedToBlock" class="form-select b-attached mt-2 border-danger" style="@(b.LayoutPattern == "Bitişik" || b.LayoutPattern == "İkiz" ? "" : "display:none;")">
                                              <option value="">Hangi Bloğa Bitişik?</option>
                                              <option value="@b.AttachedToBlock" selected>@b.AttachedToBlock</option>
                                          </select>"""
content = content.replace(find_html1, replace_html1)

# Replace HTML for dynamically added blocks (JS part)
find_html2 = """                                          <label class="form-label small fw-bold mt-2 text-primary">Yapı Karakteri</label>
                                          <select name="Blocks[${i}].StructureType" class="form-select b-type border-primary" onchange="toggleParent(this)">
                                              <option value="independent">Müstakil (Kendi Temeli)</option>
                                              <option value="podium">Ortak Baza (Alt Yapı / Otopark)</option>
                                              <option value="tower">Baza Üzerinde Kule</option>
                                          </select>
                                          
                                          <select name="Blocks[${i}].ParentIndex" class="form-select b-parent mt-2 border-warning" style="display:none;">
                                              <option value="">Hangi Bazaya Bağlı?</option>
                                          </select>"""
                                          
replace_html2 = """                                          <label class="form-label small fw-bold mt-2 text-primary">Yapı Karakteri</label>
                                          <select name="Blocks[${i}].StructureType" class="form-select b-type border-primary" onchange="toggleParent(this)">
                                              <option value="independent">Müstakil (Kendi Temeli)</option>
                                              <option value="podium">Ortak Baza (Alt Yapı / Otopark)</option>
                                              <option value="tower">Baza Üzerinde Kule</option>
                                          </select>
                                          <select name="Blocks[${i}].ParentIndex" class="form-select b-parent mt-2 border-warning" style="display:none;">
                                              <option value="">Hangi Bazaya Bağlı?</option>
                                          </select>

                                          <label class="form-label small fw-bold mt-2 text-info">Blok Nizamı (Yerleşim)</label>
                                          <select name="Blocks[${i}].LayoutPattern" class="form-select b-layout border-info" onchange="toggleLayout(this)">
                                              <option value="Ayrık">Ayrık Nizam</option>
                                              <option value="Bitişik">Bitişik Nizam</option>
                                              <option value="İkiz">İkiz Nizam</option>
                                              <option value="Blok">Blok Nizam</option>
                                          </select>
                                          
                                          <select name="Blocks[${i}].AttachedToBlock" class="form-select b-attached mt-2 border-danger" style="display:none;">
                                              <option value="">Hangi Bloğa Bitişik?</option>
                                          </select>"""
content = content.replace(find_html2, replace_html2)


# Add toggleLayout function and update updatePodiumDropdowns logic
find_script = """        function toggleParent(selectElement) {"""
replace_script = """        function toggleLayout(selectElement) {
            const blockItem = selectElement.closest('.block-item');
            const attachedSelect = blockItem.querySelector('.b-attached');
            if (selectElement.value === 'Bitişik' || selectElement.value === 'İkiz') {
                attachedSelect.style.display = 'block';
                updateAttachedDropdowns();
            } else {
                attachedSelect.style.display = 'none';
                attachedSelect.value = '';
            }
        }

        function updateAttachedDropdowns() {
            const blockItems = document.querySelectorAll('#blocksContainer .block-item');
            const blockNames = [];
            
            blockItems.forEach(item => {
                const nameInput = item.querySelector('.b-name');
                if (nameInput && nameInput.value.trim() !== '') {
                    blockNames.push(nameInput.value.trim());
                }
            });

            blockItems.forEach(item => {
                const attachedSelect = item.querySelector('.b-attached');
                if (attachedSelect) {
                    const currentValue = attachedSelect.value;
                    attachedSelect.innerHTML = '<option value="">Hangi Bloğa Bitişik?</option>';
                    blockNames.forEach(name => {
                        const myName = item.querySelector('.b-name').value.trim();
                        // Kendi ismini göstermesin
                        if(name !== myName) {
                            attachedSelect.innerHTML += `<option value="${name}">${name}</option>`;
                        }
                    });
                    if (currentValue && blockNames.includes(currentValue)) {
                        attachedSelect.value = currentValue;
                    }
                }
            });
        }

        function toggleParent(selectElement) {"""
content = content.replace(find_script, replace_script)


# Hook updateAttachedDropdowns to onkeyup of block names
find_onkeyup1 = """onkeyup="updatePodiumDropdowns()" required"""
replace_onkeyup1 = """onkeyup="updatePodiumDropdowns(); updateAttachedDropdowns();" required"""
content = content.replace(find_onkeyup1, replace_onkeyup1)

with open(r"Views\ConstructionProject\Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated Create.cshtml with Bitişik Nizam logic.")
