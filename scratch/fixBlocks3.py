import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# We will match from <h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>
# until the end of that </div> row. We know it's followed by <!-- TASLAK / KONU TARTIŞMA BÖLÜMÜ -->

pattern = r'<h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>.*?(?=<!-- TASLAK)'

replacement = '''<h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>

@if (Model.Blocks != null && Model.Blocks.Any())
{
    var existingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding).ToList();
    var newBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding).ToList();

    if (existingBlocks.Any())
    {
        <h6 class="fw-bold text-danger border-bottom border-danger pb-2 mb-3 mt-4"><i class="bi bi-building-dash me-2"></i>Eski (Yıkılacak) Binalar</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in existingBlocks)
            {
                <div class="col-md-6 col-lg-4">
                    <div class="card border border-danger border-opacity-25 shadow-sm rounded-4 h-100">
                        <div class="card-body p-4">
                            <div class="d-flex justify-content-between align-items-start mb-3">
                                <h5 class="fw-bold text-danger mb-0">@block.BlockName</h5>
                                <span class="px-2 py-1 bg-light text-dark border rounded small"><i class="bi bi-layers me-1"></i>@(block.TotalFloors ?? 0) Kat</span>
                            </div>
                            
                            <div class="d-flex justify-content-between text-muted small mb-4">
                                <div><i class="bi bi-door-open me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</div>
                                <div><i class="bi bi-check-circle text-success me-1"></i>@(block.Units?.Count ?? 0) Tanımlı</div>
                            </div>
    
                            <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-danger w-100 rounded-pill">
                                Kat ve Daire Planlarını Yönet <i class="bi bi-arrow-right ms-1"></i>
                            </a>
                        </div>
                    </div>
                </div>
            }
        </div>
    }

    if (newBlocks.Any())
    {
        <h6 class="fw-bold text-primary border-bottom border-primary pb-2 mb-3 mt-4"><i class="bi bi-building-add me-2"></i>Yeni (Hedef) Binalar</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in newBlocks)
            {
                <div class="col-md-6 col-lg-4">
                    <div class="card border border-primary border-opacity-25 shadow-sm rounded-4 h-100">
                        <div class="card-body p-4">
                            <div class="d-flex justify-content-between align-items-start mb-3">
                                <h5 class="fw-bold text-primary mb-0">@block.BlockName</h5>
                                <span class="px-2 py-1 bg-light text-dark border rounded small"><i class="bi bi-layers me-1"></i>@(block.TotalFloors ?? 0) Kat</span>
                            </div>
                            
                            <div class="d-flex justify-content-between text-muted small mb-4">
                                <div><i class="bi bi-door-open me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</div>
                                <div><i class="bi bi-check-circle text-success me-1"></i>@(block.Units?.Count ?? 0) Tanımlı</div>
                            </div>
    
                            <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-primary w-100 rounded-pill">
                                Kat ve Daire Planlarını Yönet <i class="bi bi-arrow-right ms-1"></i>
                            </a>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
}
else
{
    <div class="row g-4 mb-4">
        <div class="col-12">
            <div class="text-center p-5 bg-light rounded-4 text-muted">
                <i class="bi bi-building-x fs-1"></i>
                <p class="mt-2">Projeye henüz blok/bina eklenmemiş.</p>
                <a asp-action="ManageBlocks" asp-route-id="@Model.Id" class="btn btn-primary rounded-pill mt-2">Hemen Ekle</a>
            </div>
        </div>
    </div>
}

'''

if re.search(pattern, html, flags=re.DOTALL):
    html = re.sub(pattern, replacement, html, count=1, flags=re.DOTALL)
    print("SUCCESS")
else:
    print("NOT FOUND")

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
