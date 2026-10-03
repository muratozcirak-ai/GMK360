import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix the Where clause
target_where = r'Where\(i => i\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Submitted \|\| \(i\.OfferNotes != null && i\.OfferNotes\.Contains\("KEŞİF"\)\)\)'
replacement_where = 'Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted || (i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF")))'
content = re.sub(target_where, replacement_where, content)

# I also need to update the bestPrice calculation to include Accepted
target_bestprice = r'Where\(i => i\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Submitted \|\| \(i\.OfferNotes != null && i\.OfferNotes\.Contains\("KEŞİF"\)\)\)'
content = re.sub(target_bestprice, replacement_where, content)

# I need to add a "SEÇİLDİ" badge if the status is Accepted.
# The current rendering is:
# <span class="d-block fw-bold text-dark mb-1">@inv.Supplier?.CompanyName</span>
# <div class="d-flex align-items-center justify-content-between"> ... </div>
# I will find the span and add a check if it's Accepted.
target_name = r'<span class="d-block fw-bold text-dark mb-1">@inv\.Supplier\?\.CompanyName</span>'
replacement_name = '''<span class="d-block fw-bold text-dark mb-1">
                                                                            @inv.Supplier?.CompanyName
                                                                            @if(inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                <span class="badge bg-success ms-2 shadow-sm"><i class="bi bi-check-circle-fill"></i> SEÇİLDİ</span>
                                                                            }
                                                                          </span>'''
content = content.replace(target_name, replacement_name)

# Also if it's accepted, we don't need a "Seç" button anymore!
target_sec_btn = r'''@if \(inv\.OfferNotes \!= null && inv\.OfferNotes\.Contains\("KEŞİF"\)\) \{
\s*<span class="badge bg-danger-subtle text-danger"><i class="bi bi-exclamation-triangle-fill"></i> Keşif Talebi</span>
\s*\} else \{
\s*<form method="post" action="/PhaseZero/AcceptQuote/@doc\.Id" class="m-0">
\s*<input type="hidden" name="quoteAmount" value="@inv\.OfferedPrice" />
\s*<button type="submit" class="btn btn-sm btn-info text-white rounded-pill shadow-sm px-3 fw-bold" onclick="event\.stopPropagation\(\);">
\s*<i class="bi bi-check-circle"></i> Seç
\s*</button>
\s*</form>
\s*\}'''

replacement_sec_btn = '''@if (inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                <span class="badge bg-success-subtle text-success fs-6"><i class="bi bi-check2-all"></i> Onaylandı</span>
                                                                            } else if (inv.OfferNotes != null && inv.OfferNotes.Contains("KEŞİF")) {
                                                                                <span class="badge bg-danger-subtle text-danger"><i class="bi bi-exclamation-triangle-fill"></i> Keşif Talebi</span>
                                                                            } else {
                                                                                <form method="post" action="/PhaseZero/AcceptQuote/@doc.Id" class="m-0">
                                                                                    <input type="hidden" name="quoteAmount" value="@inv.OfferedPrice" />
                                                                                    <button type="submit" class="btn btn-sm btn-info text-white rounded-pill shadow-sm px-3 fw-bold" onclick="event.stopPropagation();">
                                                                                        <i class="bi bi-check-circle"></i> Seç
                                                                                    </button>
                                                                                </form>
                                                                            }'''
content = re.sub(target_sec_btn, replacement_sec_btn, content)

# I also need to update the submittedQ count to include Accepted!
target_submittedq = r'var submittedQ = relatedQuote\?\.Invites\.Count\(i => i\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Submitted \|\| \(i\.OfferNotes \!= null && i\.OfferNotes\.Contains\("KEŞİF"\)\)\) \?\? 0;'
replacement_submittedq = 'var submittedQ = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted || (i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF"))) ?? 0;'
content = content.replace(target_submittedq, replacement_submittedq)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)