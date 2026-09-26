import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace the heading and badge with a single heading
# Old text:
# <h6 class="fw-bold text-danger mb-0">@block.BlockName</h6>
# <span class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-3"><i class="bi bi-layers me-1"></i> @(block.TotalFloors ?? 0) Kat</span>

html = html.replace(
    '<h6 class="fw-bold text-danger mb-0">@block.BlockName</h6>\n                                        <span class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-3"><i class="bi bi-layers me-1"></i> @(block.TotalFloors ?? 0) Kat</span>',
    '<h5 class="fw-bold text-danger mb-0">@block.BlockName <span class="fs-6 text-muted fw-normal ms-1">(@(block.TotalFloors ?? 0) Kat)</span></h5>'
)

html = html.replace(
    '<h6 class="fw-bold text-primary mb-0">@block.BlockName</h6>\n                                        <span class="badge bg-primary bg-opacity-10 text-primary rounded-pill px-3"><i class="bi bi-layers me-1"></i> @(block.TotalFloors ?? 0) Kat</span>',
    '<h5 class="fw-bold text-primary mb-0">@block.BlockName <span class="fs-6 text-muted fw-normal ms-1">(@(block.TotalFloors ?? 0) Kat)</span></h5>'
)

# Replace the text for independent units
# Old text:
# <span><i class="bi bi-door-closed me-1"></i> Toplam @block.TotalUnits Bağımsız Bölüm</span>

html = html.replace(
    '<span><i class="bi bi-door-closed me-1"></i> Toplam @block.TotalUnits Bağımsız Bölüm</span>',
    '<span><i class="bi bi-door-closed me-1"></i> @block.TotalApartments Daire, @block.TotalShops Dükkan</span>'
)
# Just in case they had TotalUnits written differently:
html = html.replace(
    '<span><i class="bi bi-door-closed me-1"></i> Toplam @(block.TotalApartments + block.TotalShops) Bağımsız Bölüm</span>',
    '<span><i class="bi bi-door-closed me-1"></i> @block.TotalApartments Daire, @block.TotalShops Dükkan</span>'
)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
