import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# We will inject the new card right after the Phase Zero link card (Faz 0 Evrakları).
# Search for the "PhaseZero" link or the first row of cards.
# Let's insert a new "Truva Atı / Yönetim Davet" card inside the top stats row.

card_html = """
        <!-- BİNA YÖNETİM DAVETİ (TRUVA ATI) -->
        <div class="col-xl-3 col-sm-6 mb-xl-0 mb-4">
            <div class="card h-100 shadow-sm border-0 border-start border-info border-4 hover-shadow">
                <div class="card-body p-3">
                    <div class="row">
                        <div class="col-8">
                            <div class="numbers">
                                <p class="text-sm mb-0 text-capitalize font-weight-bold">Bina Yönetimi</p>
                                <h6 class="font-weight-bolder text-info mb-0">Müşteriyi Davet Et</h6>
                                <button class="btn btn-link text-info px-0 mb-0 mt-2" data-bs-toggle="modal" data-bs-target="#inviteManagerModal">
                                    <i class="bi bi-send-fill me-1"></i> SMS / Mail Gönder
                                </button>
                            </div>
                        </div>
                        <div class="col-4 text-end">
                            <div class="icon icon-shape bg-gradient-info shadow text-center border-radius-md">
                                <i class="bi bi-envelope-paper text-lg opacity-10" aria-hidden="true"></i>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
"""

# Let's insert it into the row with "col-xl-3 col-sm-6 mb-xl-0 mb-4" which is the top stats cards
# We can find the row <div class="row"> just after the top navbar or breadcrumbs
content = re.sub(r'(<div class="col-xl-3 col-sm-6 mb-xl-0 mb-4">.*?</div>\s*</div>\s*</div>)', r'\1\n' + card_html, content, count=1, flags=re.DOTALL)


# Now add the Modal at the bottom
modal_html = """
<!-- TRUVA ATI / MÜŞTERİ DAVET MODAL -->
<div class="modal fade" id="inviteManagerModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-info border-2 rounded-4 shadow-lg">
            <div class="modal-header bg-info bg-opacity-10 border-0">
                <h5 class="modal-title fw-bold text-dark"><i class="bi bi-building-check text-info me-2"></i> Yönetimi Şeffaf Portala Davet Et</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form action="/BuildingManagement/SendInvite" method="post">
                <div class="modal-body bg-light">
                    <input type="hidden" name="projectId" value="@Model.Id" />
                    <p class="text-muted small mb-4">Bu davet, bina yöneticisine "Teklifleri tek ekranda görebileceği" şeffaf bir portal linki gönderir. Kabul etmeleri halinde rakip firmalar da bu platforma teklif girmeye mecbur kalır (GMK360 Truva Atı).</p>
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold">Yönetici / Müşteri Adı Soyadı</label>
                        <input type="text" name="managerName" class="form-control" required placeholder="Örn: Ahmet Yılmaz" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Cep Telefonu (SMS İçin)</label>
                        <input type="text" name="managerPhone" class="form-control" required placeholder="05XX XXX XX XX" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">E-Posta (Opsiyonel)</label>
                        <input type="email" name="managerEmail" class="form-control" placeholder="ahmet@site.com" />
                    </div>
                </div>
                <div class="modal-footer border-0">
                    <button type="button" class="btn btn-secondary rounded-pill" data-bs-dismiss="modal">Vazgeç</button>
                    <button type="submit" class="btn btn-info text-white fw-bold rounded-pill px-4"><i class="bi bi-send-check me-2"></i> Davet Linki Oluştur</button>
                </div>
            </form>
        </div>
    </div>
</div>
"""

content = content + "\n" + modal_html

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected Invite Card and Modal into Details")
