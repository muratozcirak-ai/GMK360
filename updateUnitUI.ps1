$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$oldOpenModal = "function openEditUnitModal(id, number, layout, ownerName, ownerPhone) {"
$newOpenModal = "function openEditUnitModal(id, number, layout, ownerName, ownerPhone, gross, net, facade) {"

$oldModalCall = "onclick=""openEditUnitModal(@unit.Id, '@unit.DoorNumber', '@unit.RoomLayout', '@unit.OwnerName', '@unit.OwnerPhone')"""
$newModalCall = "onclick=""openEditUnitModal(@unit.Id, '@unit.DoorNumber', '@unit.RoomLayout', '@unit.OwnerName', '@unit.OwnerPhone', '@unit.GrossSquareMeters', '@unit.NetSquareMeters', '@unit.FacadeDirection')"""

$text = $text.Replace($oldOpenModal, $newOpenModal)
$text = $text.Replace($oldModalCall, $newModalCall)

$oldModalFields = @"
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Mal Sahibi (Ad Soyad)</label>
"@

$newModalFields = @"
                        <div class="col-md-4 mb-3">
                            <label class="form-label fw-bold small">Brüt m²</label>
                            <input type="number" step="0.1" name="GrossSquareMeters" id="editUnitGross" class="form-control" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <label class="form-label fw-bold small">Net m²</label>
                            <input type="number" step="0.1" name="NetSquareMeters" id="editUnitNet" class="form-control" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <label class="form-label fw-bold small">Cephe</label>
                            <input type="text" name="FacadeDirection" id="editUnitFacade" class="form-control" placeholder="Örn: Güney" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Mal Sahibi (Ad Soyad)</label>
"@

$text = $text.Replace($oldModalFields, $newModalFields)

$oldJsBody = @"
        document.getElementById("editUnitOwnerName").value = ownerName || '';
        document.getElementById("editUnitOwnerPhone").value = ownerPhone || '';
"@
$newJsBody = @"
        document.getElementById("editUnitOwnerName").value = ownerName || '';
        document.getElementById("editUnitOwnerPhone").value = ownerPhone || '';
        document.getElementById("editUnitGross").value = gross || '';
        document.getElementById("editUnitNet").value = net || '';
        document.getElementById("editUnitFacade").value = facade || '';
"@
$text = $text.Replace($oldJsBody, $newJsBody)

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
