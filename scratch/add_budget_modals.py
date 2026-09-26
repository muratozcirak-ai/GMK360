import re

filepath = r'GMK360.Web\Views\ConstructionProject\BudgetDashboard.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

modals = """
    <!-- YENİ BÜTÇE KALEMİ MODAL -->
    <div class="modal fade" id="addBudgetItemModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 border-0 shadow">
                <div class="modal-header border-bottom-0 pb-0">
                    <h5 class="modal-title fw-bold"><i class="bi bi-plus-circle text-primary me-2"></i>Yeni Bütçe Kalemi (Planlanan)</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <form action="/ConstructionProject/AddBudgetItem" method="post">
                    <input type="hidden" name="projectId" value="@ViewBag.ProjectId" />
                    <input type="hidden" name="isExtra" value="false" />
                    <div class="modal-body">
                        <div class="mb-3">
                            <label class="form-label fw-bold small">İlgili Faz (WBS)</label>
                            <select name="phaseCategory" class="form-select" required>
                                @foreach(var phase in Enum.GetValues(typeof(GMK360.Core.Entities.Construction.BudgetPhaseCategory)).Cast<GMK360.Core.Entities.Construction.BudgetPhaseCategory>())
                                {
                                    <option value="@((int)phase)">Faz @((int)phase): @phase.ToString()</option>
                                }
                            </select>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">İş Kalemi Adı</label>
                            <input type="text" name="itemName" class="form-control" placeholder="Örn: C30 Hazır Beton" required />
                        </div>
                        <div class="row mb-3">
                            <div class="col-6">
                                <label class="form-label fw-bold small">Miktar</label>
                                <input type="number" step="0.01" name="quantity" class="form-control" value="1" required />
                            </div>
                            <div class="col-6">
                                <label class="form-label fw-bold small">Birim</label>
                                <input type="text" name="unit" class="form-control" placeholder="m3, Ton, Adet" value="Adet" required />
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Planlanan Birim Fiyat (₺)</label>
                            <input type="number" step="0.01" name="plannedUnitPrice" class="form-control" required />
                            <small class="text-muted">Müteahhit tahmini veya alınan teklif rakamı.</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Açıklama (Opsiyonel)</label>
                            <textarea name="description" class="form-control" rows="2"></textarea>
                        </div>
                    </div>
                    <div class="modal-footer border-top-0 pt-0">
                        <button type="button" class="btn btn-light" data-bs-dismiss="modal">İptal</button>
                        <button type="submit" class="btn btn-primary px-4 fw-bold">Kaydet</button>
                    </div>
                </form>
            </div>
        </div>
    </div>

    <!-- BEKLENMEYEN GİDER / FİŞ EKLENTİSİ MODAL -->
    <div class="modal fade" id="emergencyRequestModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content rounded-4 border-0 shadow">
                <div class="modal-header border-bottom-0 pb-0">
                    <h5 class="modal-title fw-bold text-danger"><i class="bi bi-receipt text-danger me-2"></i>Beklenmeyen Gider / Masraf Fişi</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <form action="/ConstructionProject/AddBudgetItem" method="post" enctype="multipart/form-data">
                    <input type="hidden" name="projectId" value="@ViewBag.ProjectId" />
                    <input type="hidden" name="isExtra" value="true" />
                    <input type="hidden" name="quantity" value="1" />
                    <input type="hidden" name="unit" value="Adet" />
                    <div class="modal-body">
                        <div class="alert alert-warning small">
                            <i class="bi bi-info-circle me-1"></i> Bu kayıt doğrudan <b>Gerçekleşen Harcama</b> olarak bütçeye yansır ve limiti düşürür. Fiş fotoğrafı eklenmesi zorunludur!
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Hangi Faza Ait?</label>
                            <select name="phaseCategory" class="form-select" required>
                                @foreach(var phase in Enum.GetValues(typeof(GMK360.Core.Entities.Construction.BudgetPhaseCategory)).Cast<GMK360.Core.Entities.Construction.BudgetPhaseCategory>())
                                {
                                    <option value="@((int)phase)">Faz @((int)phase): @phase.ToString()</option>
                                }
                            </select>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Gider / Masraf Adı</label>
                            <input type="text" name="itemName" class="form-control" placeholder="Örn: Taksi Fişi (Merkeze Gidiş), Acil Kum" required />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Toplam Fatura/Fiş Tutarı (₺)</label>
                            <input type="number" step="0.01" name="plannedUnitPrice" class="form-control text-danger fw-bold" required />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small">Açıklama (Zorunlu)</label>
                            <textarea name="description" class="form-control" rows="2" placeholder="Masrafın neden yapıldığını kısaca açıklayın." required></textarea>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold small text-danger">Fiş / Fatura Fotoğrafı (*)</label>
                            <input type="file" name="receiptImage" class="form-control" accept="image/*" />
                        </div>
                    </div>
                    <div class="modal-footer border-top-0 pt-0">
                        <button type="button" class="btn btn-light" data-bs-dismiss="modal">İptal</button>
                        <button type="submit" class="btn btn-danger px-4 fw-bold">Gider Olarak İşle</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
"""

if "addBudgetItemModal" not in content:
    content = content.replace("</div>\n</div>\n", "</div>\n" + modals + "\n</div>\n")
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
