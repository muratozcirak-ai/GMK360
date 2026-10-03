import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

pattern = r'<form\s+asp-action="UpdateTaskStatus"\s+asp-controller="CompanyGarage"\s+method="post">\s*<input[^>]+name="taskId"[^>]+value="@task\.Id"[^>]*>\s*<input[^>]+name="status"[^>]+value="[^"]*"[^>]*>\s*<button[^>]+><i[^>]+></i></button>\s*</form>'

new_form = '''<button type="button" class="btn btn-sm btn-success rounded-circle shadow-sm" title="Tamamlandı İşaretle" data-bs-toggle="modal" data-bs-target="#completeTaskModal-@task.Id">
                                                <i class="bi bi-check-lg"></i>
                                            </button>
                                            
                                            <!-- Görev Tamamlama Modalı (KM ve Saat Girişi) -->
                                            <div class="modal fade" id="completeTaskModal-@task.Id" tabindex="-1">
                                                <div class="modal-dialog modal-sm modal-dialog-centered">
                                                    <div class="modal-content border-0 shadow">
                                                        <div class="modal-header bg-light border-0">
                                                            <h6 class="modal-title fw-bold text-dark">Görevi Tamamla</h6>
                                                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                                        </div>
                                                        <form asp-action="UpdateTaskStatus" asp-controller="CompanyGarage" method="post">
                                                            <div class="modal-body text-start">
                                                                <input type="hidden" name="taskId" value="@task.Id" />
                                                                <input type="hidden" name="status" value="Tamamlandı" />
                                                                <p class="small text-muted mb-3">Aracın güncel kilometresi veya iş makinası çalışma saatini girerek görevi sonlandırın.</p>
                                                                <div class="mb-3">
                                                                    <label class="form-label small fw-bold text-dark">Bitiş KM (Görev Sonu)</label>
                                                                    <input type="number" name="endKm" class="form-control" placeholder="Örn: 15450" />
                                                                    <small class="text-muted" style="font-size:0.7rem;">(Başlangıç: @(task.StartKm ?? 0) KM)</small>
                                                                </div>
                                                                <div class="mb-3">
                                                                    <label class="form-label small fw-bold text-dark">Harcanan Çalışma Saati</label>
                                                                    <input type="number" step="0.5" name="workingHours" class="form-control" placeholder="Örn: 8.5" />
                                                                </div>
                                                            </div>
                                                            <div class="modal-footer border-0 p-2">
                                                                <button type="submit" class="btn btn-success w-100 fw-bold">Görevi Bitir</button>
                                                            </div>
                                                        </form>
                                                    </div>
                                                </div>
                                            </div>'''

content = re.sub(pattern, new_form, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Regex update for form complete.')