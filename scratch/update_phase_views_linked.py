import codecs
import re
import glob

views = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')

html_snippet = '''
  @if (ViewBag.LinkedDocs != null && ((IEnumerable<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.LinkedDocs).Any())
  {
      <div class="alert alert-warning border-warning shadow-sm mb-4 rounded-4">
          <h5 class="fw-bold text-dark mb-3"><i class="bi bi-exclamation-triangle-fill text-warning me-2 fs-4"></i> DİKKAT: Bu Aşamaya Bağlı Ön Koşul Evrakları (Faz 0)</h5>
          <div class="list-group shadow-sm rounded-3">
              @foreach(var doc in (IEnumerable<GMK360.Core.Entities.Construction.ProjectLegalDocument>)ViewBag.LinkedDocs)
              {
                  <a href="/PhaseZero/Index/@ViewBag.ProjectId" class="list-group-item list-group-item-action list-group-item-warning border-warning d-flex justify-content-between align-items-center">
                      <div>
                          <i class="bi bi-file-earmark-text me-2"></i> <span class="fw-bold text-dark fs-6">@doc.DocumentName</span>
                      </div>
                      <span class="badge @(doc.Status == "Onaylandı" ? "bg-success" : "bg-danger") rounded-pill fs-6 px-3">@doc.Status</span>
                  </a>
              }
          </div>
          <small class="d-block mt-3 text-dark fw-semibold"><i class="bi bi-info-circle-fill me-1"></i> Not: Yukarıdaki evrakların süreci (Faz 0'da) tamamlanmadan bu aşamadaki imalatlara başlanması yasal risk oluşturur.</small>
      </div>
  }
'''

for path in views:
    # skip PhaseZero
    if 'PhaseZero' in path:
        continue

    try:
        with codecs.open(path, 'r', 'utf-8-sig') as f:
            content = f.read()

        if 'ViewBag.LinkedDocs' not in content:
            # inject right before accordion
            content = re.sub(
                r'(<div class="accordion border-0 shadow-sm rounded-4 overflow-hidden")',
                html_snippet + r'\n      \1',
                content, count=1
            )
            with codecs.open(path, 'w', 'utf-8-sig') as f:
                f.write(content)
            print(f"Updated {path}")
    except Exception as e:
        print(f"Error processing {path}: {e}")