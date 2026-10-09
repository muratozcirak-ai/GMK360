with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# 1. Existing Blocks replacement
existing_subblocks_html = """<div class="sub-blocks-container mt-4 pt-3 border-top border-danger border-opacity-25" style="display: none;">
                                                    <div class="row">
                                                        <div class="col-12 col-lg-4">
                                                            <label class="form-label small fw-bold text-danger sub-block-label">Kaç Kule / Blok Var?</label>
                                                            <input type="number" class="form-control form-control-sm" min="0" max="10" value="@(b.SubBlocks != null ? b.SubBlocks.Count : 0)" oninput="generateSubBlocks(this, 'ExistingBlocks', @i)" />
                                                        </div>
                                                    </div>
                                                    <div class="sub-blocks-list">
                                                        @if (b.SubBlocks != null && b.SubBlocks.Any()) {
                                                            for (int j = 0; j < b.SubBlocks.Count; j++) {
                                                                var sb = b.SubBlocks[j];
                                                                string labelName = (b.LayoutPattern != null && b.LayoutPattern.Contains("Ortak Baza")) ? "Kule" : "Blok";
                                                                <div class="card border border-secondary border-opacity-25 shadow-sm rounded-4 mt-3 bg-light">
                                                                    <div class="card-body p-3">
                                                                        <h6 class="fw-bold text-secondary mb-3"><i class="ph ph-buildings me-2"></i> @(j+1). @labelName</h6>
                                                                        <div class="row g-2">
                                                                            <div class="col-6 col-lg-3">
                                                                                <label class="form-label small fw-bold">Adı</label>
                                                                                <input type="text" name="ExistingBlocks[@i].SubBlocks[@j].BlockName" class="form-control form-control-sm" value="@sb.BlockName" />
                                                                            </div>
                                                                            <div class="col-6 col-lg-2">
                                                                                <label class="form-label small fw-bold">Kat Sayısı</label>
                                                                                <input type="number" name="ExistingBlocks[@i].SubBlocks[@j].TotalFloors" class="form-control form-control-sm" min="0" value="@sb.TotalFloors" />
                                                                            </div>
                                                                            <div class="col-6 col-lg-2">
                                                                                <label class="form-label small fw-bold">Daire</label>
                                                                                <input type="number" name="ExistingBlocks[@i].SubBlocks[@j].TotalApartments" class="form-control form-control-sm" value="@sb.TotalApartments" />
                                                                            </div>
                                                                            <div class="col-6 col-lg-2">
                                                                                <label class="form-label small fw-bold">Dükkan</label>
                                                                                <input type="number" name="ExistingBlocks[@i].SubBlocks[@j].TotalShops" class="form-control form-control-sm" value="@sb.TotalShops" />
                                                                            </div>
                                                                            <div class="col-6 col-lg-3">
                                                                                <label class="form-label small fw-bold">Bodrum Kat</label>
                                                                                <input type="number" name="ExistingBlocks[@i].SubBlocks[@j].BasementFloors" class="form-control form-control-sm" value="@sb.BasementFloors" />
                                                                            </div>
                                                                            <div class="col-6 col-lg-2 mt-3">
                                                                                <label class="form-label small fw-bold">Zemin Kat</label>
                                                                                <div class="form-check form-switch">
                                                                                    <input class="form-check-input" type="checkbox" name="ExistingBlocks[@i].SubBlocks[@j].HasGroundFloor" value="true" checked="@sb.HasGroundFloor">
                                                                                </div>
                                                                            </div>
                                                                            <div class="col-6 col-lg-2 mt-3">
                                                                                <label class="form-label small fw-bold">Çatı Katı</label>
                                                                                <div class="form-check form-switch">
                                                                                    <input class="form-check-input" type="checkbox" name="ExistingBlocks[@i].SubBlocks[@j].HasRoof" value="true" checked="@sb.HasRoof">
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            }
                                                        }
                                                    </div>
                                                </div>"""

content = re.sub(r'<div class="sub-blocks-container mt-4 pt-3 border-top border-danger border-opacity-25" style="display: none;">.*?<div class="sub-blocks-list"></div>', existing_subblocks_html, content, count=1, flags=re.DOTALL)


# 2. Target Blocks replacement
target_subblocks_html = existing_subblocks_html.replace('ExistingBlocks', 'TargetBlocks').replace('border-danger', 'border-primary').replace('text-danger', 'text-primary')
content = re.sub(r'<div class="sub-blocks-container mt-4 pt-3 border-top border-primary border-opacity-25" style="display: none;">.*?<div class="sub-blocks-list"></div>', target_subblocks_html, content, count=1, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated Razor view for SubBlocks loading!")
