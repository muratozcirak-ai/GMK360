import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target1 = r'''<div>\s*<h6 class="mb-0 text-dark fw-bold">@inv\.NetworkContact\?\.CompanyName</h6>\s*<span class="text-success fw-bold fs-6">@inv\.OfferedPrice\?\.ToString\("C2"\)</span>\s*</div>\s*<form asp-action="AcceptQuote" method="post">\s*<input type="hidden" name="documentId" value="@doc\.Id" />\s*<input type="hidden" name="inviteId" value="@inv\.Id" />\s*<button type="submit" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">\s*<i class="bi bi-check-circle-fill"></i> Seç\s*</button>\s*</form>'''

replacement1 = '''<div>
                                                                              <h6 class="mb-0 text-dark fw-bold">@inv.NetworkContact?.CompanyName</h6>
                                                                              @if(inv.OfferedPrice.HasValue) {
                                                                                  <span class="text-success fw-bold fs-6">@inv.OfferedPrice.Value.ToString("C2")</span>
                                                                              } else {
                                                                                  <span class="text-danger fw-bold" style="font-size: 0.75rem;"><i class="bi bi-exclamation-triangle"></i> Keşif Talebi (Saha Görüşmesi)</span>
                                                                              }
                                                                          </div>
                                                                          @if(inv.OfferedPrice.HasValue) {
                                                                              <form asp-action="AcceptQuote" method="post">
                                                                                  <input type="hidden" name="documentId" value="@doc.Id" />
                                                                                  <input type="hidden" name="inviteId" value="@inv.Id" />
                                                                                  <button type="submit" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">
                                                                                      <i class="bi bi-check-circle-fill"></i> Seç
                                                                                  </button>
                                                                              </form>
                                                                          }'''

content = re.sub(target1, replacement1, content)


target2 = r'''<div>\s*<h6 class="mb-0 text-dark fw-bold">@inv\.NetworkContact\?\.CompanyName</h6>\s*<span class="text-success fw-bold fs-6">@inv\.OfferedPrice\?\.ToString\("C2"\)</span>\s*</div>\s*<form asp-action="AcceptQuote" method="post">\s*<input type="hidden" name="documentId" value="@childDoc\.Id" />\s*<input type="hidden" name="inviteId" value="@inv\.Id" />\s*<button type="submit" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">\s*<i class="bi bi-check-circle-fill"></i> Seç\s*</button>\s*</form>'''

replacement2 = '''<div>
                                                                                <h6 class="mb-0 text-dark fw-bold">@inv.NetworkContact?.CompanyName</h6>
                                                                                @if(inv.OfferedPrice.HasValue) {
                                                                                    <span class="text-success fw-bold fs-6">@inv.OfferedPrice.Value.ToString("C2")</span>
                                                                                } else {
                                                                                    <span class="text-danger fw-bold" style="font-size: 0.75rem;"><i class="bi bi-exclamation-triangle"></i> Keşif Talebi (Saha Görüşmesi)</span>
                                                                                }
                                                                            </div>
                                                                            @if(inv.OfferedPrice.HasValue) {
                                                                                <form asp-action="AcceptQuote" method="post">
                                                                                    <input type="hidden" name="documentId" value="@childDoc.Id" />
                                                                                    <input type="hidden" name="inviteId" value="@inv.Id" />
                                                                                    <button type="submit" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">
                                                                                        <i class="bi bi-check-circle-fill"></i> Seç
                                                                                    </button>
                                                                                </form>
                                                                            }'''

content = re.sub(target2, replacement2, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)