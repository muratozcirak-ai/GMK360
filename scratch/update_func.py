import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

new_func = '''function handleLayoutPatternChange(selectElem) {
            var val = selectElem.value;
            var container = selectElem.closest('.card-body'); 
            var techDetails = container.querySelector('.tech-details');
            
            // Eğer Tekil Yapı ise 2. Satırı (Kat vb.) göster, değilse şimdilik gizle (Kule alt-formu mantığı gelene kadar)
            if (val === 'Tekil Yapı' || val === 'Bağımsız Temel') {
                if(techDetails) techDetails.style.display = 'flex';
            } else {
                if(techDetails) techDetails.style.display = 'none';
            }
            
            var cbRoof = container.querySelector('input[name$=".HasRoof"]');
            var cbGround = container.querySelector('input[name$=".HasGroundFloor"]');
            var inputBasement = container.querySelector('input[name$=".BasementFloors"]');
            
            if (val === 'Ortak Baza') {
                if(cbRoof) cbRoof.checked = false; 
                if(cbGround) cbGround.checked = true;
            } else if (val === 'Kule') {
                if(cbGround) cbGround.checked = false; 
                if(inputBasement) inputBasement.value = 0; 
                if(cbRoof) cbRoof.checked = true;
            } else {
                if(cbRoof) cbRoof.checked = true;
                if(cbGround) cbGround.checked = true;
            }
        }'''

# Replace old func
content = re.sub(r'function handleLayoutPatternChange\(selectElem\) \{.*?(?=\n\n\s*function generateNewBlocks)', new_func, content, flags=re.DOTALL)

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Function updated!")
