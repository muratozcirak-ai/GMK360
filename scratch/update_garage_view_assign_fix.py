import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<!-- Yeni Ara[^>]+ Modal[^>]+ -->'
# I will use re.sub for safety
replacement_modal = '''@foreach(var v in Model) {
    <!-- Araç Tahsis Modalı -->
    <div class="modal fade" id="assignModal-@v.Id" tabindex="-1">
        <div class="modal-dialog modal-sm modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header border-0 bg-secondary text-white">
                    <h6 class="modal-title fw-bold">Araç Sabit Tahsisi</h6>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <form asp-action="AssignVehicle" asp-controller="CompanyGarage" method="post">
                    <div class="modal-body">
                        <input type="hidden" name="vehicleId" value="@v.Id" />
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

if 'Araç Tahsis Modalı' not in content:
    content = re.sub(target, replacement_modal, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)