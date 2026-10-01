import io
import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Inject right before the existing Offer/Submit anchor
content = re.sub(
    r'<a href="/Offer/Submit/@invite\.Id".*?>',
    r'<a href="/B2BPartnerPortal/Invite/@invite.AccessToken" target="_blank" class="btn btn-sm btn-outline-primary fw-bold me-2" title="Tedarikçi Davet Ekranını Aç (Yeni)"><i class="bi bi-person-workspace"></i> Partner Paneli (Simüle Et)</a>\n\t\t\t\t\t\t\t\t\t\t\t\t  <a href="/Offer/Submit/@invite.Id" target="_blank" class="btn btn-sm btn-outline-secondary me-2">',
    content
)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml via regex.")
