with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# 1. Fix the ghost building issue by skipping blocks with 0 floors/units that have no children
content = content.replace(
    "var rootExistingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null).ToList();",
    "var rootExistingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding && b.ParentBuildingId == null && ((b.TotalFloors ?? 0) > 0 || b.TotalUnits > 0 || b.TotalShops > 0 || Model.Blocks.Any(c => c.ParentBuildingId == b.Id))).ToList();"
)
content = content.replace(
    "var rootNewBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null).ToList();",
    "var rootNewBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding && b.ParentBuildingId == null && ((b.TotalFloors ?? 0) > 0 || b.TotalUnits > 0 || b.TotalShops > 0 || Model.Blocks.Any(c => c.ParentBuildingId == b.Id))).ToList();"
)

# 2. In Existing Blocks, update Main Block Info condition
content = content.replace(
    "@if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0)",
    "@if ((block.TotalFloors ?? 0) > 0 || block.TotalUnits > 0 || block.TotalShops > 0 || block.LayoutPattern == \"Bitişik Nizam\")"
)

# 3. Inside Existing Blocks -> Main Block Info -> replace the old row with the new one
old_row = """<div class="row mb-4">
                                    <div class="col-md-4">
                                        <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yapı: @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="col-md-4 text-center">
                                        <div class="fw-bold fs-5 text-dark"><i class="bi bi-door-open text-danger me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</div>
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-danger rounded-pill btn-sm">
                                            Ana Yapı Planını Yönet <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>"""

new_row = """<div class="row mb-4 align-items-center">
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
                                </div>"""
content = content.replace(old_row, new_row)


# 4. Inside New Blocks -> Main Block Info -> replace the old row with the new one
old_row2 = """<div class="row mb-4">
                                    <div class="col-md-4">
                                        <span class="px-3 py-2 bg-light text-dark border rounded-pill d-inline-block fw-bold"><i class="bi bi-layers me-1"></i>Ana Yapı / Taban: @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="col-md-4 text-center">
                                        <div class="fw-bold fs-5 text-dark"><i class="bi bi-door-open text-primary me-1"></i>@(block.TotalUnits - block.TotalShops) Daire, @(block.TotalShops) Dükkan</div>
                                    </div>
                                    <div class="col-md-4 text-end">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-outline-primary rounded-pill btn-sm">
                                            Taban/Baza Planını Yönet <i class="bi bi-arrow-right ms-1"></i>
                                        </a>
                                    </div>
                                </div>"""

new_row2 = """<div class="row mb-4 align-items-center">
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
                                </div>"""
content = content.replace(old_row2, new_row2)

# 5. Hide the Manage buttons in sub-blocks if parent is Bitişik Nizam
# We need to wrap the `<a>` tags in the sub-blocks loop with an @if check.
old_a1 = """<a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-danger fw-bold">
                                                        Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                    </a>"""
new_a1 = """@if(block.LayoutPattern != "Bitişik Nizam")
                                                    {
                                                        <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-danger fw-bold">
                                                            Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                        </a>
                                                    }"""
content = content.replace(old_a1, new_a1)

old_a2 = """<a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-primary fw-bold">
                                                        Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                    </a>"""
new_a2 = """@if(block.LayoutPattern != "Bitişik Nizam")
                                                    {
                                                        <a asp-action="ManageBlock" asp-route-id="@child.Id" class="btn btn-sm btn-light border w-100 text-primary fw-bold">
                                                            Kule/Blok Yönet <i class="bi bi-arrow-right ms-1"></i>
                                                        </a>
                                                    }"""
content = content.replace(old_a2, new_a2)

with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated successfully.")
