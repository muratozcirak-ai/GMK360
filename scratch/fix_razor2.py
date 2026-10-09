with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

new_render = r"""@if (Model.Blocks != null && Model.Blocks.Any())
{
    // ONLY get root blocks (ParentBuildingId == null)
    var rootExistingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null).ToList();
    var rootNewBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null).ToList();

    if (rootExistingBlocks.Any())
    {
        <h6 class="fw-bold text-danger border-bottom border-danger pb-2 mb-3 mt-4"><i class="bi bi-building-dash me-2"></i>Eski (Yklacak) Binalar (Paketler)</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in rootExistingBlocks)
            {
                var children = Model.Blocks.Where(c => c.ParentBuildingId == block.Id).ToList();
                <div class="col-12">
                    <div class="card border border-danger border-opacity-25 shadow-sm rounded-4 h-100">
                        <div class="card-header bg-danger bg-opacity-10 border-0 pt-3 pb-2">
                            <h5 class="fw-bold text-danger mb-0"><i class="ph ph-intersect me-2"></i>@block.Name @(children.Any() ? "(Ana Yap)" : "(Tekil Yap)")</h5>
                        </div>
                        <div class="card-body p-4">
                            <!-- Main Block Info -->
                            @if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0)
                            {
                                <div class="row mb-4">
                                    <div class="col-md-4">
                                        <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yap: @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="col-md-4 text-center">
                                        <div class="fw-bold fs-5 text-dark"><i class="bi bi-door-open text-danger me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dkkan</div>
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-danger rounded-pill btn-sm">
                                            Ana Yap Plann Ynet <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>
                            }
                            
                            <!-- Sub Blocks (Towers) -->
                            @if(children.Any())
                            {
                                <h6 class="fw-bold text-dark border-bottom pb-2 mb-3"><i class="ph ph-buildings me-2"></i>Alt Yaplar (Kuleler / Bloklar)</h6>
                                <div class="row g-3">
                                    @foreach(var child in children)
                                    {
                                        <div class="col-md-6 col-lg-4">
                                            <div class="card border-0 bg-light rounded-3 shadow-sm h-100">
                                                <div class="card-body p-3">
                                                    <div class="d-flex justify-content-between mb-2">
                                                        <h6 class="fw-bold text-dark mb-0">@child.Name</h6>
                                                        <span class="badge bg-secondary">@(child.TotalFloors ?? 0) Kat</span>
                                                    </div>
                                                    <div class="text-muted small mb-3">
                                                        <i class="bi bi-door-open me-1"></i>@(child.TotalUnits - child.TotalShops) Daire, @(child.TotalShops) Dkkan
                                                    </div>
                                                    <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-danger fw-bold">
                                                        Kule/Blok Ynet <i class="bi bi-arrow-right ms-1"></i>
                                                    </a>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
                        </div>
                    </div>
                </div>
            }
        </div>
    }

    if (rootNewBlocks.Any())
    {
        <h6 class="fw-bold text-primary border-bottom border-primary pb-2 mb-3 mt-4"><i class="bi bi-building-add me-2"></i>Yeni (Hedef) Binalar (Paketler)</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in rootNewBlocks)
            {
                var children = Model.Blocks.Where(c => c.ParentBuildingId == block.Id).ToList();
                <div class="col-12">
                    <div class="card border border-primary border-opacity-25 shadow-sm rounded-4 h-100">
                        <div class="card-header bg-primary bg-opacity-10 border-0 pt-3 pb-2">
                            <h5 class="fw-bold text-primary mb-0"><i class="ph ph-intersect me-2"></i>@block.Name @(children.Any() ? "(Ana Yap)" : "(Tekil Yap)")</h5>
                        </div>
                        <div class="card-body p-4">
                            <!-- Main Block Info -->
                            @if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0)
                            {
                                <div class="row mb-4">
                                    <div class="col-md-4">
                                        <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yap / Taban: @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="col-md-4 text-center">
                                        <div class="fw-bold fs-5 text-dark"><i class="bi bi-door-open text-primary me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dkkan</div>
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-primary rounded-pill btn-sm">
                                            Taban/Baza Plann Ynet <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>
                            }
                            
                            <!-- Sub Blocks (Towers) -->
                            @if(children.Any())
                            {
                                <h6 class="fw-bold text-dark border-bottom pb-2 mb-3"><i class="ph ph-buildings me-2"></i>Alt Yaplar (Kuleler / Bloklar)</h6>
                                <div class="row g-3">
                                    @foreach(var child in children)
                                    {
                                        <div class="col-md-6 col-lg-4">
                                            <div class="card border-0 bg-light rounded-3 shadow-sm h-100">
                                                <div class="card-body p-3">
                                                    <div class="d-flex justify-content-between mb-2">
                                                        <h6 class="fw-bold text-dark mb-0">@child.Name</h6>
                                                        <span class="badge bg-secondary">@(child.TotalFloors ?? 0) Kat</span>
                                                    </div>
                                                    <div class="text-muted small mb-3">
                                                        <i class="bi bi-door-open me-1"></i>@(child.TotalUnits - child.TotalShops) Daire, @(child.TotalShops) Dkkan
                                                    </div>
                                                    <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-primary fw-bold">
                                                        Kule/Blok Ynet <i class="bi bi-arrow-right ms-1"></i>
                                                    </a>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
                        </div>
                    </div>
                </div>
            }
        </div>
    }
}"""

content = re.sub(r'@if \(Model\.Blocks != null && Model\.Blocks\.Any\(\)\)\s*\{.*?\}\s*\}', new_render, content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated Details.cshtml perfectly!")
