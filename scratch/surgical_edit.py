import re

with open(r'GMK360.Web\Views\ConstructionProject\Create.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Hide Eski Binalar for Greenfield
content = content.replace(
    '''<!-- ESKİ BİNA BÖLÜMÜ -->
                        <div id="kentselDonusumOldBlocks"''',
    '''<!-- ESKİ BİNA BÖLÜMÜ (Sadece Kentsel Dönüşüm) -->
                        <div id="kentselDonusumOldBlocks" style="display: @(Model.ProjectOriginId == 1 ? "block" : "none");"'''
)

# 2. Update Labels
content = content.replace('Yıkılacak Mevcut Blok (Bina) Sayısı', 'Yıkılacak Mevcut Yapı (Fiziksel Kütle) Sayısı')
content = content.replace('1. Eski Blok / Yapı', '1. Eski Yapı')
content = content.replace('2. Eski Blok / Yapı', '2. Eski Yapı')
content = content.replace('Yapılacak Yeni Blok Sayısı', 'Projedeki Yeni "Yapı" (Farklı Temel/Kütle) Sayısı')
content = content.replace('1. Yeni Blok / Yapı', '1. Yeni Yapı / Kütle')
content = content.replace('2. Yeni Blok / Yapı', '2. Yeni Yapı / Kütle')
content = content.replace('Yeni Blok Adı', 'Yapı / Blok Adı')
content = content.replace('Tek Blok', 'Tek Yapı')
content = content.replace('Blok / Aşama Dağılımı', 'Yapı / Aşama Dağılımı')

# 3. Update LayoutPattern options in Razor loop
razor_pattern = r'<select name="TargetBlocks\[@i\]\.LayoutPattern"[^>]*>.*?<\/select>'
new_razor_select = '''<select name="TargetBlocks[@i].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                                            <option value="" selected="@(string.IsNullOrEmpty(b.LayoutPattern))">Mimari Tarz (Seçiniz)</option>
                                                            <option value="Tekil Yapı" selected="@(b.LayoutPattern == "Tekil Yapı")">Tekil Yapı (Standart)</option>
                                                            <option value="Bitişik Nizam" selected="@(b.LayoutPattern == "Bitişik Nizam")">Bitişik Nizam (Tevhit)</option>
                                                            <option value="Ortak Baza" selected="@(b.LayoutPattern == "Ortak Baza")">Ortak Baza (Kule Altı)</option>
                                                            <option value="Kule" selected="@(b.LayoutPattern == "Kule")">Kule (Baza Üstü)</option>
                                                        </select>'''
content = re.sub(razor_pattern, new_razor_select, content, flags=re.DOTALL)

# 4. Update LayoutPattern options in JS template
js_pattern = r'<select name="TargetBlocks\[\$\{i\}\]\.LayoutPattern"[^>]*>.*?<\/select>'
new_js_select = '''<select name="TargetBlocks[].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                        <option value="" selected>Mimari Tarz (Seçiniz)</option>
                                        <option value="Tekil Yapı">Tekil Yapı (Standart)</option>
                                        <option value="Bitişik Nizam">Bitişik Nizam (Tevhit)</option>
                                        <option value="Ortak Baza">Ortak Baza (Kule Altı)</option>
                                        <option value="Kule">Kule (Baza Üstü)</option>
                                    </select>'''
content = re.sub(js_pattern, new_js_select, content, flags=re.DOTALL)


# 5. Inject JS helper function for auto-checking boxes based on LayoutPattern
js_helper = '''
        function handleLayoutPatternChange(selectElem) {
            var val = selectElem.value;
            var container = selectElem.closest('.row'); // find the parent row of the fields
            
            var cbRoof = container.querySelector('input[name$=".HasRoof"]');
            var cbGround = container.querySelector('input[name$=".HasGroundFloor"]');
            var inputBasement = container.querySelector('input[name$=".BasementFloors"]');
            
            if (val === 'Ortak Baza') {
                if(cbRoof) cbRoof.checked = false; // Baza has no roof
                if(cbGround) cbGround.checked = true;
            } else if (val === 'Kule') {
                if(cbGround) cbGround.checked = false; // Kule starts above baza, no ground floor
                if(inputBasement) inputBasement.value = 0; // Kule has no basement
                if(cbRoof) cbRoof.checked = true;
            } else {
                // Default resets
                if(cbRoof) cbRoof.checked = true;
                if(cbGround) cbGround.checked = true;
            }
        }
'''

idx_ready = content.find('function generateNewBlocks')
if idx_ready != -1:
    content = content[:idx_ready] + js_helper + '\n' + content[idx_ready:]

with open(r'GMK360.Web\Views\ConstructionProject\Create.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Surgical UI edit complete.")
