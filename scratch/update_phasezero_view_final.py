import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

injection = """                                        @{
                                            var relatedQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == doc.Id);
                                            var pendingQuotes = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Pending) ?? 0;
                                            var submittedQuotes = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted) ?? 0;
                                            var kesifRequests = relatedQuote?.Invites.Count(i => i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF")) ?? 0;
                                            var hasQuote = relatedQuote != null;
                                        }
                                        <tr data-bs-toggle="@(isDependent ? "collapse" : "")" data-bs-target=".child-of-@doc.Id" style="@(isDependent ? "cursor: pointer;" : "")">"""

content = re.sub(r'<tr data-bs-toggle="@\(isDependent \? "collapse" : ""\)" data-bs-target="\.child-of-@doc\.Id"', injection, content, flags=re.DOTALL)

# Inject Status Badges
badge_injection = """                                                @if (doc.Status == "Fiyat Araştırılıyor" || doc.Status == "Teklif Geldi") 
                                                { 
                                                    <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> @doc.Status</span> 
                                                    @if(submittedQuotes > 0)
                                                    {
                                                        <br/><span class="badge bg-success mt-1 rounded-pill px-2 shadow-sm"><i class="bi bi-check-circle"></i> @submittedQuotes Teklif Geldi</span>
                                                    }
                                                    @if(kesifRequests > 0)
                                                    {
                                                        <br/><span class="badge bg-danger mt-1 rounded-pill px-2 shadow-sm"><i class="bi bi-geo-alt"></i> @kesifRequests Keşif Talebi</span>
                                                    }
                                                    @if(pendingQuotes > 0 && submittedQuotes == 0 && kesifRequests == 0)
                                                    {
                                                        <br/><span class="badge bg-secondary mt-1 rounded-pill px-2 shadow-sm"><i class="bi bi-hourglass-split"></i> @pendingQuotes Bekliyor</span>
                                                    }
                                                }
                                                else if (isDependent && !isReadyToApply) { <span class="badge bg-danger rounded-pill px-3 shadow-sm"><i class="bi bi-lock-fill"></i> Kilitli (Önkoşul)</span> }"""

# Replacing the status block
import sys
content = re.sub(r'@if \(doc\.Status == "Fiyat Ara.*?else if \(isDependent && !isReadyToApply\) \{ <span class="badge bg-danger rounded-pill px-3 shadow-sm"><i class="bi bi-lock-fill"></i> Kilitli \(\?-nko\?Yul\)</span> \}', badge_injection, content, flags=re.DOTALL)
# Actually the regex is tricky due to charmap. Let's do it simply by reading the file and replacing the specific block

lines = content.split('\n')
out_lines = []
in_status = False
for line in lines:
    if '@if (doc.Status == "Fiyat Ara' in line:
        out_lines.append(badge_injection)
        in_status = True
    elif in_status and 'else if (isDependent && !isReadyToApply)' in line:
        in_status = False
    elif not in_status:
        out_lines.append(line)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write("\n".join(out_lines))

print("Updated PhaseZero View Badges")
