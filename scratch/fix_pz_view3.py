import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# The second occurrence of @if (relatedQuote != null ...) is the broken one.
# We will find the child loop and fix it.
child_loop_start = r'(@foreach\(var childDoc in group\.Where.*?)\s*(@if \(relatedQuote != null)'

# Let's just find the second @if (relatedQuote != null...) and replace it.
# Instead of guessing, I'll split the content by '@if (relatedQuote != null && relatedQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))'

parts = content.split('@if (relatedQuote != null && relatedQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))')

# parts[0] is everything before the first one.
# parts[1] is the content of the first one.
# parts[2] is the content of the second one (child).

if len(parts) >= 3:
    # Fix parts[2]
    new_part2 = '''@{ var childQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id); }
                                          @if (childQuote != null && childQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))'''
    
    # We also need to replace elatedQuote with childQuote and doc.Id with childDoc.Id inside parts[2]
    # Actually, parts[2] starts with the { from the if block.
    # Let's just do a targeted replace.
    
    # Let's use a simpler approach. I'll replace the block for the child loop completely.
    pass

# Alternative simpler approach:
# Just find the block inside child loop and replace.
# The block starts right after:
# <button class="btn btn-sm btn-outline-secondary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal(@childDoc.Id)">
#    <i class="bi bi-pencil-square"></i> Yönet
# </button>
# </div>
# </td>
# </tr>

target = r'''(<button class="btn btn-sm btn-outline-secondary rounded-pill px-3 fw-bold shadow-sm" onclick="openManageModal\(@childDoc\.Id\)">\s*<i class="bi bi-pencil-square"></i> Y\?önet\s*</button>\s*</div>\s*</td>\s*</tr>)\s*@if \(relatedQuote != null.*?renderedDocIds\.Add\(childDoc\.Id\);'''

replacement = r'''\1
                                          @{ var childQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id); }
                                          @if (childQuote != null && childQuote.Invites.Any(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted))
                                          {
                                              <tr class="bg-light">
                                                  <td colspan="7" class="p-3 border-start border-4 border-info">
                                                      <div class="d-flex align-items-center mb-2">
                                                          <i class="bi bi-arrow-return-right me-2 text-info"></i>
                                                          <strong class="text-secondary">Alınan Fiyat Teklifleri (Fizibiliteye Eklenebilir)</strong>
                                                      </div>
                                                      <div class="row g-3">
                                                          @foreach(var inv in childQuote.Invites.Where(i => i.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted).OrderBy(i => i.OfferedPrice))
                                                          {
                                                              <div class="col-md-4">
                                                                  <div class="card border border-info shadow-sm">
                                                                      <div class="card-body p-2 d-flex justify-content-between align-items-center">
                                                                          <div>
                                                                              <h6 class="mb-0 text-dark fw-bold">@inv.NetworkContact?.CompanyName</h6>
                                                                              <span class="text-success fw-bold fs-6">@inv.OfferedPrice?.ToString("C2")</span>
                                                                          </div>
                                                                          <form asp-action="AcceptQuote" method="post">
                                                                              <input type="hidden" name="documentId" value="@childDoc.Id" />
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
                                          }
                                                      renderedDocIds.Add(childDoc.Id);'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)