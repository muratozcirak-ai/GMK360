import codecs

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add enctype and file input to AddExpense
content = content.replace('<form asp-action="AddExpense" asp-controller="CompanyGarage" method="post">', '<form asp-action="AddExpense" asp-controller="CompanyGarage" method="post" enctype="multipart/form-data">')

expense_file_input = '''<div class="col-12">
                                    <label class="form-label fw-bold text-dark">Fiş / Fatura Fotoğrafı (Kameradan Çek / Yükle)</label>
                                    <input type="file" name="photo" class="form-control" accept="image/*" capture="environment" />
                                    <small class="text-muted d-block mt-1" style="font-size:0.75rem;"><i class="bi bi-camera"></i> Telefondan direkt fotoğraf çekebilirsiniz.</small>
                                </div>
                                <div class="col-12 text-end mt-4">'''
content = content.replace('<div class="col-12 text-end mt-4">', expense_file_input)


# Add startTime and endTime to CreateTask
task_time_inputs = '''<div class="col-md-6">
                                    <label class="form-label fw-bold text-dark">Başlangıç Saati</label>
                                    <input type="time" name="startTime" class="form-control" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold text-dark">Bitiş Saati</label>
                                    <input type="time" name="endTime" class="form-control" />
                                </div>
                                <div class="col-12 text-end mt-4">'''
# Wait, let's find the closing div of CreateTask body.
# Actually I can replace <div class="col-12 text-end mt-4"> but the first one is AddVehicle.
# Let's use a unique string for CreateTask form.
content = content.replace('''<label class="form-label fw-bold">Görev Tarihi</label>
                                    <input type="date" name="taskDate" class="form-control" required value="@DateTime.Today.ToString("yyyy-MM-dd")" />
                                </div>''', '''<label class="form-label fw-bold">Görev Tarihi</label>
                                    <input type="date" name="taskDate" class="form-control" required value="@DateTime.Today.ToString("yyyy-MM-dd")" />
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold">Saat Aralığı (Örn: 09:00 - 12:00)</label>
                                    <div class="input-group">
                                        <input type="time" name="startTime" class="form-control" />
                                        <span class="input-group-text">-</span>
                                        <input type="time" name="endTime" class="form-control" />
                                    </div>
                                </div>''')

# Add 'Tüm Görevler' button at the top right
content = content.replace('''<button class="btn btn-primary fw-bold rounded-pill px-4 shadow-sm" data-bs-toggle="modal" data-bs-target="#addVehicleModal">
                <i class="bi bi-plus-lg me-1"></i> Yeni Araç Ekle
            </button>''', '''<a href="/CompanyGarage/AllTasks" class="btn btn-outline-primary fw-bold rounded-pill px-4 shadow-sm me-2">
                <i class="bi bi-list-task me-1"></i> Tüm Görev Listesi
            </a>
            <button class="btn btn-primary fw-bold rounded-pill px-4 shadow-sm" data-bs-toggle="modal" data-bs-target="#addVehicleModal">
                <i class="bi bi-plus-lg me-1"></i> Yeni Araç Ekle
            </button>''')

# Fix "Masraf Gör" link inside the card
# The card has "Masraf İşle", I'll turn "Toplam Araç Masrafı" into a clickable link.
# <span class="text-danger fw-bold">@totalExpense.ToString("N2") ₺</span>
content = content.replace('''<span class="text-danger fw-bold">@totalExpense.ToString("N2") ₺</span>''', '''<a href="/CompanyGarage/VehicleExpenses/@vehicle.Id" class="text-danger fw-bold text-decoration-none">@totalExpense.ToString("N2") ₺ <i class="bi bi-arrow-right-short"></i></a>''')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated Index View UI')