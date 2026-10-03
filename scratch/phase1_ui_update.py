import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<td class="text-end pe-4">\s*@if \(item\.QuoteStatus == GMK360\.Core\.Entities\.Construction\.BudgetQuoteStatus\.WaitingForPrice\)\s*\{\s*<button class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2"><i class="bi bi-briefcase"></i> Teklif İste</button>\s*\}\s*else\s*\{\s*<button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2"><i class="bi bi-pencil"></i> Yönet</button>\s*\}\s*<form action="/PhaseOne/DeleteItem/@item\.Id" method="post" class="d-inline">\s*<button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>\s*</form>\s*</td>'
replacement = '''<td class="text-end pe-4">
                                                @{
                                                    var relatedQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == item.Id);
                                                }
                                                @if (relatedQuote != null)
                                                {
                                                    <a href="/B2BPurchasing/Details/@relatedQuote.Id" class="btn btn-sm btn-info rounded-pill px-3 me-2 text-white fw-bold"><i class="bi bi-box-arrow-up-right"></i> İhaleye Git</a>
                                                }
                                                else if (item.QuoteStatus == GMK360.Core.Entities.Construction.BudgetQuoteStatus.WaitingForPrice)
                                                {
                                                    <form action="/PhaseOne/RequestQuote/@item.Id" method="post" class="d-inline">
                                                        <button type="submit" class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2"><i class="bi bi-briefcase"></i> Teklif İste</button>
                                                    </form>
                                                }
                                                else
                                                {
                                                    <button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName', '@item.PlannedUnitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description')"><i class="bi bi-pencil"></i> Yönet</button>
                                                }
                                                <form action="/PhaseOne/DeleteItem/@item.Id" method="post" class="d-inline">
                                                    <button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>
                                                </form>
                                            </td>
                                        </tr>
                                        @if (relatedQuote != null && relatedQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))
                                        {
                                            <tr class="bg-light">
                                                <td colspan="4" class="p-3 border-start border-4 border-info">
                                                    <strong class="text-secondary d-block mb-2">Gelen B2B Teklifleri (Fizibiliteye Eklenebilir)</strong>
                                                    <div class="row g-2">
                                                        @foreach(var inv in relatedQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted || i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Accepted).OrderBy(i => i.OfferedPrice))
                                                        {
                                                            <div class="col-md-4">
                                                                <div class="card border border-info shadow-sm h-100">
                                                                    <div class="card-body p-2 d-flex flex-column">
                                                                        <h6 class="mb-1 text-dark fw-bold" style="font-size:0.85rem;">@inv.NetworkContact?.CompanyName</h6>
                                                                        <div class="d-flex justify-content-between align-items-center mt-auto">
                                                                            <span class="text-primary fw-bold">@inv.OfferedPrice?.ToString("N2") ₺</span>
                                                                            @if(inv.IsFeasibilitySelected) {
                                                                                <span class="badge bg-primary"><i class="bi bi-star-fill text-warning"></i> FİZİBİLİTEDE KULLANILDI</span>
                                                                            } else {
                                                                                <form action="/PhaseOne/AcceptQuote" method="post" class="d-inline">
                                                                                    <input type="hidden" name="inviteId" value="@inv.Id" />
                                                                                    <input type="hidden" name="budgetItemId" value="@item.Id" />
                                                                                    <button type="submit" class="btn btn-sm btn-outline-primary rounded-pill" style="font-size:0.75rem;">Fizibilite Seç</button>
                                                                                </form>
                                                                            }
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        }
                                                    </div>
                                                </td>'''
# Wait, replacing across '</tr>' is tricky since I didn't include it in target. Let's just use a precise regex or append logic.
content = re.sub(target, replacement, content, flags=re.DOTALL)

target_modal = r'<!-- Yeni Kalem Ekle Modal -->'
replacement_modal = '''<!-- Yönet Modal -->
<div class="modal fade" id="manageModal" tabindex="-1">
    <div class="modal-dialog">
        <form action="/PhaseOne/UpdatePrice" method="post" class="modal-content">
            <input type="hidden" name="id" id="manageId" />
            
            <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold text-dark" id="manageTitle">Bütçe Kalemini Yönet</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label fw-bold text-dark">Tahmini / Sabit Tutar (₺)</label>
                    <div class="input-group">
                        <input type="number" step="0.01" name="plannedUnitPrice" id="managePrice" class="form-control" required />
                        <span class="input-group-text">₺</span>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-dark">Açıklama / Notlar</label>
                    <textarea name="description" id="manageDesc" class="form-control" rows="2"></textarea>
                </div>
            </div>
            <div class="modal-footer border-0">
                <button type="button" class="btn btn-light" data-bs-dismiss="modal">İptal</button>
                <button type="submit" class="btn btn-primary px-4">Kaydet</button>
            </div>
        </form>
    </div>
</div>

<!-- Yeni Kalem Ekle Modal -->'''

content = re.sub(target_modal, replacement_modal, content)

target_script = r'function openAddModal\(categoryName\) \{'
replacement_script = '''function openManageModal(id, title, price, desc) {
        document.getElementById('manageId').value = id;
        document.getElementById('manageTitle').innerText = title;
        document.getElementById('managePrice').value = price;
        document.getElementById('manageDesc').value = desc !== 'null' ? desc : '';
        new bootstrap.Modal(document.getElementById('manageModal')).show();
    }

    function openAddModal(categoryName) {'''

content = re.sub(target_script, replacement_script, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)