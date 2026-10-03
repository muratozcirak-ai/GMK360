import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# 1. Add KM and Hours badges
target_badge = r'<span class="badge bg-@statusColor rounded-pill">@statusText</span>'
replacement_badge = '''<div class="text-end">
                                <span class="badge bg-@statusColor rounded-pill mb-1">@statusText</span><br/>
                                <span class="badge bg-secondary" title="Güncel Kilometre"><i class="bi bi-speedometer2"></i> @vehicle.CurrentKm KM</span>
                                <span class="badge bg-info text-dark" title="Çalışma Saati (İş Makinası)"><i class="bi bi-clock-history"></i> @vehicle.CurrentWorkingHours Saat</span>
                            </div>'''
content = re.sub(target_badge, replacement_badge, content)

# 2. Add enctype to form
content = re.sub(r'<form asp-action="AddExpense" asp-controller="CompanyGarage" method="post">', r'<form asp-action="AddExpense" asp-controller="CompanyGarage" method="post" enctype="multipart/form-data">', content)

# 3. Add file input and odometer to AddExpense
target_expense = r'<div class="col-12">\s*<label class="form-label fw-bold">Masraf T[^<]+</label>'
replacement_expense = '''<div class="col-12">
                                    <label class="form-label fw-bold text-dark">Fiş / Fatura Fotoğrafı (Kameradan Çek / Yükle)</label>
                                    <input type="file" name="photo" class="form-control" accept="image/*" capture="environment" />
                                    <small class="text-muted d-block mt-1" style="font-size:0.75rem;"><i class="bi bi-camera"></i> Telefondan direkt fotoğraf çekebilirsiniz.</small>
                                </div>
                                <div class="col-12">
                                    <label class="form-label fw-bold text-dark">Fiş Anındaki Kilometre <span class="text-muted fw-normal">(İsteğe Bağlı)</span></label>
                                    <input type="number" name="odometer" class="form-control" placeholder="Örn: 15400" />
                                </div>
                                <div class="col-12">
                                    <label class="form-label fw-bold">Masraf Türü</label>'''
content = re.sub(target_expense, replacement_expense, content)

# 4. Add time inputs to CreateTask
target_taskdate = r'<label class="form-label fw-bold">G[^<]+Tarihi</label>\s*<input type="date" name="taskDate"[^>]+>\s*</div>'
replacement_taskdate = '''<label class="form-label fw-bold">Görev Tarihi</label>
                                    <input type="date" name="taskDate" class="form-control" required value="@DateTime.Today.ToString("yyyy-MM-dd")" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold">Saat Aralığı (Örn: 09:00 - 12:00)</label>
                                    <div class="input-group">
                                        <input type="time" name="startTime" class="form-control" />
                                        <span class="input-group-text">-</span>
                                        <input type="time" name="endTime" class="form-control" />
                                    </div>
                                </div>'''
content = re.sub(target_taskdate, replacement_taskdate, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated Index.cshtml successfully with regex.')