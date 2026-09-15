import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Timesheet\PendingAdvances.cshtml'

content = '''@model IEnumerable<GMK360.Core.Entities.Finance.AgencyStaffAdvance>
@{
    ViewData["Title"] = "Gizli Avans Onay Ekranı";
    Layout = "~/Views/Shared/_ConstructionLayout.cshtml";
}

<div class="container-fluid mt-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
        <div>
            <h2 class="fw-bold mb-0 text-danger"><i class="bi bi-shield-lock me-2"></i> Yönetici Avans Onay Merkezi</h2>
            <p class="text-muted mb-0">Sahadan gelen işçi avans taleplerini bu ekranda onaylayıp direkt kasadan çıkışını sağlayabilirsiniz.</p>
        </div>
        <div>
            <span class="badge bg-danger rounded-pill px-3 py-2 fs-6">
                <i class="bi bi-clock-history me-1"></i> Bekleyen: @Model.Count()
            </span>
        </div>
    </div>

    @if (TempData["SuccessMessage"] != null)
    {
        <div class="alert alert-success shadow-sm">
            <i class="bi bi-check-circle me-1"></i> @TempData["SuccessMessage"]
        </div>
    }

    <!-- Table -->
    <div class="card shadow border-0 rounded-4">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th class="ps-4 py-3">Tarih</th>
                            <th class="py-3">Personel / Usta</th>
                            <th class="py-3">Talep Açıklaması</th>
                            <th class="text-end py-3">Tutar</th>
                            <th class="text-center pe-4 py-3">İşlem</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var item in Model)
                        {
                            <tr>
                                <td class="ps-4">
                                    <div class="fw-semibold">@item.RequestDate.ToLocalTime().ToString("dd.MM.yyyy")</div>
                                    <small class="text-muted">@item.RequestDate.ToLocalTime().ToString("HH:mm")</small>
                                </td>
                                <td>
                                    <div class="fw-bold">@item.Worker.FullName</div>
                                    <span class="badge bg-secondary opacity-75">@item.Worker.WorkerType</span>
                                </td>
                                <td>
                                    @item.Description
                                </td>
                                <td class="text-end fw-bold text-danger fs-5">
                                    @item.Amount.ToString("N2") ₺
                                </td>
                                <td class="text-center pe-4">
                                    <form asp-action="ApproveAdvance" method="post">
                                        <input type="hidden" name="id" value="@item.Id" />
                                        <button type="submit" class="btn btn-success btn-sm rounded-pill px-3 shadow-sm" onclick="return confirm('Bu avansı onayladığınızda Kasa\'dan otomatik olarak para çıkışı yapılacaktır. Onaylıyor musunuz?');">
                                            <i class="bi bi-check2-circle me-1"></i> Onayla ve Kasadan Düş
                                        </button>
                                    </form>
                                </td>
                            </tr>
                        }

                        @if (!Model.Any())
                        {
                            <tr>
                                <td colspan="5" class="text-center py-5 text-muted">
                                    <i class="bi bi-shield-check fs-1 text-success opacity-50 mb-3 d-block"></i>
                                    Sahadan gelen ve onay bekleyen hiçbir avans talebi bulunmuyor.
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
