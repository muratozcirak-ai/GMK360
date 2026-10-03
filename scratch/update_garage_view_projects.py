import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add DestinationProjectId dropdown to taskModal
target_task_modal = r'<div class="mb-3">\s*<label class="form-label fw-bold">Şoför / Personel Adı</label>'
replacement_task_modal = '''<div class="mb-3">
                                    <label class="form-label fw-bold text-dark">Görev Yeri (Şirket / Şantiye)</label>
                                    <select name="destinationProjectId" class="form-select">
                                        <option value="">Şirket İçi (Merkez / Genel)</option>
                                        @foreach(var p in ViewBag.Projects) {
                                            <option value="@p.Id">Şantiye: @p.Name</option>
                                        }
                                    </select>
                                </div>
                                <div class="mb-3">
                                    <label class="form-label fw-bold">Şoför / Personel Adı</label>'''
content = re.sub(target_task_modal, replacement_task_modal, content)

# Show it in the Active Tasks list
target_task_li = r'<small class="text-muted d-block"><i class="bi bi-person me-1"></i> Şoför: @task.DriverName</small>'
replacement_task_li = '''<small class="text-muted d-block"><i class="bi bi-person me-1"></i> Şoför: @task.DriverName</small>
                                                @if(task.DestinationProjectId.HasValue) {
                                                    var pName = ((IEnumerable<dynamic>)ViewBag.Projects).FirstOrDefault(p => p.Id == task.DestinationProjectId.Value)?.Name;
                                                    <span class="badge bg-primary mb-1"><i class="bi bi-building"></i> Şantiye: @pName</span><br/>
                                                } else {
                                                    <span class="badge bg-secondary mb-1"><i class="bi bi-building"></i> Şirket Merkez</span><br/>
                                                }'''
content = content.replace(target_task_li, replacement_task_li)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Garage Index view updated for Task Destination.')