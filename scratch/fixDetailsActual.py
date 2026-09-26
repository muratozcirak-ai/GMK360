import sys

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

start_idx = html.find('<h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>')
if start_idx == -1:
    print("START NOT FOUND")
    sys.exit(1)

# Find the end of this block. It ends with:
#         </div>
#     }
# </div>
end_str = 'Ltfen dzenleme ekranndan blok ekleyin.'
end_idx = html.find(end_str, start_idx)
if end_idx == -1:
    end_str = 'Lütfen düzenleme ekranından blok ekleyin.'
    end_idx = html.find(end_str, start_idx)

if end_idx == -1:
    print("END NOT FOUND")
    sys.exit(1)

# Find the enclosing </div></div></div> after end_idx
final_end_idx = html.find('</div>', end_idx)
final_end_idx = html.find('</div>', final_end_idx + 6)
final_end_idx = html.find('</div>', final_end_idx + 6)

if final_end_idx == -1:
    print("FINAL END NOT FOUND")
    sys.exit(1)

final_end_idx += 6 # include the </div>

replacement = '''<h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>
            
            @if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding))
            {
                <h6 class="fw-bold text-danger mt-4 mb-3 border-bottom border-danger border-opacity-25 pb-2"><i class="bi bi-building-dash me-2"></i> Eski (Yıkılacak) Binalar</h6>
                <div class="row g-4 mb-4">
                    @foreach (var block in Model.Blocks.Where(b => b.IsExistingBuilding))
                    {
                        <div class="col-md-6 col-lg-4">
                            <div class="card border border-danger border-opacity-25 shadow-sm rounded-4 h-100 hover-shadow transition-all">
                                <div class="card-body p-4">
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <h6 class="fw-bold text-danger mb-0">@block.BlockName</h6>
                                        <span class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-3"><i class="bi bi-layers me-1"></i> @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="d-flex justify-content-between text-muted small mb-4">
                                        <span><i class="bi bi-door-closed me-1"></i> Toplam @block.TotalUnits Bağımsız Bölüm</span>
                                        <span class="text-success"><i class="bi bi-check-circle me-1"></i> @(block.Units?.Count ?? 0) Tanımlı</span>
                                    </div>
                                    <div class="text-center">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-sm btn-outline-danger rounded-pill px-4">Kat ve Daire Planlarını Yönet <i class="bi bi-arrow-right ms-1"></i></a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            }

            @if (Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))
            {
                <h6 class="fw-bold text-primary mt-4 mb-3 border-bottom border-primary border-opacity-25 pb-2"><i class="bi bi-building-add me-2"></i> Yeni (Hedef) Binalar</h6>
                <div class="row g-4 mb-4">
                    @foreach (var block in Model.Blocks.Where(b => !b.IsExistingBuilding))
                    {
                        <div class="col-md-6 col-lg-4">
                            <div class="card border border-primary border-opacity-25 shadow-sm rounded-4 h-100 hover-shadow transition-all">
                                <div class="card-body p-4">
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <h6 class="fw-bold text-primary mb-0">@block.BlockName</h6>
                                        <span class="badge bg-primary bg-opacity-10 text-primary rounded-pill px-3"><i class="bi bi-layers me-1"></i> @(block.TotalFloors ?? 0) Kat</span>
                                    </div>
                                    <div class="d-flex justify-content-between text-muted small mb-4">
                                        <span><i class="bi bi-door-closed me-1"></i> Toplam @block.TotalUnits Bağımsız Bölüm</span>
                                        <span class="text-success"><i class="bi bi-check-circle me-1"></i> @(block.Units?.Count ?? 0) Tanımlı</span>
                                    </div>
                                    <div class="text-center">
                                        <a asp-action="ManageBlock" asp-route-id="@block.Id" class="btn btn-sm btn-outline-primary rounded-pill px-4">Kat ve Daire Planlarını Yönet <i class="bi bi-arrow-right ms-1"></i></a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            }

            @if (Model.Blocks == null || !Model.Blocks.Any())
            {
                <div class="row g-4">
                    <div class="col-12">
                        <div class="alert alert-info rounded-4 border-0 d-flex align-items-center">
                            <i class="bi bi-info-circle-fill fs-4 me-3"></i>
                            <div>
                                Bu projeye henüz blok eklenmemiş. Lütfen düzenleme ekranından blok ekleyin.
                            </div>
                        </div>
                    </div>
                </div>
            }'''

new_html = html[:start_idx] + replacement + html[final_end_idx:]

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(new_html)

print("SUCCESS")
