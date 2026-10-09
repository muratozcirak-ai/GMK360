with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# We want to replace the row but KEEP the closing tags properly!
# Let's just find the exact block and replace it using string replacement.
old_str1 = """                            <!-- Main Block Info -->
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
                            </div>"""

new_str1 = """                            <!-- Main Block Info -->
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
                            }"""

old_str2 = """                            <!-- Main Block Info -->
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
                            </div>"""

new_str2 = """                            <!-- Main Block Info -->
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
                            }"""

content = content.replace(old_str1, new_str1)
content = content.replace(old_str2, new_str2)

with open("GMK360.Web/Views/ConstructionProject/Details.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed razor syntax!")
