import io
import re
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

cost_injection = """                                            <td class="text-end fw-bold" title="Harç: @(doc.DocumentFee?.ToString("N2") ?? "0,00") | Ek Masraf: @(doc.AdditionalCost?.ToString("N2") ?? "0,00")">
                                                @if (submittedQuotes > 0 && relatedQuote != null)
                                                {
                                                    var bestPrice = relatedQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted).Min(i => i.OfferedPrice);
                                                    <span class="text-success"><i class="bi bi-check-circle-fill"></i> @bestPrice.ToString("N2") ₺</span>
                                                }
                                                else
                                                {
                                                    @((doc.DocumentFee.GetValueOrDefault() + doc.AdditionalCost.GetValueOrDefault()).ToString("N2")) <span>₺</span>
                                                }
                                            </td>"""

lines = content.split('\n')
out_lines = []
in_cost = False
for line in lines:
    if '<td class="text-end fw-bold" title="Har' in line:
        out_lines.append(cost_injection)
        in_cost = True
    elif in_cost and '</td>' in line:
        in_cost = False
    elif not in_cost:
        out_lines.append(line)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write("\n".join(out_lines))
print("Updated cost display")
