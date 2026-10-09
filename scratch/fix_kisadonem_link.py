import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the 5th card
old_card = '''<!-- 5. Günlük & Kısa Dönem -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-secondary bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-calendar-event fs-5 text-secondary"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Kısa Dönem Kiralık</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Check-in / Check-out takvimi, temizlik yönlendirme ve dinamik fiyatlama ile gelirinizi katlayın.</p>
                    <a href="#" class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

new_card = '''<!-- 5. Günlük & Kısa Dönem & Pansiyon -->
        <div class="col-lg-3 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 h-100 hover-lift bg-white">
                <div class="card-body p-3 d-flex flex-column">
                    <div class="d-flex align-items-center mb-2">
                        <div class="bg-secondary bg-opacity-10 rounded-3 d-flex align-items-center justify-content-center me-3 flex-shrink-0" style="width: 40px; height: 40px;">
                            <i class="bi bi-calendar-event fs-5 text-secondary"></i>
                        </div>
                        <h6 class="fw-bold text-navy mb-0 lh-sm">Günlük Kiralık & Pansiyon</h6>
                    </div>
                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Check-in takvimi, KBS (Emniyet) bildirimi, temizlik yönlendirme ve dinamik fiyatlama ile geliri katlayın.</p>
                    <a href="/Modules/KisaDonem" target="_blank" class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>'''

content = content.replace(old_card, new_card)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
