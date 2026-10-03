import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# I need to insert a collapsible TR after the main TR.
# The main TR ends with </tr>
# First I'll find the main TR tag and add a toggle button in the first TD if there are quotes.

target_tr_start = r'<tr class="align-middle"(.*?)>'
replacement_tr_start = '''<tr class="align-middle">'''
# Actually, let's just find the end of the TR.
target_tr_end = r'</td>\s*</tr>'
replacement_tr_end = '''</td>
                                        </tr>
                                        @if (relatedQuote != null && relatedQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))
                                        {
                                            <tr class="bg-light">
                                                <td colspan="7" class="p-3 border-start border-4 border-info">
                                                    <div class="d-flex align-items-center mb-2">
                                                        <i class="bi bi-arrow-return-right me-2 text-info"></i>
                                                        <strong class="text-secondary">Alınan Fiyat Teklifleri (Fizibiliteye Eklenebilir)</strong>
                                                    </div>
                                                    <div class="row g-3">
                                                        @foreach(var inv in relatedQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted).OrderBy(i => i.OfferedPrice))
                                                        {
                                                            <div class="col-md-4">
                                                                <div class="card border border-info shadow-sm">
                                                                    <div class="card-body p-2 d-flex justify-content-between align-items-center">
                                                                        <div>
                                                                            <h6 class="mb-0 text-dark fw-bold">@inv.NetworkContact?.CompanyName</h6>
                                                                            <span class="text-success fw-bold fs-6">@inv.OfferedPrice?.ToString("C2")</span>
                                                                        </div>
                                                                        <form asp-action="AcceptQuote" method="post">
                                                                            <input type="hidden" name="documentId" value="@doc.Id" />
                                                                            <input type="hidden" name="inviteId" value="@inv.Id" />
                                                                            <button type="submit" class="btn btn-sm btn-info text-white fw-bold" title="Fizibilite Olarak Seç">
                                                                                <i class="bi bi-check-circle-fill"></i> Seç
                                                                            </button>
                                                                        </form>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        }
                                                    </div>
                                                </td>
                                            </tr>
                                        }'''

content = re.sub(target_tr_end, replacement_tr_end, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)