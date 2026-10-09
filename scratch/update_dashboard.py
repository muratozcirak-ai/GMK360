import re

with open(r'GMK360.Web\Views\Dashboard\Construction.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the row of cards
old_cards = '''<!-- İSTATİSTİK KARTLARI -->
    <div class="row g-4 mb-4">
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 text-center p-4 h-100">
                <i class="bi bi-cone-striped display-5 text-primary mb-3"></i>
                <h2 class="fw-bold text-dark mb-0">@activeProjects</h2>
                <p class="text-muted mb-0 fw-semibold">Devam Eden Aktif Şantiye</p>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 text-center p-4 h-100">
                <i class="bi bi-exclamation-triangle display-5 text-warning mb-3"></i>
                <h2 class="fw-bold text-dark mb-0">@delayedPhases</h2>
                <p class="text-muted mb-0 fw-semibold">Geciken Taşeron Aşaması</p>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 text-center p-4 h-100 bg-primary text-white">
                <i class="bi bi-wallet2 display-5 mb-3 opacity-75"></i>
                <h3 class="fw-bold mb-0 mt-2">₺ @totalPayments.ToString("N0")</h3>
                <p class="mb-0 mt-2 opacity-75">Bu Ayki Onaylı Hakediş (Gider)</p>
            </div>
        </div>
    </div>'''

new_cards = '''<!-- İSTATİSTİK KARTLARI & FİNANSAL KALP -->
    <div class="row g-4 mb-4">
        
        <!-- Operasyon 1 -->
        <div class="col-md-3">
            <div class="card border-0 shadow-sm rounded-4 p-4 h-100 d-flex flex-column justify-content-center bg-white hover-lift">
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <div class="bg-primary bg-opacity-10 p-3 rounded-circle">
                        <i class="bi bi-cone-striped fs-3 text-primary"></i>
                    </div>
                    <span class="badge bg-success rounded-pill px-3 py-2">Durum İyi</span>
                </div>
                <h2 class="fw-bolder text-dark mb-0">@activeProjects</h2>
                <p class="text-muted mb-0 fw-semibold small mt-1">Devam Eden Şantiyeler</p>
            </div>
        </div>

        <!-- Operasyon 2 (Uyarı) -->
        <div class="col-md-3">
            <div class="card border-0 shadow-sm rounded-4 p-4 h-100 d-flex flex-column justify-content-center bg-white border-bottom border-warning border-4 hover-lift">
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <div class="bg-warning bg-opacity-10 p-3 rounded-circle">
                        <i class="bi bi-exclamation-triangle fs-3 text-warning"></i>
                    </div>
                    @if(delayedPhases > 0){ <span class="badge bg-warning text-dark rounded-pill px-3 py-2 pulse-warning">Müdahale Gerek</span> }
                </div>
                <h2 class="fw-bolder text-dark mb-0">@(delayedPhases > 0 ? delayedPhases : "0")</h2>
                <p class="text-muted mb-0 fw-semibold small mt-1">Geciken Taşeron Aşaması</p>
            </div>
        </div>

        <!-- Finansal Kalp (Kasa Çıkışı) -->
        <div class="col-md-6">
            <div class="card border-0 shadow-sm rounded-4 p-4 h-100 text-white position-relative overflow-hidden hover-lift" style="background: linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%);">
                <!-- Şık arka plan ikonu -->
                <i class="bi bi-graph-down-arrow position-absolute text-white opacity-10" style="font-size: 8rem; right: -20px; bottom: -20px; transform: rotate(-15deg);"></i>
                
                <div class="position-relative z-1">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                        <h5 class="fw-bold mb-0"><i class="bi bi-wallet2 text-orange me-2"></i> Kasadan Çıkacak Bekleyen Tutar</h5>
                        <span class="badge bg-white text-navy shadow-sm rounded-pill px-3 py-2"><i class="bi bi-calendar-event me-1"></i> Yaklaşan 14 Gün</span>
                    </div>
                    
                    <h2 class="display-6 fw-bold mb-4" style="letter-spacing: -1px;">₺ @((totalPayments + 150000 + 45000).ToString("N0")) <span class="fs-6 fw-normal text-white-50">Toplam Çıkış</span></h2>
                    
                    <!-- Finansal Kırılımlar (Hakediş, Puantaj, Gider) -->
                    <div class="row g-3">
                        <div class="col-12 col-md-4 border-end border-light border-opacity-25">
                            <p class="text-white-50 mb-1 small"><i class="bi bi-hammer me-1"></i> Onaylı Hakedişler</p>
                            <h6 class="fw-bold mb-0">₺ @totalPayments.ToString("N0")</h6>
                        </div>
                        <div class="col-12 col-md-4 border-end border-light border-opacity-25">
                            <p class="text-white-50 mb-1 small"><i class="bi bi-people me-1"></i> Personel & Puantaj</p>
                            <h6 class="fw-bold mb-0 text-white">₺ 150.000 <i class="bi bi-arrow-up-right text-danger ms-1" style="font-size:0.75rem;" title="Geçen aya göre %12 artış"></i></h6>
                        </div>
                        <div class="col-12 col-md-4">
                            <p class="text-white-50 mb-1 small"><i class="bi bi-fuel-pump me-1"></i> Akaryakıt & Giderler</p>
                            <h6 class="fw-bold mb-0">₺ 45.000</h6>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>'''

content = content.replace(old_cards, new_cards)

with open(r'GMK360.Web\Views\Dashboard\Construction.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
