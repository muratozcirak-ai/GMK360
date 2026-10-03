import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the Teklif buttons
# Main Row:
target1 = r'<form method="post" action="/PhaseZero/RequestQuote/@doc\.Id" class="m-0 p-0">\s*<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @\(isDependent && !isReadyToApply \? "disabled" : ""\)>\s*<i class="bi bi-cart-plus"></i> Teklif\s*</button>\s*</form>'

replacement1 = '''@if(relatedQuote != null) {
                                                              <button type="button" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Firma Teklifi Ekle" onclick="openInviteModal(@relatedQuote.Id, '@doc.DocumentName')">
                                                                  <i class="bi bi-plus-circle"></i> Firma Ekle
                                                              </button>
                                                          } else {
                                                              <form method="post" action="/PhaseZero/RequestQuote/@doc.Id" class="m-0 p-0">
                                                                  <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @(isDependent && !isReadyToApply ? "disabled" : "")>
                                                                      <i class="bi bi-cart-plus"></i> Teklif
                                                                  </button>
                                                              </form>
                                                          }'''
content = re.sub(target1, replacement1, content)


# Child Row:
target2 = r'<form method="post" action="/PhaseZero/RequestQuote/@childDoc\.Id" class="m-0 p-0">\s*<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">\s*<i class="bi bi-cart-plus"></i> Teklif\s*</button>\s*</form>'

replacement2 = '''@if(childQuote != null) {
                                                                      <button type="button" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Firma Teklifi Ekle" onclick="openInviteModal(@childQuote.Id, '@childDoc.DocumentName')">
                                                                          <i class="bi bi-plus-circle"></i> Firma Ekle
                                                                      </button>
                                                                  } else {
                                                                      <form method="post" action="/PhaseZero/RequestQuote/@childDoc.Id" class="m-0 p-0">
                                                                          <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">
                                                                              <i class="bi bi-cart-plus"></i> Teklif
                                                                          </button>
                                                                      </form>
                                                                  }'''
content = re.sub(target2, replacement2, content)


# Add the modal HTML and JS at the end of the file
modal_html = '''
    <!-- FIRMA EKLE MODAL -->
    <div class="modal fade" id="inviteFirmModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow-lg rounded-4">
                <div class="modal-header border-0 bg-info text-white rounded-top-4">
                    <h5 class="modal-title fw-bold"><i class="bi bi-building me-2"></i> Yeni Firma Teklifi Ekle</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Kapat"></button>
                </div>
                <form asp-action="AddFirmQuote" method="post">
                    <div class="modal-body p-4">
                        <input type="hidden" id="inviteQuoteRequestId" name="quoteRequestId" />
                        
                        <p class="text-muted fw-bold mb-4" id="inviteDocNameTitle">Evrak</p>
                        
                        <div class="mb-3">
                            <label class="form-label fw-bold">Firma Adı</label>
                            <input type="text" class="form-control" name="companyName" required placeholder="Örn: Asım Hafriyat" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">Verilen Fiyat Teklifi (TL)</label>
                            <input type="number" step="0.01" class="form-control" name="offeredPrice" placeholder="Boş bırakırsanız Keşif Talebi sayılır" />
                            <small class="text-muted">Eğer firma fiyat veremeyip saha ziyareti/keşif istiyorsa boş bırakın veya 0 yazın.</small>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">Firma Notu (Keşif vs.)</label>
                            <textarea class="form-control" name="offerNotes" rows="2" placeholder="Örn: Fiyat vermek için önce sahayı görmek istiyoruz."></textarea>
                        </div>
                    </div>
                    <div class="modal-footer border-top-0 bg-light rounded-bottom-4">
                        <button type="button" class="btn btn-outline-secondary fw-bold rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                        <button type="submit" class="btn btn-info text-white fw-bold rounded-pill px-4 shadow-sm"><i class="bi bi-save me-1"></i> Teklifi Kaydet</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
    
    <script>
        function openInviteModal(quoteReqId, docName) {
            document.getElementById('inviteQuoteRequestId').value = quoteReqId;
            document.getElementById('inviteDocNameTitle').innerText = docName;
            var modal = new bootstrap.Modal(document.getElementById('inviteFirmModal'));
            modal.show();
        }
    </script>
'''

content = content.replace('</body>', modal_html + '\n</body>')
# Or if there is no </body>, just append it.
if '</body>' not in content:
    content += '\n' + modal_html

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)

print("Modals injected.")