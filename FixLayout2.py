import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ProjectLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

# Replace sidebar
sidebar_start_match = re.search(r'<div class="list-group list-group-flush"', content)
if sidebar_start_match:
    sidebar_start = sidebar_start_match.start()
    
    # find the closing of nav
    sidebar_end = content.find('</nav>', sidebar_start)
    if sidebar_end != -1:
        
        project_sidebar = '''<div class="list-group list-group-flush" style="font-size: 0.9rem;">
                      <div class="px-3 mb-3 mt-3">
                          <a href="/ConstructionProject/Index" class="btn btn-outline-secondary w-100 rounded-pill fw-bold shadow-sm d-flex justify-content-center align-items-center">
                              <i class="bi bi-arrow-left-circle-fill me-2 fs-5 text-secondary"></i> Tüm Projelere Dön
                          </a>
                      </div>

                      <a href="/ConstructionProject/Details/@ViewData["ProjectId"]" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active" : "")">
                          <i class="bi bi-bar-chart-fill me-2 text-primary"></i> Şantiye Panosu
                      </a>
                      <a href="#" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                          <i class="bi bi-people-fill me-2 text-primary"></i> Puantaj ve Ekipler
                      </a>
                      <a href="/Inventory/Index/@ViewData["ProjectId"]" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                          <i class="bi bi-boxes me-2 text-primary"></i> Şantiye Deposu
                      </a>
                      <a href="/ConstructionProject/ManagePhases/@ViewData["ProjectId"]" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "ManagePhases" ? "active" : "")">
                          <i class="bi bi-kanban me-2 text-primary"></i> Bütçe ve Fazlar
                      </a>
                      <a href="#collapseFaz0" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Documents" ? "active" : "")">
                          <i class="bi bi-file-earmark-lock-fill me-2 text-danger"></i> Evraklar & Faz 0
                      </a>
                      
                      <div class="list-group-item bg-light fw-bold mt-4 text-uppercase text-center" style="font-size: 0.8rem; color: #6c757d;">
                          Şantiye Modu Aktif
                      </div>
                  </div>
'''
        content = content[:sidebar_start] + project_sidebar + content[sidebar_end:]

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
