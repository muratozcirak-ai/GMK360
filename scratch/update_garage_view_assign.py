import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add button
target_btn = r'<!-- Aksiyon Butonlar. -->'
replacement_btn = '''<!-- Aksiyon Butonları -->
                        <div class="d-flex mb-2">
                            <button class="btn btn-sm btn-light w-100 fw-bold border text-secondary" data-bs-toggle="modal" data-bs-target="#assignModal-@vehicle.Id">
                                <i class="bi bi-person-badge me-1"></i> Sabit Tahsis Değiştir
                            </button>
                        </div>'''
content = re.sub(target_btn, replacement_btn, content)

# Add Modal
target_modal = r'<!-- Yeni Ara. Ekle Modal. -->'
replacement_modal = '''@foreach(var vehicle in Model) {
    <!-- Araç Tahsis Modalı -->
    <div class="modal fade" id="assignModal-@vehicle.Id" tabindex="-1">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header border-0 bg-secondary text-white">
                    <h6 class="modal-title fw-bold">Araç Sabit Tahsisi</h6>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <form asp-action="AssignVehicle" asp-controller="CompanyGarage" method="post">
                    <div class="modal-body">
                        <input type="hidden" name="vehicleId" value="@vehicle.Id" />
                        <div class="mb-3">
                            <label class="form-label fw-bold">Sabit Şantiye (Opsiyonel)</label>
                            <select name="projectId" class="form-select">
                                <option value="">Yok (Merkez / Havuz Aracı)</option>
                                @foreach(var p in ViewBag.Projects) {
                                    <option value="@p.Id">Şantiye: @p.Name</option>
                                }
                            </select>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">Zimmetli Personel / Sorumlu</label>
                            <input type="text" name="assignedToName" class="form-control" placeholder="Örn: Hasan Yılmaz" />
                        </div>
                    </div>
                    <div class="modal-footer border-0 pt-0">
                        <button type="submit" class="btn btn-secondary w-100 fw-bold">Tahsisi Kaydet</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
}

<!-- Yeni Araç Ekle Modalı -->'''
content = content.replace('<!-- Yeni Ara Ekle Modal -->', replacement_modal)

# Show Assignment in Card
target_badge = r'<span class="text-muted small">@vehicle.BrandModel - @vehicle.VehicleType</span>'
replacement_badge = '''<span class="text-muted small">@vehicle.BrandModel - @vehicle.VehicleType</span>
                                    @if(activeAssignment != null && activeAssignment.ProjectId.HasValue) {
                                        var pName = ((IEnumerable<dynamic>)ViewBag.Projects).FirstOrDefault(p => p.Id == activeAssignment.ProjectId.Value)?.Name;
                                        <div class="mt-1"><span class="badge bg-primary bg-opacity-10 text-primary border border-primary"><i class="bi bi-pin-angle-fill"></i> Sabit: @pName</span></div>
                                    }'''
content = content.replace(target_badge, replacement_badge)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)