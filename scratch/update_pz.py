import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Variables Injection (Fixing RZ1010)
calc_injection = """
                                        var relatedQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == doc.Id);
                                        var pendingQ = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Pending) ?? 0;
                                        var submittedQ = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted) ?? 0;
                                        var kesifReq = relatedQuote?.Invites.Count(i => i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF")) ?? 0;
                                        decimal bestPrice = submittedQ > 0 ? (relatedQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted).Min(i => i.OfferedPrice) ?? 0) : 0;
                                        
                                        <tr data-bs-toggle="@(isDependent ? "collapse" : "")" data-bs-target=".child-of-@doc.Id" style="@(isDependent ? "cursor: pointer;" : "")">"""

content = re.sub(r'<tr data-bs-toggle="@\(isDependent \? "collapse" : ""\)" data-bs-target="\.child-of-@doc\.Id" style="@\(isDependent \? "cursor: pointer;" : ""\)">', calc_injection, content)

# 2. Status Badges Injection
status_search = r'@if \(doc\.Status == "Fiyat Araş.*?<i class="bi bi-search"></i> Fiyat Araştırılıyor</span> }'

status_injection = """@if (doc.Status == "Fiyat Araştırılıyor" || doc.Status == "Teklif Geldi") 
                                                { 
                                                    <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> @doc.Status</span> 
                                                    @if(submittedQ > 0)
                                                    {
                                                        <br/><span class="badge bg-success mt-1 rounded-pill px-2 shadow-sm" style="font-size:0.75rem;"><i class="bi bi-check-circle"></i> @submittedQ Teklif Geldi</span>
                                                    }
                                                    @if(kesifReq > 0)
                                                    {
                                                        <br/><span class="badge bg-danger mt-1 rounded-pill px-2 shadow-sm" style="font-size:0.75rem;"><i class="bi bi-geo-alt"></i> @kesifReq Keşif Talebi</span>
                                                    }
                                                    @if(pendingQ > 0 && submittedQ == 0 && kesifReq == 0)
                                                    {
                                                        <br/><span class="badge bg-secondary mt-1 rounded-pill px-2 shadow-sm" style="font-size:0.75rem;"><i class="bi bi-hourglass-split"></i> @pendingQ Davet Bekliyor</span>
                                                    }
                                                }"""

content = re.sub(status_search, status_injection, content)

# 3. Cost Injection (Fixing CS1056 by being extremely careful with ₺)
# Let's find: @((doc.DocumentFee.GetValueOrDefault() + doc.AdditionalCost.GetValueOrDefault()).ToString("N2"))
cost_search = r'(@\(\(doc\.DocumentFee\.GetValueOrDefault\(\) \+ doc\.AdditionalCost\.GetValueOrDefault\(\)\)\.ToString\("N2"\)\))'

cost_injection = """@if(bestPrice > 0) { <span class="text-success fs-6"><i class="bi bi-check-circle-fill"></i> @bestPrice.ToString("N2")</span> } else { \\1 }"""

content = re.sub(cost_search, cost_injection, content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZero Index successfully.")
