import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

summary_cards = '''
        <!-- YENİ EKLENEN: ÖZET MALİYET TABLOSU -->
        @if (Model.Any())
        {
            var totalFee = Model.Sum(x => x.DocumentFee ?? 0);
            var totalAdditional = Model.Sum(x => x.AdditionalCost ?? 0);
            var grandTotal = totalFee + totalAdditional;
            var completedCount = Model.Count(x => x.Status == "Tamamlandı");
            var totalCount = Model.Count();
            var percentage = totalCount > 0 ? (completedCount * 100) / totalCount : 0;

            <div class="row g-4 mb-4">
                <div class="col-xl-3 col-md-6">
                    <div class="card border-0 shadow-sm rounded-4 bg-primary text-white h-100">
                        <div class="card-body p-4 d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="fw-bold mb-1 opacity-75">TOPLAM FAZ-0 MALİYETİ</h6>
                                <h3 class="fw-bold mb-0">@grandTotal.ToString("N2") ₺</h3>
                            </div>
                            <div class="bg-white bg-opacity-25 rounded-circle p-3">
                                <i class="bi bi-wallet2 fs-3"></i>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-xl-3 col-md-6">
                    <div class="card border-0 shadow-sm rounded-4 h-100">
                        <div class="card-body p-4 d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="fw-bold mb-1 text-muted">RESMİ HARÇLAR & ÜCRETLER</h6>
                                <h3 class="fw-bold mb-0 text-dark">@totalFee.ToString("N2") ₺</h3>
                            </div>
                            <div class="bg-light rounded-circle p-3 text-secondary">
                                <i class="bi bi-bank fs-3"></i>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-xl-3 col-md-6">
                    <div class="card border-0 shadow-sm rounded-4 h-100">
                        <div class="card-body p-4 d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="fw-bold mb-1 text-muted">EK GİDER / KOMİSYON</h6>
                                <h3 class="fw-bold mb-0 text-danger">@totalAdditional.ToString("N2") ₺</h3>
                            </div>
                            <div class="bg-danger bg-opacity-10 rounded-circle p-3 text-danger">
                                <i class="bi bi-cash-coin fs-3"></i>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-xl-3 col-md-6">
                    <div class="card border-0 shadow-sm rounded-4 h-100">
                        <div class="card-body p-4">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <h6 class="fw-bold mb-0 text-muted">EVRAK İLERLEMESİ</h6>
                                <span class="badge bg-success">@completedCount / @totalCount Tamamlandı</span>
                            </div>
                            <div class="progress" style="height: 10px;">
                                <div class="progress-bar bg-success" role="progressbar" style="width: @percentage%;" aria-valuenow="@percentage" aria-valuemin="0" aria-valuemax="100"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        }
        <!-- ÖZET TABLOSU BİTİŞ -->
        
        @if (!Model.Any())'''

content = content.replace('@if (!Model.Any())', summary_cards)

# Also let's fix the "Maliyet (?)" to "Maliyet (₺)"
content = content.replace('Maliyet (?)', 'Maliyet (₺)')
content = content.replace('FAZ 0 Ynetim Masas', 'FAZ 0 Yönetim Masası')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Phase Zero summary UI updated.')