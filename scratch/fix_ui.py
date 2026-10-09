with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# We will apply the filtering
content = content.replace("var rootExistingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null).ToList();", "var rootExistingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null && ((b.TotalFloors ?? 0) > 0 || b.TotalUnits > 0 || b.TotalShops > 0 || Model.Blocks.Any(c => c.ParentBuildingId == b.Id))).ToList();")
content = content.replace("var rootNewBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null).ToList();", "var rootNewBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null && ((b.TotalFloors ?? 0) > 0 || b.TotalUnits > 0 || b.TotalShops > 0 || Model.Blocks.Any(c => c.ParentBuildingId == b.Id))).ToList();")

# Now we fix the Bitişik Nizam UI
# For existing blocks:
existing_replace = r"""
                            <!-- Main Block Info -->
                            @if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0 || block.LayoutPattern == "Bitişik Nizam")
                            {
                                <div class="row mb-4 align-items-center">
                                    <div class="col-md-8">
                                        @if(block.LayoutPattern == "Bitişik Nizam")
                                        {
                                            <span class="px-3 py-2 bg-danger bg-opacity-10 text-danger border border-danger rounded-pill d-inline-block fw-bold"><i class="bi bi-building me-1"></i>Bitişik Nizam (Tek Temel)</span>
                                        }
                                        else if ((block.TotalFloors ?? 0) > 0)
                                        {
                                            <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yapı: @(block.TotalFloors ?? 0) Kat</span>
                                        }
                                        
                                        @if(block.TotalUnits > 0 || block.TotalShops > 0)
                                        {
                                            <span class="ms-2 fw-bold text-dark"><i class="bi bi-door-open text-danger me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</span>
                                        }
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-danger rounded-pill btn-sm fw-bold px-4 py-2 shadow-sm">
                                            <i class="bi bi-grid-1x2 me-1"></i> @(block.LayoutPattern == "Bitişik Nizam" ? "Bitişik Nizam Ortak İskeletini Yönet" : "Ana Yapı Planını Yönet") <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>
                            }
                            
                            <!-- Sub Blocks (Towers) -->
                            @if(children.Any())
                            {
                                <h6 class="fw-bold text-dark border-bottom pb-2 mb-3"><i class="ph ph-buildings me-2"></i>Alt Yapılar (Kuleler / Bloklar)</h6>
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
                                                        <i class="bi bi-door-open me-1"></i>@(child.TotalUnits - child.TotalShops) Daire, @(child.TotalShops) Dükkan
                                                    </div>
                                                    @if(block.LayoutPattern != "Bitişik Nizam")
                                                    {
                                                        <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-danger fw-bold">
                                                            Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                        </a>
                                                    }
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
"""

new_replace = r"""
                            <!-- Main Block Info -->
                            @if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0 || block.LayoutPattern == "Bitişik Nizam")
                            {
                                <div class="row mb-4 align-items-center">
                                    <div class="col-md-8">
                                        @if(block.LayoutPattern == "Bitişik Nizam")
                                        {
                                            <span class="px-3 py-2 bg-primary bg-opacity-10 text-primary border border-primary rounded-pill d-inline-block fw-bold"><i class="bi bi-building me-1"></i>Bitişik Nizam (Tek Temel)</span>
                                        }
                                        else if ((block.TotalFloors ?? 0) > 0)
                                        {
                                            <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yapı / Taban: @(block.TotalFloors ?? 0) Kat</span>
                                        }
                                        
                                        @if(block.TotalUnits > 0 || block.TotalShops > 0)
                                        {
                                            <span class="ms-2 fw-bold text-dark"><i class="bi bi-door-open text-primary me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</span>
                                        }
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-primary rounded-pill btn-sm fw-bold px-4 py-2 shadow-sm">
                                            <i class="bi bi-grid-1x2 me-1"></i> @(block.LayoutPattern == "Bitişik Nizam" ? "Bitişik Nizam Ortak İskeletini Yönet" : "Taban/Baza Planını Yönet") <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>
                            }
                            
                            <!-- Sub Blocks (Towers) -->
                            @if(children.Any())
                            {
                                <h6 class="fw-bold text-dark border-bottom pb-2 mb-3"><i class="ph ph-buildings me-2"></i>Alt Yapılar (Kuleler / Bloklar)</h6>
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
                                                        <i class="bi bi-door-open me-1"></i>@(child.TotalUnits - child.TotalShops) Daire, @(child.TotalShops) Dükkan
                                                    </div>
                                                    @if(block.LayoutPattern != "Bitişik Nizam")
                                                    {
                                                        <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-primary fw-bold">
                                                            Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                        </a>
                                                    }
                                                </div>
                                            </div>
                                        </div>
                                    }
                                </div>
                            }
"""

content = re.sub(r'<!-- Main Block Info -->\s*@if \(\(block\.TotalFloors \?\? 0\) > 0 \|\| block\.TotalUnits > 0 \|\| block\.TotalShops > 0\)\s*\{.*?\}\s*<!-- Sub Blocks \(Towers\) -->\s*@if\(children\.Any\(\)\)\s*\{.*?\}\s*</div', existing_replace + '\n                        </div', content, count=1, flags=re.DOTALL)
content = re.sub(r'<!-- Main Block Info -->\s*@if \(\(block\.TotalFloors \?\? 0\) > 0 \|\| block\.TotalUnits > 0 \|\| block\.TotalShops > 0\)\s*\{.*?\}\s*<!-- Sub Blocks \(Towers\) -->\s*@if\(children\.Any\(\)\)\s*\{.*?\}\s*</div', new_replace + '\n                        </div', content, count=1, flags=re.DOTALL)


with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated perfectly.")
