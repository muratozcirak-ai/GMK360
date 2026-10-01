import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Inject the "Teklifleri Onayla" button in the Action column if there are submitted quotes.
# Wait, I already calculate `submittedQ` inside the row.
# Let's find the button group for actions.
button_search = r'<form action="/PhaseZero/RequestB2BQuote" method="post".*?</form>'
# It's better to replace the whole Action column cell logic.

# Let's search for: <div class="d-flex justify-content-end gap-2">
action_col = r'(<div class="d-flex justify-content-end gap-2">.*?<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal\(@doc\.Id\)")'
action_injection = """<div class="d-flex justify-content-end gap-2">
                                                        @if(submittedQ > 0)
                                                        {
                                                            <button class="btn btn-sm btn-success rounded-pill px-3 fw-bold shadow-sm" onclick="openPinModal(@doc.Id, @bestPrice, '@doc.DocumentName')">
                                                                <i class="bi bi-key-fill"></i> Teklif Onayla
                                                            </button>
                                                        }
                                                        <form action="/PhaseZero/RequestB2BQuote" method="post" class="d-inline">
                                                            <input type="hidden" name="docId" value="@doc.Id" />
                                                            <button type="submit" class="btn btn-sm btn-outline-warning text-dark rounded-pill px-2 fw-bold shadow-sm" title="B2B Fiyat İste">
                                                                <i class="bi bi-cart-plus"></i> Teklif
                                                            </button>
                                                        </form>
                                                        <button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@doc.Id)"""

content = re.sub(action_col, action_injection, content, flags=re.DOTALL)

# 2. Add "Bedelsiz" checkbox to the Manage Modal
manage_modal_search = r'(<!-- Maliyetler -->.*?<div class="col-md-6">)'
manage_modal_injection = """<!-- Maliyetler -->
                              <div class="col-md-12 mb-3">
                                  <div class="form-check form-switch fs-6 p-3 bg-light rounded-3 border">
                                      <input class="form-check-input ms-0 mt-1" type="checkbox" id="modalIsFreeInternal" name="isFreeInternal" onchange="toggleCostFields()" style="width: 2.5em; height: 1.25em;">
                                      <label class="form-check-label fw-bold text-success ms-3" for="modalIsFreeInternal">
                                          <i class="bi bi-gift-fill me-1"></i> Bu İşlem Bedelsiz / Şirket İçi Yapılacaktır
                                      </label>
                                  </div>
                              </div>
                              <div class="col-md-6">"""

content = re.sub(manage_modal_search, manage_modal_injection, content, flags=re.DOTALL)

# 3. Add PIN Modal and JS
pin_modal = """
    <!-- BOSS PIN APPROVAL MODAL -->
    <div class="modal fade" id="pinApprovalModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow-lg rounded-4">
                <div class="modal-header border-0 bg-success text-white rounded-top-4">
                    <h5 class="modal-title fw-bold"><i class="bi bi-shield-lock-fill me-2"></i> Yönetici Onayı (PIN)</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <div class="modal-body p-4 text-center">
                    <h5 id="pinDocName" class="fw-bold mb-3 text-dark">Evrak Adı</h5>
                    <div class="alert alert-info rounded-3 mb-4">
                        Sistemde bulunan <strong>en uygun teklif: <span id="pinBestPrice" class="fs-5 fw-bold text-success">0</span> ₺</strong>.<br/>
                        Teklifi resmi olarak maliyete işlemek ve diğer firmaları reddetmek için PIN kodunuzu giriniz.
                    </div>
                    
                    <label class="form-label fw-bold text-secondary">6 Haneli Yönetici PIN Kodu</label>
                    <div class="d-flex justify-content-center gap-2 mb-3">
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;" autofocus>
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;">
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;">
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;">
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;">
                        <input type="password" class="form-control form-control-lg text-center fw-bold pin-digit" maxlength="1" style="width: 50px; font-size: 24px;">
                    </div>
                    <small class="text-muted" id="pinErrorText" style="display:none;"></small>
                </div>
                <div class="modal-footer border-0 bg-light rounded-bottom-4 d-flex justify-content-center">
                    <button type="button" class="btn btn-success fw-bold rounded-pill px-5 shadow-sm" onclick="verifyPinAndAccept()">
                        <i class="bi bi-check-circle-fill me-2"></i> TEKLİFİ ONAYLA
                    </button>
                </div>
            </div>
        </div>
    </div>
"""

# Append modal right before Manage Modal
content = content.replace('<div class="modal fade" id="manageDocModal"', pin_modal + '\n    <div class="modal fade" id="manageDocModal"')

# Add JS scripts
js_injection = """
        let currentPinDocId = 0;
        let currentBestPrice = 0;

        function openPinModal(docId, bestPrice, docName) {
            currentPinDocId = docId;
            currentBestPrice = bestPrice;
            document.getElementById('pinDocName').innerText = docName;
            document.getElementById('pinBestPrice').innerText = bestPrice.toLocaleString('tr-TR');
            
            // Temizle
            document.querySelectorAll('.pin-digit').forEach(i => i.value = '');
            document.getElementById('pinErrorText').style.display = 'none';
            
            var modal = new bootstrap.Modal(document.getElementById('pinApprovalModal'));
            modal.show();
        }

        // Auto focus for PIN digits
        document.querySelectorAll('.pin-digit').forEach((input, index, inputs) => {
            input.addEventListener('input', function() {
                if (this.value.length === 1 && index < inputs.length - 1) {
                    inputs[index + 1].focus();
                }
            });
            input.addEventListener('keydown', function(e) {
                if (e.key === 'Backspace' && this.value.length === 0 && index > 0) {
                    inputs[index - 1].focus();
                }
            });
        });

        function verifyPinAndAccept() {
            let pin = Array.from(document.querySelectorAll('.pin-digit')).map(i => i.value).join('');
            if(pin.length < 6) {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Lütfen 6 haneli şifreyi eksiksiz girin.</span>";
                document.getElementById('pinErrorText').style.display = 'block';
                return;
            }
            
            // Simüle edilmiş PIN onayı (Gerçek senaryoda backend'e gider)
            if(pin !== '123456') {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Hatalı PIN! (Demo için: 123456)</span>";
                document.getElementById('pinErrorText').style.display = 'block';
                return;
            }
            
            // Onaylandı, sayfayı reload yap
            alert("Teklif başarıyla onaylandı ve maliyet olarak işlendi!");
            location.reload();
        }

        function toggleCostFields() {
            var isFree = document.getElementById('modalIsFreeInternal').checked;
            var feeInput = document.getElementById('modalDocFee');
            var addInput = document.getElementById('modalAddCost');
            if(isFree) {
                feeInput.value = 0;
                addInput.value = 0;
                feeInput.setAttribute('readonly', true);
                addInput.setAttribute('readonly', true);
                feeInput.classList.add('bg-light');
                addInput.classList.add('bg-light');
            } else {
                feeInput.removeAttribute('readonly');
                addInput.removeAttribute('readonly');
                feeInput.classList.remove('bg-light');
                addInput.classList.remove('bg-light');
            }
        }
"""
content = content.replace("function openManageModal(docId) {", js_injection + "\n        function openManageModal(docId) {")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Injected PIN and Bedelsiz Modal")
