import io

filepath = r'GMK360.Web\Views\B2BPurchasing\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the old Offer link with the new Partner Portal link, or put them side by side.
old_link = """<a href="/Offer/Submit/@invite.Id" target="_blank" class="btn btn-sm btn-outline-secondary me-2" title="Giden Teklif Formunu A">
                                                      <i class="bi bi-box-arrow-up-right"></i> Formu A
                                                  </a>"""

new_link = """<a href="/B2BPartnerPortal/Invite/@invite.AccessToken" target="_blank" class="btn btn-sm btn-outline-primary fw-bold me-2" title="Tedarikçi Davet Ekranını Aç (Yeni)">
                                                      <i class="bi bi-person-workspace"></i> Partner Portalı (Simüle Et)
                                                  </a>
                                                  <a href="/Offer/Submit/@invite.Id" target="_blank" class="btn btn-sm btn-outline-secondary me-2" title="Giden Teklif Formunu Aç (Eski)">
                                                      <i class="bi bi-box-arrow-up-right"></i> Formu Aç
                                                  </a>"""

content = content.replace(old_link, new_link)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml to include the B2BPartnerPortal link.")
