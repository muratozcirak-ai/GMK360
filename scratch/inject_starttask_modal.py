import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add StartTask modal next to CompleteTask modal
start_task_modal = '''
                                            <button type="button" class="btn btn-sm btn-primary rounded-circle shadow-sm me-1" title="Görevi Üstlen / Başlat (Şoför/Araç Değişimi)" data-bs-toggle="modal" data-bs-target="#startTaskModal-@task.Id">
                                                <i class="bi bi-play-fill"></i>
                                            </button>
                                            <button type="button" class="btn btn-sm btn-success rounded-circle shadow-sm" title="Tamamlandı İşaretle" data-bs-toggle="modal" data-bs-target="#completeTaskModal-@task.Id">
                                                <i class="bi bi-check-lg"></i>
                                            </button>

                                            <!-- Görevi Üstlen / Başlat Modalı -->
                                            <div class="modal fade" id="startTaskModal-@task.Id" tabindex="-1">
                                                <div class="modal-dialog modal-sm modal-dialog-centered">
                                                    <div class="modal-content border-0 shadow">
                                                        <div class="modal-header border-0 bg-primary text-white">
                                                            <h6 class="modal-title fw-bold"><i class="bi bi-play-circle me-1"></i> Görevi Başlat / Devral</h6>
                                                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                                                        </div>
                                                        <form asp-action="StartTask" asp-controller="CompanyGarage" method="post">
                                                            <div class="modal-body">
                                                                <input type="hidden" name="taskId" value="@task.Id" />
                                                                <p class="small text-muted mb-3">Bu görevi üstlenmek için başlangıç KM'sini girin. Araç arızalandıysa farklı bir araç seçerek görevi devredebilirsiniz.</p>
                                                                
                                                                <div class="mb-3">
                                                                    <label class="form-label fw-bold">Görevli Araç</label>
                                                                    <select name="vehicleId" class="form-select">
                                                                        @foreach(var v in Model) {
                                                                            if(v.Id == vehicle.Id) {
                                                                                <option value="@v.Id" selected>@v.PlateNumber</option>
                                                                            } else {
                                                                                <option value="@v.Id">@v.PlateNumber</option>
                                                                            }
                                                                        }
                                                                    </select>
                                                                </div>
                                                                <div class="mb-3">
                                                                    <label class="form-label fw-bold">Şoför Adı</label>
                                                                    <input type="text" name="driverName" class="form-control" value="@task.DriverName" required />
                                                                </div>
                                                                <div class="mb-3">
                                                                    <label class="form-label fw-bold">Araç Başlangıç KM</label>
                                                                    <input type="number" name="startKm" class="form-control" value="@vehicle.CurrentKm" required />
                                                                </div>
                                                            </div>
                                                            <div class="modal-footer border-0 pt-0">
                                                                <button type="submit" class="btn btn-primary w-100 fw-bold">Görevi Başlat</button>
                                                            </div>
                                                        </form>
                                                    </div>
                                                </div>
                                            </div>
'''

content = content.replace('''<button type="button" class="btn btn-sm btn-success rounded-circle shadow-sm" title="Tamamland aretle" data-bs-toggle="modal" data-bs-target="#completeTaskModal-@task.Id">
                                                <i class="bi bi-check-lg"></i>
                                            </button>''', start_task_modal)
content = content.replace('''<button type="button" class="btn btn-sm btn-success rounded-circle shadow-sm" title="Tamamlandı İşaretle" data-bs-toggle="modal" data-bs-target="#completeTaskModal-@task.Id">
                                                <i class="bi bi-check-lg"></i>
                                            </button>''', start_task_modal)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('StartTask modal injected to Index.cshtml.')