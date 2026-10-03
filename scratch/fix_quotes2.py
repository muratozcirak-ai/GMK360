import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# 1. Add SEÇİLDİ badge
target_name = r'<h6 class="mb-0 text-dark fw-bold">@inv\.NetworkContact\?\.CompanyName</h6>'
replacement_name = '''<h6 class="mb-0 text-dark fw-bold">
                                                                                @inv.NetworkContact?.CompanyName
                                                                                @if(inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                    <span class="badge bg-success ms-1"><i class="bi bi-check-circle-fill"></i> ONAYLI</span>
                                                                                }
                                                                            </h6>'''
content = re.sub(target_name, replacement_name, content)

# 2. Hide Seç button if Accepted
target_form = r'<form asp-action="AcceptQuote".*?</form>'
replacement_form = '''@if(inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                <span class="badge bg-success-subtle text-success border border-success p-2"><i class="bi bi-check2-all"></i> Satınalma Onaylı</span>
                                                                            } else {
                                                                                <form asp-action="AcceptQuote" method="post">
                                                                                    <input type="hidden" name="documentId" value="@doc.Id" />
                                                                                    <input type="hidden" name="inviteId" value="@inv.Id" />
                                                                                    <button type="submit" onclick="event.stopPropagation();" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">
                                                                                        <i class="bi bi-check-circle-fill"></i> Seç
                                                                                    </button>
                                                                                </form>
                                                                            }'''
content = re.sub(target_form, replacement_form, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)