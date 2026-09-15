import codecs

src_path = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
dest_path = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ProjectLayout.cshtml'

with codecs.open(src_path, 'r', 'utf-8') as f:
    content = f.read()

# Modify the Topbar to show project name
topbar_target = r'<span class="fw-bold text-navy ms-2 fs-5">GMK360</span>'
topbar_replacement = r'''<span class="fw-bold text-navy ms-2 fs-5">GMK360</span>
                @if (ViewData["ProjectName"] != null)
                {
                    <span class="fs-5 text-muted mx-2">|</span>
                    <i class="bi bi-building fs-5 text-orange me-2"></i>
                    <span class="fw-bold fs-5 text-uppercase" style="color: #2c3e50;">@ViewData["ProjectName"] ŞANTİYESİ</span>
                }'''

content = content.replace(topbar_target, topbar_replacement)

# Modify Sidebar
# Find the start of the sidebar list
sidebar_start = content.find('<div class="list-group list-group-flush pt-3 pb-5">')
sidebar_end = content.find('</div>', content.find('<div class="mt-auto', sidebar_start))

project_id_code = '@ViewData["ProjectId"]'

project_sidebar = f'''<div class="list-group list-group-flush pt-3 pb-5">
                      <div class="px-3 mb-3">
                          <a href="/ConstructionProject/Index" class="btn btn-outline-secondary w-100 rounded-pill fw-bold shadow-sm d-flex justify-content-center align-items-center">
                              <i class="bi bi-arrow-left-circle-fill me-2 fs-5 text-secondary"></i> Tüm Projelere Dön
                          </a>
                      </div>

                      <a href="/ConstructionProject/Details/{project_id_code}" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active" : "")">
                          <i class="bi bi-bar-chart-fill me-2 text-primary"></i> Şantiye Panosu
                      </a>
                      <a href="#" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                          <i class="bi bi-people-fill me-2 text-primary"></i> Puantaj ve Ekipler
                      </a>
                      <a href="/Inventory/Index/{project_id_code}" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                          <i class="bi bi-boxes me-2 text-primary"></i> Şantiye Deposu
                      </a>
                      <a href="/ConstructionProject/ManagePhases/{project_id_code}" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "ManagePhases" ? "active" : "")">
                          <i class="bi bi-kanban me-2 text-primary"></i> Bütçe ve Fazlar
                      </a>
                      <a href="#collapseFaz0" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Documents" ? "active" : "")">
                          <i class="bi bi-file-earmark-lock-fill me-2 text-danger"></i> Evraklar & Faz 0
                      </a>
                      
                      <div class="list-group-item bg-light fw-bold mt-4 text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                          Şantiye Modu Aktif
                      </div>
'''

if sidebar_start != -1:
    # We replace from <div class="list-group list-group-flush pt-3 pb-5"> down to just before the closing </div> of the sidebar wrapper
    sidebar_end_actual = content.find('</div>\n            </nav>', sidebar_start)
    if sidebar_end_actual != -1:
        content = content[:sidebar_start] + project_sidebar + content[sidebar_end_actual:]

with codecs.open(dest_path, 'w', 'utf-8') as f:
    f.write(content)
