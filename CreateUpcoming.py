import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Finance\UpcomingPayments.cshtml'

content = '''@model IEnumerable<GMK360.Core.Entities.Finance.SupplierPayment>
@{
    ViewData["Title"] = "Vadesi Yaklaşan Ödemeler";
    Layout = "~/Views/Shared/_ConstructionLayout.cshtml";
}

<div class="container-fluid mt-4">
    <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
        <div>
            <h2 class="fw-bold mb-0 text-dark"><i class="bi bi-calendar-event text-danger me-2"></i> Vadesi Yaklaşan Ödemeler</h2>
            <p class="text-muted mb-0">Tedarikçi ve taşeronlara verilmiş, ödeme tarihi bekleyen çek ve senetler.</p>
        </div>
    </div>

    <div class="card shadow-sm border-0 rounded-4">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th class="ps-4 py-3">Firma / Alacaklı</th>
                            <th class="py-3">Vade Tarihi</th>
                            <th class="py-3">Ödeme Türü</th>
                            <th class="py-3">Belge/Çek No</th>
                            <th class="text-end pe-4 py-3">Tutar</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var p in Model)
                        {
                            var isOverdue = p.DueDate.HasValue && p.DueDate.Value < DateTime.UtcNow;
                            <tr class="@(isOverdue ? "bg-danger bg-opacity-10" : "")">
                                <td class="ps-4 fw-bold">@p.SupplierCurrentAccount.PhonebookContact.Name</td>
                                <td>
                                    @if(p.DueDate.HasValue)
                                    {
                                        <span class="@(isOverdue ? "text-danger fw-bold" : "text-dark")">
                                            @if(isOverdue) { <i class="bi bi-exclamation-circle me-1"></i> }
                                            @p.DueDate.Value.ToLocalTime().ToString("dd.MM.yyyy")
                                        </span>
                                    }
                                    else
                                    {
                                        <span class="text-muted">-</span>
                                    }
                                </td>
                                <td><span class="badge bg-secondary">@p.Method.ToString()</span></td>
                                <td><small class="text-muted">@p.CheckNumber</small></td>
                                <td class="text-end pe-4 fw-bold text-danger fs-5">@p.Amount.ToString("N2") ₺</td>
                            </tr>
                        }

                        @if (!Model.Any())
                        {
                            <tr>
                                <td colspan="5" class="text-center py-5 text-muted">
                                    <i class="bi bi-check-circle fs-1 text-success opacity-50 mb-3 d-block"></i>
                                    Vadesi yaklaşan veya gecikmiş bekleyen bir ödemeniz bulunmuyor.
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
