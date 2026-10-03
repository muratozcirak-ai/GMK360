import codecs

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace vehicle status badge with KM and Hours info
target_badge = '''<span class="badge bg-success ms-2">@vehicle.Status</span>'''
new_badge = '''<span class="badge bg-success ms-2">@vehicle.Status</span>
                                    <span class="badge bg-secondary ms-2" title="Güncel Kilometre"><i class="bi bi-speedometer2"></i> @vehicle.CurrentKm KM</span>
                                    <span class="badge bg-info ms-2" title="Çalışma Saati (İş Makinası)"><i class="bi bi-clock-history"></i> @vehicle.CurrentWorkingHours Saat</span>'''
content = content.replace(target_badge, new_badge)

# Update the complete task form to include KM and Hours inputs
target_complete_form = '''<form asp-action="UpdateTaskStatus" asp-controller="CompanyGarage" method="post">
                                                <input type="hidden" name="taskId" value="@task.Id" />
                                                <input type="hidden" name="status" value="Tamamland" />
                                                <button type="submit" class="btn btn-sm btn-outline-success border-0" title="Tamamland aretle">
                                                    <i class="bi bi-check-circle-fill"></i>
                                                </button>
                                            </form>'''

new_complete_form = '''<button type="button" class="btn btn-sm btn-outline-success border-0" title="Tamamlandı İşaretle" data-bs-toggle="modal" data-bs-target="#completeTaskModal-@task.Id">
                                                <i class="bi bi-check-circle-fill"></i>
                                            </button>
                                            
                                            <!-- Görev Tamamlama Modalı (KM ve Saat Girişi) -->
                                            <div class="modal fade" id="completeTaskModal-@task.Id" tabindex="-1">
                                                <div class="modal-dialog modal-sm modal-dialog-centered">
                                                    <div class="modal-content border-0 shadow">
                                                        <div class="modal-header bg-light border-0">
                                                            <h6 class="modal-title fw-bold">Görevi Tamamla</h6>
                                                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                                        </div>
                                                        <form asp-action="UpdateTaskStatus" asp-controller="CompanyGarage" method="post">
                                                            <div class="modal-body">
                                                                <input type="hidden" name="taskId" value="@task.Id" />
                                                                <input type="hidden" name="status" value="Tamamlandı" />
                                                                <p class="small text-muted mb-3">Aracın güncel kilometresi veya iş makinası çalışma saatini girerek görevi sonlandırın.</p>
                                                                <div class="mb-3">
                                                                    <label class="form-label small fw-bold">Bitiş KM (Görev Sonu)</label>
                                                                    <input type="number" name="endKm" class="form-control" placeholder="Örn: 15450" />
                                                                    <small class="text-muted" style="font-size:0.7rem;">(Başlangıç: @(task.StartKm ?? 0) KM)</small>
                                                                </div>
                                                                <div class="mb-3">
                                                                    <label class="form-label small fw-bold">Harcanan Çalışma Saati</label>
                                                                    <input type="number" step="0.5" name="workingHours" class="form-control" placeholder="Örn: 8.5" />
                                                                </div>
                                                            </div>
                                                            <div class="modal-footer border-0">
                                                                <button type="submit" class="btn btn-success w-100 fw-bold">Görevi Bitir</button>
                                                            </div>
                                                        </form>
                                                    </div>
                                                </div>
                                            </div>'''
content = content.replace(target_complete_form, new_complete_form)
content = content.replace('Tamamland', 'Tamamlandı')
content = content.replace('aretle', 'İşaretle')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated Garage View with KM forms.')