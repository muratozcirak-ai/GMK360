import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# 1. Prerequisite fix
target_prereq = r'bool isOnlyPrereq = globalRules\.Any\(r => r\.Prerequisites != null && r\.Prerequisites\.Any\(p => p\.PrerequisiteTemplateId == doc\.SystemTemplateId\)\) && rule == null;'
replacement_prereq = '''bool isChildOfSomeone = Model.Any(m => globalRules.FirstOrDefault(r => r.SystemLegalDocumentTemplateId == m.SystemTemplateId)?.Prerequisites?.Any(p => p.PrerequisiteTemplateId == doc.SystemTemplateId) == true);'''
content = content.replace(target_prereq, replacement_prereq)
content = content.replace('if (isOnlyPrereq) { continue; }', 'if (isChildOfSomeone) { continue; }')

# 2. Biddable logic
target_biddable = r'var prereqs = rule\?\.Prerequisites;'
replacement_biddable = '''var biddableKeywords = new[] { "Firma", "Taşeron", "Ofis", "Mühendis", "Mimar", "Laboratuvar", "OSGB" };
                                        bool canRequestQuote = doc.SystemTemplate?.IssuedBy != null && biddableKeywords.Any(k => doc.SystemTemplate.IssuedBy.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                                        var prereqs = rule?.Prerequisites;'''
content = content.replace(target_biddable, replacement_biddable)

target_biddable_child = r'renderedDocIds\.Add\(cDoc\.Id\);'
replacement_biddable_child = '''bool canRequestChildQuote = childDoc.SystemTemplate?.IssuedBy != null && biddableKeywords.Any(k => childDoc.SystemTemplate.IssuedBy.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
                                                renderedDocIds.Add(cDoc.Id);'''
content = content.replace(target_biddable_child, replacement_biddable_child)


# 3. Main row button fix
target_main_btn = r'''@if\(relatedQuote != null\) \{
\s*<button type="button" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Firma Teklifi Ekle" onclick="openInviteModal\(@relatedQuote\.Id, '@doc\.DocumentName'\)">
\s*<i class="bi bi-plus-circle"></i> Firma Ekle
\s*</button>
\s*\} else \{
\s*<form method="post" action="/PhaseZero/RequestQuote/@doc\.Id" class="m-0 p-0">
\s*<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @\(isDependent && !isReadyToApply \? "disabled" : ""\)>
\s*<i class="bi bi-cart-plus"></i> Teklif
\s*</button>
\s*</form>
\s*\}'''

replacement_main_btn = '''@if(canRequestQuote) {
                                                              @if(relatedQuote != null) {
                                                                  <a href="/B2BPurchasing/Details/@relatedQuote.Id" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Satınalma & İhale Yönetimi">
                                                                      <i class="bi bi-box-arrow-up-right"></i> İhaleye Git
                                                                  </a>
                                                              } else {
                                                                  <form method="post" action="/PhaseZero/RequestQuote/@doc.Id" class="m-0 p-0">
                                                                      <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @(isDependent && !isReadyToApply ? "disabled" : "")>
                                                                          <i class="bi bi-cart-plus"></i> Teklif
                                                                      </button>
                                                                  </form>
                                                              }
                                                          }'''
content = re.sub(target_main_btn, replacement_main_btn, content)

# 4. Child row button fix
target_child_btn = r'''@if\(\(ViewBag\.Quotes as IEnumerable<GMK360\.Core\.Entities\.B2B\.B2BQuoteRequest>\)\?\.FirstOrDefault\(q => q\.SourceReferenceId == childDoc\.Id\) != null\) \{
\s*<button type="button" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Firma Teklifi Ekle" onclick="openInviteModal\(@\(\(\(ViewBag\.Quotes as IEnumerable<GMK360\.Core\.Entities\.B2B\.B2BQuoteRequest>\)\?\.FirstOrDefault\(q => q\.SourceReferenceId == childDoc\.Id\)\)\?\.Id\), '@childDoc\.DocumentName'\)">
\s*<i class="bi bi-plus-circle"></i> Firma Ekle
\s*</button>
\s*\} else \{
\s*<form method="post" action="/PhaseZero/RequestQuote/@childDoc\.Id" class="m-0 p-0">
\s*<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">
\s*<i class="bi bi-cart-plus"></i> Teklif
\s*</button>
\s*</form>
\s*\}'''

replacement_child_btn = '''@if(canRequestChildQuote) {
                                                                      var tempQuote = (ViewBag.Quotes as IEnumerable<GMK360.Core.Entities.B2B.B2BQuoteRequest>)?.FirstOrDefault(q => q.SourceReferenceId == childDoc.Id);
                                                                      @if(tempQuote != null) {
                                                                          <a href="/B2BPurchasing/Details/@tempQuote.Id" class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Satınalma & İhale Yönetimi">
                                                                              <i class="bi bi-box-arrow-up-right"></i> İhaleye Git
                                                                          </a>
                                                                      } else {
                                                                          <form method="post" action="/PhaseZero/RequestQuote/@childDoc.Id" class="m-0 p-0">
                                                                              <button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">
                                                                                  <i class="bi bi-cart-plus"></i> Teklif
                                                                              </button>
                                                                          </form>
                                                                      }
                                                                  }'''
content = re.sub(target_child_btn, replacement_child_btn, content)

# 5. Remove the modal block at the bottom
target_modal = r'<!-- FIRMA EKLE MODAL -->.*?</script>'
content = re.sub(target_modal, '', content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)