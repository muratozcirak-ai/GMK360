$viewPath = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$viewText = [System.IO.File]::ReadAllText($viewPath)

# 1. Change layout sizes
$viewText = $viewText.Replace('<div class="col-md-4">', '<div class="col-md-3">')
$viewText = $viewText.Replace('<div class="col-md-8">', '<div class="col-md-9">')

# 2. Add data-bs-parent to accordion items
$viewText = $viewText.Replace('<div id="@collapseId" class="accordion-collapse collapse" aria-labelledby="@headingId">', '<div id="@collapseId" class="accordion-collapse collapse" aria-labelledby="@headingId" data-bs-parent="#floorAccordion">')

# 3. Fix Accordion Button Layout
$oldBtn = @"
                                    <button class="accordion-button bg-light fw-bold collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#@collapseId" aria-expanded="false" aria-controls="@collapseId">
                                        <i class="bi bi-building-up me-2 text-primary"></i> @floorName
                                        <span class="badge bg-secondary ms-auto me-3">@(floorGroup.Count()) Birim</span>
                                        @if(planDoc != null)
                                        {
                                            <span class="badge bg-success me-2"><i class="bi bi-file-earmark-image"></i> Plan Yüklü</span>
                                        }
                                    </button>
"@
$newBtn = @"
                                    <button class="accordion-button bg-light fw-bold collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#@collapseId" aria-expanded="false" aria-controls="@collapseId">
                                        <div class="d-flex justify-content-between align-items-center w-100 me-3">
                                            <span><i class="bi bi-building-up me-2 text-primary"></i> @floorName</span>
                                            <div>
                                                @if(planDoc != null)
                                                {
                                                    <span class="badge bg-success me-2"><i class="bi bi-file-earmark-image"></i> Plan Yüklü</span>
                                                }
                                                <span class="badge bg-secondary">@(floorGroup.Count()) Birim</span>
                                            </div>
                                        </div>
                                    </button>
"@
$viewText = $viewText.Replace($oldBtn, $newBtn)

# 4. Portal Button change
$oldPortal = '<a asp-controller="CustomerPortal" asp-action="MyUnitMaterials" asp-route-unitId="@unit.Id" class="btn btn-sm btn-outline-primary rounded-pill me-1" title="Müşteri Seçim Portalı (Görünüm)"><i class="bi bi-shop"></i> Portal</a>'
$newPortal = '<a asp-controller="CustomerPortal" asp-action="MyUnitMaterials" asp-route-unitId="@unit.Id" class="btn btn-sm btn-outline-primary rounded-pill me-1" target="_blank" title="Daire Detayı (Yeni Sekme)"><i class="bi bi-door-open"></i> Daireyi Yönet</a>'
$viewText = $viewText.Replace($oldPortal, $newPortal)

# 5. Edit Modal Update
$oldModal = @"
<div class="modal fade" id="editUnitModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="UpdateUnitProperties" method="post">
                <input type="hidden" name="Id" id="editUnitId" />
                <input type="hidden" name="BuildingId" value="@Model.Id" />
                <div class="modal-header border-0 pb-0">
                    <h5 class="modal-title fw-bold"><i class="bi bi-pencil-square text-primary me-2"></i>Bağımsız Bölümü Düzenle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Kapı No / İsim</label>
                        <input type="text" name="DoorNumber" id="editUnitDoorNumber" class="form-control" required />
                        <div class="form-text">Örn: Daire 1, Dükkan 2, Sığınak, Otopark</div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Kullanım Türü / Plan</label>
                        <select name="RoomLayout" id="editUnitRoomLayout" class="form-select">
                            <option value="1+0">1+0</option>
                            <option value="1+1">1+1</option>
                            <option value="2+1">2+1</option>
                            <option value="3+1">3+1</option>
                            <option value="4+1">4+1</option>
                            <option value="Dubleks (4+1)">Dubleks (4+1)</option>
                            <option value="Ticari Alan">Ticari Alan (Dükkan)</option>
                            <option value="Açık Alan">Açık Alan (Otopark vb.)</option>
                            <option value="Ortak Alan">Ortak Alan (Sığınak vb.)</option>
                        </select>
                    </div>
                </div>
                <div class="modal-footer border-0 pt-0">
                    <button type="button" class="btn btn-light rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script>
    function openEditUnitModal(id, number, layout) {
        document.getElementById("editUnitId").value = id;
        document.getElementById("editUnitDoorNumber").value = number;
        document.getElementById("editUnitRoomLayout").value = layout;
        new bootstrap.Modal(document.getElementById("editUnitModal")).show();
    }
</script>
"@
$newModal = @"
<div class="modal fade" id="editUnitModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="UpdateUnitProperties" method="post">
                <input type="hidden" name="Id" id="editUnitId" />
                <input type="hidden" name="BuildingId" value="@Model.Id" />
                <div class="modal-header border-0 pb-0">
                    <h5 class="modal-title fw-bold"><i class="bi bi-pencil-square text-primary me-2"></i>Bağımsız Bölümü Düzenle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Kapı No / İsim</label>
                            <input type="text" name="DoorNumber" id="editUnitDoorNumber" class="form-control" required />
                        </div>
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Kullanım Türü / Plan</label>
                            <select name="RoomLayout" id="editUnitRoomLayout" class="form-select">
                                <option value="1+0">1+0</option>
                                <option value="1+1">1+1</option>
                                <option value="2+1">2+1</option>
                                <option value="3+1">3+1</option>
                                <option value="4+1">4+1</option>
                                <option value="Dubleks (4+1)">Dubleks (4+1)</option>
                                <option value="Ticari Alan">Ticari Alan (Dükkan)</option>
                                <option value="Açık Alan">Açık Alan (Otopark vb.)</option>
                                <option value="Ortak Alan">Ortak Alan (Sığınak vb.)</option>
                            </select>
                        </div>
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Mal Sahibi (Ad Soyad)</label>
                            <input type="text" name="OwnerName" id="editUnitOwnerName" class="form-control" placeholder="Sahipsiz" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Telefon</label>
                            <input type="text" name="OwnerPhone" id="editUnitOwnerPhone" class="form-control" placeholder="05XX..." />
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-0 pt-0">
                    <button type="button" class="btn btn-light rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary rounded-pill px-4">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script>
    function openEditUnitModal(id, number, layout, ownerName, ownerPhone) {
        document.getElementById("editUnitId").value = id;
        document.getElementById("editUnitDoorNumber").value = number;
        document.getElementById("editUnitRoomLayout").value = layout;
        document.getElementById("editUnitOwnerName").value = ownerName || '';
        document.getElementById("editUnitOwnerPhone").value = ownerPhone || '';
        new bootstrap.Modal(document.getElementById("editUnitModal")).show();
    }
</script>
"@

# I also need to update the button onclick in the table
$oldOnclick = "onclick=`"openEditUnitModal(@unit.Id, '@unit.DoorNumber', '@unit.RoomLayout')`""
$newOnclick = "onclick=`"openEditUnitModal(@unit.Id, '@unit.DoorNumber', '@unit.RoomLayout', '@unit.OwnerName', '@unit.OwnerPhone')`""
$viewText = $viewText.Replace($oldOnclick, $newOnclick)

$viewText = $viewText.Replace($oldModal, $newModal)

[System.IO.File]::WriteAllText($viewPath, $viewText, [System.Text.Encoding]::UTF8)
