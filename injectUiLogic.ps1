$content = Get-Content -Raw "GMK360.Web\Views\ConstructionProject\Create.cshtml"

$newRow = @'
<div class="row g-3">
    <div class="col-md-12 col-lg-3">
        <label class="form-label small fw-bold">Blok / Yapı Adı</label>
        <input type="text" name="Blocks[${i}].BlockName" class="form-control b-name" value="${defaultName}" onkeyup="updatePodiumDropdowns()" required ${safeCount === 1 ? 'readonly' : ''} />
        <label class="form-label small fw-bold mt-2 text-primary">Yapı Karakteri</label>
        <select name="Blocks[${i}].StructureType" class="form-select b-type border-primary" onchange="toggleParent(this)">
            <option value="independent">Müstakil (Kendi Temeli)</option>
            <option value="podium">Ortak Baza (Alt Yapı / Otopark)</option>
            <option value="tower">Baza Üzerinde Kule</option>
        </select>
        <select name="Blocks[${i}].ParentIndex" class="form-select b-parent mt-2 border-warning" style="display:none;">
            <option value="">Hangi Bazaya Bağlı?</option>
        </select>
    </div>
    <div class="col-12 col-lg-2">
        <label class="form-label small fw-bold">Taban (m²)</label>
        <input type="number" name="Blocks[${i}].BaseArea" class="form-control" placeholder="Örn: 200" min="1" required />
    </div>
    <div class="col-6 col-lg-2">
        <label class="form-label small fw-bold">Normal Kat</label>
        <input type="number" name="Blocks[${i}].TotalFloors" class="form-control b-floors" value="5" min="0" required />
    </div>
    <div class="col-6 col-lg-2">
        <label class="form-label small fw-bold">Bodrum</label>
        <input type="number" name="Blocks[${i}].BasementFloors" class="form-control b-basements" value="1" min="0" required />
        
        <div class="form-check mt-2">
            <input class="form-check-input" type="checkbox" name="Blocks[${i}].HasGroundFloor" value="true" id="ground_${i}" checked>
            <label class="form-check-label small fw-bold" for="ground_${i}">Zemin Var</label>
        </div>
        <div class="form-check">
            <input class="form-check-input" type="checkbox" name="Blocks[${i}].HasRoof" value="true" id="roof_${i}">
            <label class="form-check-label small fw-bold" for="roof_${i}">Çatı Var</label>
        </div>
    </div>
</div>
'@

# Oh wait! In @'', ${i} will not be evaluated, which is correct for JS template! But I need to replace the old row.
# Since my previous script corrupted the file with empty strings where $(this) was, I better use C# to cleanly rewrite it!
