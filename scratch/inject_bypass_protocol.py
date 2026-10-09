import codecs
import re
import glob

# Mapping phases to their BudgetPhaseCategory
phase_map = {
    'PhaseOne': 'YikimVeZeminHazirligi',
    'PhaseTwo': 'KabaInsaat',
    'PhaseThree': 'CatıVeYalitim',
    'PhaseFour': 'InceInsaatVeTesisat',
    'PhaseFive': 'DisCepheVePencere',
    'PhaseSix': 'ZeminVeIcMekan',
    'PhaseSeven': 'PeyzajVeCevre',
    'PhaseEight': 'TestVeTeslim'
}

ctrls = glob.glob('GMK360.Web/Controllers/Phase*Controller.cs')
for path in ctrls:
    if 'PhaseZero' in path: continue
    
    # identify phase category
    phase_name = ""
    for k in phase_map.keys():
        if k in path: phase_name = k

    if not phase_name: continue
    
    enum_name = phase_map[phase_name]

    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Inject LinkedDocs
    if 'ViewBag.LinkedDocs' not in content:
        content = content.replace(
            'ViewData["BypassedPhases"] = project.BypassedPhases;',
            f'ViewData["BypassedPhases"] = project.BypassedPhases;\n            ViewBag.LinkedDocs = await _context.ProjectLegalDocuments.Where(d => d.ConstructionProjectId == projectId && d.LinkedPhaseCategory == GMK360.Core.Entities.Construction.BudgetPhaseCategory.{enum_name}).ToListAsync();\n            ViewBag.PhaseId = (int)GMK360.Core.Entities.Construction.BudgetPhaseCategory.{enum_name};'
        )
        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Updated Controller {path}")

