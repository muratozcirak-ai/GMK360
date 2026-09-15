import codecs
import os

folder = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal'
contracts_html = '''@model IEnumerable<GMK360.Core.Entities.Finance.SubcontractorContract>
@{
    ViewData["Title"] = "Sözleşme ve Hakedişlerim";
    Layout = "_PartnerLayout";
}

<div class="d-flex justify-content-between align-items-center mb-4">
    <div>
        <h2 class="fw-bold mb-1"><i class="bi bi-file-earmark-check text-primary me-2"></i>Sözleşme ve Hakedişlerim</h2>
        <p class="text-muted mb-0">Şantiyelerdeki taşeronluk sözleşmeleriniz ve merkezin onayladığı hakediş tutarları.</p>
    </div>
</div>

<div class="row g-4">
    @foreach(var contract in Model)
    {
        <div class="col-12">
            <div class="card shadow-sm border-0">
                <div class="card-header bg-light d-flex justify-content-between align-items-center py-3">
                    <h5 class="fw-bold mb-0 text-dark"><i class="bi bi-building me-2"></i>@contract.Project?.Name - @contract.WorkDescription</h5>
                    <span class="badge bg-success">@contract.Status</span>
                </div>
                <div class="card-body">
                    <div class="row mb-4">
                        <div class="col-md-3">
                            <small class="text-muted d-block">Sözleşme Tarihi</small>
                            <span class="fw-bold">@contract.ContractDate.ToShortDateString()</span>
                        </div>
                        <div class="col-md-3">
                            <small class="text-muted d-block">Toplam Tutar</small>
                            <span class="fw-bold text-dark">@contract.TotalAmount.ToString("C2")</span>
                        </div>
                    </div>
                    
                    <h6 class="fw-bold text-primary mb-3">Onaylı Hakedişler</h6>
                    <div class="table-responsive">
                        <table class="table table-bordered table-sm align-middle">
                            <thead class="table-light">
                                <tr>
                                    <th>Dönem / Açıklama</th>
                                    <th>Onay Tarihi</th>
                                    <th class="text-end">Tutar (TL)</th>
                                    <th>Durum</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach(var hakedis in contract.ProgressPayments)
                                {
                                    <tr>
                                        <td>@hakedis.PeriodEndDate.ToString("MMMM yyyy") Hakedişi - @hakedis.Description</td>
                                        <td>@hakedis.CreatedAt.ToShortDateString()</td>
                                        <td class="text-end fw-bold">@hakedis.NetPayableAmount.ToString("C2")</td>
                                        <td><span class="badge bg-success">Onaylandı</span></td>
                                    </tr>
                                }
                                @if(!contract.ProgressPayments.Any()){ <tr><td colspan="4" class="text-center text-muted">Henüz onaylanmış hakediş kaydı bulunmuyor.</td></tr> }
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    }
    @if(!Model.Any()){ <div class="col-12"><div class="alert alert-info border-0 shadow-sm text-center py-4"><i class="bi bi-info-circle me-2"></i>Aktif bir sözleşmeniz bulunmuyor.</div></div> }
</div>
'''

with codecs.open(os.path.join(folder, 'Contracts.cshtml'), 'w', 'utf-8-sig') as f:
    f.write(contracts_html)
