import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# We need to find where the cost and status are displayed
# Currently it shows Cost as @doc.Cost.ToString("N2") ₺ and Status as the badge.

# We will inject the logic to compute real cost and quote status above the row rendering or inside it.
# Actually, inside the foreach loop:

injection = """                                  @{
                                      var relatedQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == doc.Id);
                                      var pendingQuotes = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Pending) ?? 0;
                                      var submittedQuotes = relatedQuote?.Invites.Count(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted) ?? 0;
                                      var kesifRequests = relatedQuote?.Invites.Count(i => i.OfferNotes != null && i.OfferNotes.Contains("KEŞİF")) ?? 0;
                                      var hasQuote = relatedQuote != null;
                                  }
                                  <tr>
                                      <td>
                                          <i class="bi bi-file-earmark-text text-muted me-2"></i> @doc.SystemTemplate.Name
                                          @if (!string.IsNullOrEmpty(doc.Conditions))
                                          {
                                              <div class="small text-danger ms-4"><i class="bi bi-exclamation-triangle"></i> Ön koşullar tamamlanmadı!</div>
                                          }
                                      </td>
                                      <td><i class="bi bi-building text-secondary"></i> @doc.Source</td>
                                      <td><i class="bi bi-person text-primary"></i> @doc.Assignee</td>
                                      <td class="fw-bold">
                                          @if (submittedQuotes > 0 && relatedQuote != null)
                                          {
                                              // Get lowest or latest submitted quote just for display
                                              var bestPrice = relatedQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted).Min(i => i.OfferedPrice);
                                              <span class="text-success"><i class="bi bi-check-circle-fill"></i> @bestPrice.ToString("N2") ₺</span>
                                          }
                                          else
                                          {
                                              @doc.Cost.ToString("N2") <span class="text-muted">₺</span>
                                          }
                                      </td>
                                      <td>
                                          @if (isQuoteActive)
                                          {
                                              <span class="badge bg-info bg-opacity-10 text-info border border-info rounded-pill px-3">
                                                  <i class="bi bi-search"></i> Fiyat Araştırılıyor
                                              </span>
                                              @if (submittedQuotes > 0)
                                              {
                                                  <span class="badge bg-success bg-opacity-10 text-success border border-success rounded-pill px-2 ms-1 mt-1 d-block" style="font-size: 0.75rem;">
                                                      <i class="bi bi-envelope-check"></i> @submittedQuotes Teklif Geldi
                                                  </span>
                                              }
                                              @if (kesifRequests > 0)
                                              {
                                                  <span class="badge bg-danger bg-opacity-10 text-danger border border-danger rounded-pill px-2 ms-1 mt-1 d-block" style="font-size: 0.75rem;">
                                                      <i class="bi bi-geo-alt"></i> @kesifRequests Keşif Talebi
                                                  </span>
                                              }
                                              @if (pendingQuotes > 0 && submittedQuotes == 0 && kesifRequests == 0)
                                              {
                                                  <span class="badge bg-secondary bg-opacity-10 text-secondary border border-secondary rounded-pill px-2 ms-1 mt-1 d-block" style="font-size: 0.75rem;">
                                                      <i class="bi bi-hourglass-split"></i> @pendingQuotes Davet Bekliyor
                                                  </span>
                                              }
                                          }
                                          else if (isLocked)"""

# I need to find the existing <tr> block and replace it.
import io
with io.open(r'GMK360.Web\Views\PhaseZero\Index.cshtml', 'r', encoding='utf-8') as f:
    existing_content = f.read()

# Replace the specific block
existing_block = """                                  <tr>
                                      <td>
                                          <i class="bi bi-file-earmark-text text-muted me-2"></i> @doc.SystemTemplate.Name
                                          @if (!string.IsNullOrEmpty(doc.Conditions))
                                          {
                                              <div class="small text-danger ms-4"><i class="bi bi-exclamation-triangle"></i> n koullar tamamlanmad!</div>
                                          }
                                      </td>
                                      <td><i class="bi bi-building text-secondary"></i> @doc.Source</td>
                                      <td><i class="bi bi-person text-primary"></i> @doc.Assignee</td>
                                      <td class="fw-bold">@doc.Cost.ToString("N2") ₺</td>
                                      <td>
                                          @if (isQuoteActive)
                                          {
                                              <span class="badge bg-info bg-opacity-10 text-info border border-info rounded-pill px-3">
                                                  <i class="bi bi-search"></i> Fiyat Aratrlyor
                                              </span>
                                          }
                                          else if (isLocked)"""

# Instead of direct string replace which fails on Turkish chars, let's use regex from '<tr>' to 'else if (isLocked)'
existing_content = re.sub(r'<tr>\s*<td>.*?else if \(isLocked\)', injection, existing_content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(existing_content)

print("Updated View")
