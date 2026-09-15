import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\Ledger.cshtml'

content = '''@model GMK360.Core.Entities.Finance.SupplierCurrentAccount
@{
    ViewData["Title"] = "Cari Ekstrem";
    Layout = "~/Views/Shared/_PartnerLayout.cshtml";
    bool noAccount = ViewBag.NoAccount != null ? (bool)ViewBag.NoAccount : false;
}

<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h2 class="h4 fw-bold mb-1"><i class="bi bi-journal-text me-2"></i> Cari Ekstrem</h2>
        <p class="text-muted mb-0">Tüm alım, veresiye ve tahsilat dökümünüz.</p>
    </div>
</div>

@if(noAccount)
{
    <div class="card shadow-sm border-0 rounded-4">
        <div class="card-body text-center py-5 text-muted">
            <i class="bi bi-wallet2 fs-1 d-block mb-3 opacity-25"></i>
            Henüz size ait bir finansal kayıt/cari hesap oluşturulmamış.
        </div>
    </div>
}
else
{
    <div class="card shadow-sm border-0 rounded-4 mb-4">
        <div class="card-body d-flex justify-content-between align-items-center p-4">
            <div>
                <h6 class="text-muted text-uppercase fw-bold mb-1">Şirketimizdeki Güncel Bakiyeniz</h6>
                <h2 class="fw-bold text-primary mb-0">@Model.CurrentBalance.ToString("N2") ₺</h2>
            </div>
            <div>
                <button class="btn btn-outline-primary" onclick="window.print()"><i class="bi bi-printer me-2"></i>Yazdır / PDF</button>
            </div>
        </div>
    </div>

    <div class="card shadow-sm border-0 rounded-4">
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th class="ps-4 py-3">Tarih</th>
                            <th class="py-3">İşlem Tipi</th>
                            <th class="py-3">Açıklama / Proje</th>
                            <th class="text-end py-3">Tutar</th>
                            <th class="text-end pe-4 py-3">Kalan Bakiye</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach(var tx in Model.Transactions)
                        {
                            var amountClass = "";
                            var prefix = "";
                            var icon = "";
                            var typeLabel = "";

                            if(tx.Type == GMK360.Core.Entities.Finance.SupplierTransactionType.PurchaseInvoice)
                            {
                                amountClass = "text-danger"; prefix = "+"; icon = "bi-receipt text-primary"; typeLabel = "Resmi Fatura";
                            }
                            else if(tx.Type == GMK360.Core.Entities.Finance.SupplierTransactionType.OpenAccountPurchase)
                            {
                                amountClass = "text-danger"; prefix = "+"; icon = "bi-pencil text-warning"; typeLabel = "Veresiye / Açık Hesap";
                            }
                            else if(tx.Type == GMK360.Core.Entities.Finance.SupplierTransactionType.PaymentMade)
                            {
                                amountClass = "text-success"; prefix = "-"; icon = "bi-cash-coin text-success"; typeLabel = "Size Yapılan Ödeme";
                            }

                            <tr>
                                <td class="ps-4 text-muted small">@tx.TransactionDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm")</td>
                                <td>
                                    <i class="bi @icon me-2"></i>@typeLabel
                                </td>
                                <td>
                                    <div class="fw-bold">@tx.Description</div>
                                    @if(tx.Project != null)
                                    {
                                        <small class="text-muted"><i class="bi bi-geo-alt me-1"></i>@tx.Project.Name</small>
                                    }
                                </td>
                                <td class="text-end fw-bold @amountClass">@prefix @tx.Amount.ToString("N2") ₺</td>
                                <td class="text-end pe-4 fw-bold text-muted">@tx.BalanceAfterTransaction.ToString("N2") ₺</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
}
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
