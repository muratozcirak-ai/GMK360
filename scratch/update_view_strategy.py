import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'else if \(item\.QuoteStatus == GMK360\.Core\.Entities\.Construction\.BudgetQuoteStatus\.WaitingForPrice\)\s*\{\s*<form action="/PhaseOne/RequestQuote/@item\.Id" method="post" class="d-inline">\s*<button type="submit" class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2"><i class="bi bi-briefcase"></i> Teklif İste</button>\s*</form>\s*\}'
replacement = '''else if (item.QuoteStatus == GMK360.Core.Entities.Construction.BudgetQuoteStatus.WaitingForPrice)
                                                    {
                                                        if (item.ProcurementStrategy == GMK360.Core.Entities.Construction.ProcurementStrategy.NotSelected)
                                                        {
                                                            <button class="btn btn-sm btn-primary rounded-pill fw-bold px-3 me-2" onclick="openStrategyModal(@item.Id, '@item.ItemName')"><i class="bi bi-diagram-3"></i> Strateji Seç</button>
                                                        }
                                                        else if (item.ProcurementStrategy == GMK360.Core.Entities.Construction.ProcurementStrategy.Purchase)
                                                        {
                                                            <form action="/PhaseOne/RequestQuote/@item.Id" method="post" class="d-inline">
                                                                <button type="submit" class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2"><i class="bi bi-cart"></i> Satınalma İhalesi Başlat</button>
                                                            </form>
                                                        }
                                                        else if (item.ProcurementStrategy == GMK360.Core.Entities.Construction.ProcurementStrategy.Rent)
                                                        {
                                                            <button class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName (Aylık Kira)', '@item.PlannedUnitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description')"><i class="bi bi-calendar-range"></i> Kira Gideri Ekle</button>
                                                        }
                                                    }
                                                    else if (item.ProcurementStrategy == GMK360.Core.Entities.Construction.ProcurementStrategy.InternalTransfer || item.ProcurementStrategy == GMK360.Core.Entities.Construction.ProcurementStrategy.Borrow)
                                                    {
                                                        <span class="badge bg-success-subtle text-success border border-success me-2 px-3 py-2 rounded-pill"><i class="bi bi-box-seam"></i> Depodan / Şirketten Çözüldü</span>
                                                        <button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName (Nakliye/Transfer Bedeli)', '@item.PlannedUnitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description')"><i class="bi bi-truck"></i> Nakliye Gir</button>
                                                    }'''
content = re.sub(target, replacement, content)

target_modal = r'<!-- Yeni Kalem Ekle Modal -->'
replacement_modal = '''<!-- Strateji Modal -->
<div class="modal fade" id="strategyModal" tabindex="-1">
    <div class="modal-dialog">
        <form action="/PhaseOne/SelectStrategy" method="post" class="modal-content border-0 shadow">
            <input type="hidden" name="id" id="strategyId" />
            
            <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-dark">Tedarik Stratejisi Belirle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <p class="text-muted small mb-3">Seçilen Kalem: <strong id="strategyTitle" class="text-dark"></strong></p>
                <div class="mb-3">
                    <label class="form-label fw-bold text-dark">Lütfen bu kalemi şantiyeye nasıl getireceğinizi seçin:</label>
                    <select name="strategy" class="form-select form-select-lg shadow-sm" required>
                        <option value="">-- Strateji Seçin --</option>
                        <option value="1">Satın Alma (B2B Tedarikçilerden Fiyat İste)</option>
                        <option value="2">Aylık Kiralama (Kira Süresi ve Aylık Tutar Girilecek)</option>
                        <option value="3">Merkez Depodan / Eski Şantiyeden Transfer (Sıfır Malzeme Maliyeti)</option>
                        <option value="4">Kardeş / Ortak Firmadan Ödünç Alma</option>
                    </select>
                </div>
                <div class="alert alert-info py-2 small">
                    <i class="bi bi-info-circle me-1"></i> Depo veya Ödünç seçerseniz sistem sizden satın alma bedeli istemez, sadece varsa nakliye/sevk bedelini girmenizi ister.
                </div>
            </div>
            <div class="modal-footer border-0">
                <button type="button" class="btn btn-light" data-bs-dismiss="modal">İptal</button>
                <button type="submit" class="btn btn-primary px-4">Uygula</button>
            </div>
        </form>
    </div>
</div>

<!-- Yeni Kalem Ekle Modal -->'''
content = re.sub(target_modal, replacement_modal, content)

target_script = r'function openManageModal\(id, title, price, desc\) \{'
replacement_script = '''function openStrategyModal(id, title) {
        document.getElementById('strategyId').value = id;
        document.getElementById('strategyTitle').innerText = title;
        new bootstrap.Modal(document.getElementById('strategyModal')).show();
    }

    function openManageModal(id, title, price, desc) {'''
content = re.sub(target_script, replacement_script, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)