# Now update the Views
views = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in views:
    if 'PhaseZero' in path: continue

    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # The HTML to inject: The Bypass Modal and the Lock logic
    lock_logic = '''
@{
    var linkedDocs = ViewBag.LinkedDocs as IEnumerable<GMK360.Core.Entities.Construction.ProjectLegalDocument>;
    bool isHardLocked = false;
    bool isBypassed = false;
    int currentPhaseId = ViewBag.PhaseId != null ? (int)ViewBag.PhaseId : 0;
    string bpStr = ViewData["BypassedPhases"]?.ToString() ?? "";
    
    if (linkedDocs != null && linkedDocs.Any(d => d.Status != "Onaylandı"))
    {
        isHardLocked = true;
    }
    
    if (bpStr.Split(',').Contains(currentPhaseId.ToString()))
    {
        isBypassed = true;
        isHardLocked = false; // Bypassed
    }
}

<!-- BYPASS MODAL -->
<div class="modal fade" id="bypassModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg rounded-4">
            <div class="modal-header bg-danger text-white rounded-top-4 border-0 p-4">
                <h5 class="modal-title fw-bold"><i class="bi bi-shield-exclamation me-2"></i> Kademeli Yetki (Bypass) Protokolü</h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
            </div>
            <form action="/ConstructionProject/BypassPhase" method="post">
                <div class="modal-body p-4 bg-light">
                    <input type="hidden" name="projectId" value="@ViewData["ProjectId"]" />
                    <input type="hidden" name="phaseId" value="@currentPhaseId" />
                    <input type="hidden" name="redirectUrl" value="@Context.Request.Path" />
                    
                    <p class="text-danger fw-bold mb-3"><i class="bi bi-exclamation-triangle-fill me-2"></i> UYARI: Bu faz için gerekli ön koşul (Faz 0) belgeleri henüz MÜHENDİSLİK/MERKEZ OFİS tarafından ONAYLANMAMIŞTIR.</p>
                    <p class="text-dark small mb-4">Patron/Merkez inisiyatifi ile, sahadaki acil durumlar için geçici olarak işleme devam etme yetkiniz bulunmaktadır. Ancak bu işlemi yaptığınız an, sistem arka planda bu işlemi Kırmızı Log (Riskli İşlem) olarak kaydedecek ve Merkez Ofise Anlık Bildirim (Push Notification) gönderecektir.</p>
                    
                    <div class="form-check border border-danger rounded-3 p-3 bg-white">
                        <input class="form-check-input ms-1" type="checkbox" id="liabilityCheck" required>
                        <label class="form-check-label ms-2 fw-semibold text-dark" for="liabilityCheck">
                            İlgili ön koşul evrak sürecinin tamamlanmadığını biliyorum. İmalata başlama kararı ve yasal sorumluluk tamamen benim (Şantiye Şefi) kontrolümdedir.
                        </label>
                    </div>
                </div>
                <div class="modal-footer border-0 p-3 bg-light rounded-bottom-4">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-danger fw-bold px-4"><i class="bi bi-unlock-fill me-2"></i> Kilidi Kaldır ve Başla</button>
                </div>
            </form>
        </div>
    </div>
</div>
'''

    # We inject the lock_logic at the top of the file, right after <div class="container-fluid mt-2"> or layout
    if 'bool isHardLocked' not in content:
        content = re.sub(r'(<div class="container-fluid mt-2">)', r'\1\n' + lock_logic, content)
        
        # Replace the yellow box rendering
        new_yellow_box = '''
    @if (linkedDocs != null && linkedDocs.Any())
    {
        <div class="alert @(isHardLocked ? "alert-danger border-danger" : (isBypassed ? "alert-primary border-primary" : "alert-success border-success")) shadow-sm mb-4 rounded-4">
            <h5 class="fw-bold text-dark mb-3">
                <i class="bi @(isHardLocked ? "bi-lock-fill text-danger" : (isBypassed ? "bi-unlock-fill text-primary" : "bi-check-circle-fill text-success")) me-2 fs-4"></i> 
                @(isHardLocked ? "KİLİTLİ: Bu Aşamaya Bağlı Ön Koşul Evrakları (Faz 0) Eksik!" : (isBypassed ? "BYPASS EDİLDİ: Bu aşama yasal sorumluluk alınarak açıldı." : "ONAYLI: Tüm Faz 0 ön koşul evrakları tam."))
            </h5>
            <div class="list-group shadow-sm rounded-3 mb-3">
                @foreach(var doc in linkedDocs)
                {
                    <a href="/PhaseZero/Index/@ViewBag.ProjectId" class="list-group-item list-group-item-action @(doc.Status == "Onaylandı" ? "list-group-item-success" : "list-group-item-danger") d-flex justify-content-between align-items-center">
                        <div>
                            <i class="bi bi-file-earmark-text me-2"></i> <span class="fw-bold text-dark fs-6">@doc.DocumentName</span>
                        </div>
                        <span class="badge @(doc.Status == "Onaylandı" ? "bg-success" : "bg-danger") rounded-pill fs-6 px-3">@doc.Status</span>
                    </a>
                }
            </div>
            
            @if(isHardLocked)
            {
                <div class="d-flex justify-content-between align-items-center">
                    <small class="text-danger fw-bold"><i class="bi bi-info-circle-fill me-1"></i> SİSTEM KİLİDİ: Evraklar tamamlanmadan yeni işlem yapılamaz.</small>
                    <button class="btn btn-sm btn-outline-danger fw-bold" data-bs-toggle="modal" data-bs-target="#bypassModal"><i class="bi bi-key-fill me-1"></i> İnisiyatif Kullan (Kilidi Kaldır)</button>
                </div>
            }
        </div>
    }
'''
        # We need to replace the old yellow box alert with the new one
        # The old yellow box starts with @if (ViewBag.LinkedDocs != null and ends with }
        content = re.sub(
            r'(@if \(ViewBag\.LinkedDocs != null.*?)(<div class="card border-0 shadow-sm rounded-4">)',
            new_yellow_box + r'\n\n    \2',
            content, flags=re.DOTALL
        )

        # Apply HardLock to buttons!
        # Search for: class="btn btn-primary" and other buttons inside the header and add disabled if isHardLocked
        content = content.replace(
            '<button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addModal">',
            '<button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addModal" @(isHardLocked ? "disabled" : "")>'
        )
        content = content.replace(
            '<button type="submit" class="btn btn-secondary">',
            '<button type="submit" class="btn btn-secondary" @(isHardLocked ? "disabled" : "")>'
        )
        content = content.replace(
            '<a href="/B2BPurchasing/ProjectOverview/@ViewData["ProjectId"]" class="btn btn-warning text-dark fw-bold">',
            '<a href="/B2BPurchasing/ProjectOverview/@ViewData["ProjectId"]" class="btn btn-warning text-dark fw-bold @(isHardLocked ? "disabled" : "")">'
        )
        # Disable action buttons in the list
        content = content.replace(
            'class="btn btn-sm btn-outline-primary" onclick="openManageModal',
            'class="btn btn-sm btn-outline-primary @(isHardLocked ? "disabled" : "")" onclick="openManageModal'
        )

        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
        print(f"Updated View {path}")