import io
import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Make the status column show the OfferNotes if it exists
new_status = """                                                  @if (invite.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Pending)
                                                  {
                                                      <span class="badge bg-secondary">Bekliyor</span>
                                                  }
                                                  else if (invite.Status == GMK360.Core.Entities.B2B.QuoteInviteStatus.Submitted)
                                                  {
                                                      <span class="badge bg-success">Teklif Geldi</span>
                                                  }
                                                  
                                                  @if(!string.IsNullOrEmpty(invite.OfferNotes))
                                                  {
                                                      <div class="small text-danger mt-1 fw-bold" style="max-width:200px; white-space:normal;">
                                                          <i class="bi bi-info-circle"></i> @invite.OfferNotes
                                                      </div>
                                                  }"""
                                                  
content = re.sub(
    r'@if\s*\(invite\.Status\s*==\s*GMK360\.Core\.Entities\.B2B\.QuoteInviteStatus\.Pending\).*?</span>\s*}',
    new_status,
    content,
    flags=re.DOTALL | re.MULTILINE
)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml to show OfferNotes")
