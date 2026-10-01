import io
import re

filepath = r'GMK360.Web\Views\LandlordDashboard\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the empty state buttons to add "İlana Çıkar" button
target = """<button class="btn btn-light border shadow-sm rounded-pill fw-bold text-primary px-4" data-bs-toggle="modal" data-bs-target="#editTenantModal-@prop.Id">
                                        <i class="bi bi-person-plus-fill me-2"></i> Kiracı & Sözleşme Ekle
                                    </button>"""

replacement = """<div class="d-flex flex-column flex-md-row justify-content-center gap-3">
                                        <button class="btn btn-light border shadow-sm rounded-pill fw-bold text-primary px-4" data-bs-toggle="modal" data-bs-target="#editTenantModal-@prop.Id">
                                            <i class="bi bi-person-plus-fill me-2"></i> Eski Kiracıyı Ekle
                                        </button>
                                        <a href="/Property/Create?sourcePropertyId=@prop.Id" class="btn btn-warning border-0 shadow-sm rounded-pill fw-bold text-dark px-4" title="Tek tıkla GMK360 Emlak Portalında ilana çıkın">
                                            <i class="bi bi-megaphone-fill me-2"></i> Hemen İlana Çıkar
                                        </a>
                                    </div>"""

content = content.replace(target, replacement)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated LandlordDashboard Index with İlana Çıkar button")
