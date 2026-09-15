import codecs
import os

folder = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal'
os.makedirs(folder, exist_ok=True)

workers_html = '''@model IEnumerable<GMK360.Core.Entities.Construction.AgencyWorker>
@{
    ViewData["Title"] = "Ekiplerim";
    Layout = "_PartnerLayout";
}

<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h2 class="fw-bold mb-1"><i class="bi bi-people text-primary me-2"></i>Ekiplerim (Personeller)</h2>
        <p class="text-muted mb-0">Şantiyeye getirdiğiniz işçilerin listesi.</p>
    </div>
    <button class="btn btn-primary rounded-pill px-4" data-bs-toggle="modal" data-bs-target="#addWorkerModal">
        <i class="bi bi-plus-lg me-1"></i> Yeni Personel Ekle
    </button>
</div>

<div class="card shadow-sm border-0">
    <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
                <tr>
                    <th class="ps-4">Ad Soyad</th>
                    <th>TC Kimlik No</th>
                    <th>Telefon</th>
                    <th>Meslek</th>
                    <th>Durum</th>
                </tr>
            </thead>
            <tbody>
                @foreach(var w in Model)
                {
                    <tr>
                        <td class="ps-4 fw-bold">@w.FullName</td>
                        <td>@w.IdentityNumber</td>
                        <td>@w.PhoneNumber</td>
                        <td><span class="badge bg-secondary">@w.Profession</span></td>
                        <td><span class="badge bg-success">Aktif</span></td>
                    </tr>
                }
                @if(!Model.Any()){ <tr><td colspan="5" class="text-center text-muted py-4">Henüz kayıtlı personeliniz bulunmuyor.</td></tr> }
            </tbody>
        </table>
    </div>
</div>

<!-- Yeni Personel Modal -->
<div class="modal fade" id="addWorkerModal" tabindex="-1">
    <div class="modal-dialog">
        <form asp-action="AddWorker" method="post" class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title fw-bold">Yeni Personel Ekle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">Ad</label>
                    <input type="text" name="FirstName" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">Soyad</label>
                    <input type="text" name="LastName" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">TC Kimlik No</label>
                    <input type="text" name="IdentityNumber" class="form-control" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Telefon</label>
                    <input type="text" name="PhoneNumber" class="form-control" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Meslek / Uzmanlık</label>
                    <input type="text" name="Profession" class="form-control" placeholder="Örn: Kalıpçı, Demirci" required />
                </div>
            </div>
            <div class="modal-footer">
                <button type="submit" class="btn btn-primary px-4 rounded-pill">Kaydet</button>
            </div>
        </form>
    </div>
</div>
'''
with codecs.open(os.path.join(folder, 'Workers.cshtml'), 'w', 'utf-8-sig') as f:
    f.write(workers_html)


timesheets_html = '''@{
    ViewData["Title"] = "Kör Puantaj";
    Layout = "_PartnerLayout";
    
    var workers = ViewBag.Workers as IEnumerable<GMK360.Core.Entities.Construction.AgencyWorker>;
    var existingTimesheets = ViewBag.ExistingTimesheets as Dictionary<int, GMK360.Core.Entities.Construction.DailyTimesheet>;
    var projects = ViewBag.Projects as IEnumerable<GMK360.Core.Entities.Construction.ConstructionProject>;
    var targetDate = (DateTime)ViewBag.TargetDate;
}

<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h2 class="fw-bold mb-1"><i class="bi bi-calendar-check text-primary me-2"></i>Kör Puantaj (Yoklama)</h2>
        <p class="text-muted mb-0">Ekiplerinizin günlük devam durumunu işaretleyin. Ücret bilgileri merkez tarafından hesaplanır.</p>
    </div>
</div>

<div class="card shadow-sm border-0 mb-4">
    <div class="card-body">
        <form method="get" class="row gx-2 gy-2 align-items-center">
            <div class="col-auto">
                <label class="col-form-label fw-bold">Tarih:</label>
            </div>
            <div class="col-auto">
                <input type="date" name="date" class="form-control" value="@targetDate.ToString("yyyy-MM-dd")" onchange="this.form.submit()" />
            </div>
            <div class="col-auto ms-3">
                <label class="col-form-label fw-bold">Şantiye:</label>
            </div>
            <div class="col-auto">
                <select name="projectId" class="form-select" onchange="this.form.submit()" required>
                    <option value="">-- Şantiye Seçin --</option>
                    @if(projects != null) {
                        foreach (var proj in projects)
                        {
                            var isSel = ViewBag.CurrentProjectId == proj.Id ? "selected" : "";
                            <option value="@proj.Id" selected="@isSel">@proj.Name</option>
                        }
                    }
                </select>
            </div>
        </form>
    </div>
</div>

@if(ViewBag.CurrentProjectId != null)
{
<form asp-action="SaveTimesheets" method="post">
    <input type="hidden" name="projectId" value="@ViewBag.CurrentProjectId" />
    <input type="hidden" name="targetDate" value="@targetDate.ToString("yyyy-MM-dd")" />
    
    <div class="card shadow-sm border-0 mb-3">
        <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
                <thead class="table-light">
                    <tr>
                        <th class="ps-4">Personel / Usta</th>
                        <th>Meslek</th>
                        <th style="width: 250px;">Yoklama Durumu</th>
                        <th>Notlar</th>
                    </tr>
                </thead>
                <tbody>
                    @if(workers != null) {
                        foreach (var worker in workers)
                        {
                            var t = existingTimesheets != null && existingTimesheets.ContainsKey(worker.Id) ? existingTimesheets[worker.Id] : null;
                            var status = t != null ? t.AttendanceStatus : "Gelmedi";
                            var notes = t?.Notes ?? "";

                            <tr>
                                <td class="ps-4 fw-bold">
                                    <input type="hidden" name="workerIds" value="@worker.Id" />
                                    @worker.FullName
                                </td>
                                <td>@worker.Profession</td>
                                <td>
                                    <select name="statuses" class="form-select">
                                        <option value="Gelmedi" selected="@(status == "Gelmedi" ? "selected" : null)">Gelmedi</option>
                                        <option value="Tam Gün" selected="@(status == "Tam Gün" ? "selected" : null)">Tam Gün (Çalıştı)</option>
                                        <option value="Yarım Gün" selected="@(status == "Yarım Gün" ? "selected" : null)">Yarım Gün</option>
                                    </select>
                                </td>
                                <td>
                                    <input type="text" name="notesList" class="form-control" value="@notes" placeholder="Görev veya notlar..." />
                                </td>
                            </tr>
                        }
                    }
                    @if (workers == null || !workers.Any())
                    {
                        <tr><td colspan="4" class="text-center py-4 text-muted">Kayıtlı personeliniz bulunmuyor.</td></tr>
                    }
                </tbody>
            </table>
        </div>
        <div class="card-footer bg-white text-end py-3">
            <button type="submit" class="btn btn-primary px-4 rounded-pill">Puantajı Kaydet</button>
        </div>
    </div>
</form>
}
else
{
    <div class="alert alert-info shadow-sm border-0"><i class="bi bi-info-circle me-2"></i>Puantaj girmek için yukarıdan şantiye seçiniz.</div>
}
'''
with codecs.open(os.path.join(folder, 'Timesheets.cshtml'), 'w', 'utf-8-sig') as f:
    f.write(timesheets_html)
