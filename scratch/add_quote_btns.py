import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Make the header wider
content = content.replace('<th class="text-end pe-4" style="width:100px;">İşlem</th>', '<th class="text-end pe-4" style="width:180px;">İşlem</th>')

# Add the Fiyat İste button to the Main Doc Row
old_btn_main = """<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@doc.Id)" @(isDependent && !isReadyToApply ? "disabled title='Önce alt ön koşul evrakları tamamlanmalıdır'" : "")>
                                                      @if(isDependent && !isReadyToApply) { <i class="bi bi-lock-fill text-danger"></i> } else { <i class="bi bi-pencil-square"></i> } Yönet
                                                  </button>"""
new_btn_main = """<div class="d-flex justify-content-end gap-1">
                                                      <form method="post" action="/PhaseZero/RequestQuote/@doc.Id" class="m-0 p-0">
                                                          <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @(isDependent && !isReadyToApply ? "disabled" : "")>
                                                              <i class="bi bi-cart-plus"></i> Teklif
                                                          </button>
                                                      </form>
                                                      <button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@doc.Id)" @(isDependent && !isReadyToApply ? "disabled title='Önce alt ön koşul evrakları tamamlanmalıdır'" : "")>
                                                          @if(isDependent && !isReadyToApply) { <i class="bi bi-lock-fill text-danger"></i> } else { <i class="bi bi-pencil-square"></i> } Yönet
                                                      </button>
                                                  </div>"""
content = content.replace(old_btn_main, new_btn_main)

# Add the Fiyat İste button to the Child Doc Row
old_btn_child = """<button class="btn btn-sm btn-outline-secondary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@childDoc.Id)"><i class="bi bi-pencil-square"></i> Yönet</button>"""
new_btn_child = """<div class="d-flex justify-content-end gap-1">
                                                                <form method="post" action="/PhaseZero/RequestQuote/@childDoc.Id" class="m-0 p-0">
                                                                    <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">
                                                                        <i class="bi bi-cart-plus"></i> Teklif
                                                                    </button>
                                                                </form>
                                                                <button class="btn btn-sm btn-outline-secondary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@childDoc.Id)">
                                                                    <i class="bi bi-pencil-square"></i> Yönet
                                                                </button>
                                                            </div>"""
content = content.replace(old_btn_child, new_btn_child)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Index.cshtml with RequestQuote buttons.")
