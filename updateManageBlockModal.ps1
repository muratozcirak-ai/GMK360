$content = Get-Content -Raw "GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"

# Replace the green button
$oldBtn = '<button type="button" class="btn btn-sm btn-success rounded-pill px-3 me-2" data-bs-toggle="modal" data-bs-target="#addUnitModal">'
$newBtn = '<button type="button" class="btn btn-sm btn-success rounded-pill px-3 me-2 btn-add-global" data-bs-toggle="modal" data-bs-target="#editBlockSkeletonModal">'
$content = $content.Replace($oldBtn, $newBtn)

$oldText = '<i class="bi bi-plus-circle me-1"></i> Yeni Kat / Birim Ekle'
$newText = '<i class="bi bi-building-up me-1"></i> Bina İskeletini Güncelle (Kat Ekle/Çıkar)'
$content = $content.Replace($oldText, $newText)

# Append the modal before scripts
$modalHtml = @"
<!-- Edit Block Skeleton Modal -->
<div class="modal fade" id="editBlockSkeletonModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content rounded-4 border-0 shadow">
            <form asp-action="UpdateBlockSkeleton" method="post">
                <input type="hidden" name="buildingId" value="@Model.Id" />
                
                <div class="modal-header border-0 pb-0">
                    <h5 class="modal-title fw-bold"><i class="bi bi-building-up text-success me-2"></i>Bina İskeletini Güncelle</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="alert alert-warning py-2 small mb-4">
                        <i class="bi bi-exclamation-triangle me-1"></i> Kat sayılarını değiştirdiğinizde binanın kat yapısı yeniden çizilir. Mevcut daireler silinmez ancak tanımlanan kat aralığı dışına çıkan katlar (örn: 9'dan 8'e düşürülürse 9. kat) listede görünmez.
                    </div>
                    
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Normal Kat Sayısı</label>
                            <input type="number" name="TotalFloors" class="form-control" value="@Model.TotalFloors" min="0" required />
                        </div>
                        <div class="col-md-6 mb-3">
                            <label class="form-label fw-bold small">Bodrum Kat Sayısı</label>
                            <input type="number" name="BasementFloors" class="form-control" value="@Model.BasementFloors" min="0" required />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <div class="form-check form-switch mt-2">
                                <input class="form-check-input" type="checkbox" id="hasGroundSwitch" name="HasGroundFloor" value="true" @(Model.HasGroundFloor ? "checked" : "")>
                                <label class="form-check-label fw-bold small" for="hasGroundSwitch">Zemin Kat Var</label>
                            </div>
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="form-check form-switch mt-2">
                                <input class="form-check-input" type="checkbox" id="hasRoofSwitch" name="HasRoof" value="true" @(Model.HasRoof ? "checked" : "")>
                                <label class="form-check-label fw-bold small" for="hasRoofSwitch">Çatı Katı Var</label>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-0 pt-0">
                    <button type="button" class="btn btn-light rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-success rounded-pill px-4">İskeleti Güncelle</button>
                </div>
            </form>
        </div>
    </div>
</div>
"@

$content = $content.Replace("@section Scripts {", $modalHtml + "`r`n`r`n@section Scripts {")

$content | Set-Content "GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml" -Encoding UTF8
Write-Output "Modal updated"
