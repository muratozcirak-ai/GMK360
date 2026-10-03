import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Modify the Name badge
target_name = r'@if\(inv\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Accepted\) \{\s*<span class="badge bg-success ms-1"><i class="bi bi-check-circle-fill"></i> ONAYLI</span>\s*\}'
replacement_name = '''@if(inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                    <span class="badge bg-success ms-1"><i class="bi bi-check-circle-fill"></i> RESMİ ONAYLI</span>
                                                                                } else if (inv.IsFeasibilitySelected) {
                                                                                    <span class="badge bg-primary ms-1"><i class="bi bi-star-fill text-warning"></i> FİZİBİLİTEDE KULLANILDI</span>
                                                                                }'''
content = re.sub(target_name, replacement_name, content, flags=re.DOTALL)


# Modify the Form block
target_form = r'@if\(inv\.Status == GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Accepted\) \{\s*<span class="badge bg-success-subtle text-success border border-success p-2"><i class="bi bi-check2-all"></i> Satınalma Onaylı</span>\s*\} else if \(inv\.OfferNotes'
replacement_form = '''@if(inv.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted) {
                                                                                <span class="badge bg-success-subtle text-success border border-success p-2"><i class="bi bi-check2-all"></i> Satınalma Onaylı</span>
                                                                            } else if (inv.IsFeasibilitySelected) {
                                                                                <span class="badge bg-primary-subtle text-primary border border-primary p-2"><i class="bi bi-calculator"></i> Fizibiliteye Eklendi</span>
                                                                            } else if (inv.OfferNotes'''
content = re.sub(target_form, replacement_form, content, flags=re.DOTALL)


# Let's change the text of the Seç button to "Fizibilite Seç" to be clearer
content = content.replace('<i class="bi bi-check-circle-fill"></i> Seç', '<i class="bi bi-calculator"></i> Fizibilite Seç')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)