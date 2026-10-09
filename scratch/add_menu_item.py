import re

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active fw-bold" : "")">
                                <i class="bi bi-bar-chart-fill me-2 fs-5 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "text-white" : "text-primary")"></i> Şantiye Panosu
                            </a>'''

# Handle encoding differences gracefully
target_alt = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active fw-bold" : "")">
                                <i class="bi bi-bar-chart-fill me-2 fs-5 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "text-white" : "text-primary")"></i> '''

idx = content.find(target_alt)
if idx != -1:
    end_a = content.find('</a>', idx) + 4
    
    new_menu_item = '''
                            
                            <a href="/ConstructionProject/Emergencies/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 position-relative @(ViewContext.RouteData.Values["Action"]?.ToString() == "Emergencies" ? "active fw-bold bg-warning text-dark border-warning" : "text-dark")">
                                <i class="bi bi-exclamation-triangle-fill me-2 fs-5 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Emergencies" ? "text-dark" : "text-warning")"></i> Acil Durum / Bildirimler
                                <span class="position-absolute top-50 end-0 translate-middle-y me-3 badge bg-danger rounded-pill shadow-sm" style="animation: pulse 1.5s infinite;">2 Yeni</span>
                            </a>'''
    
    content = content[:end_a] + new_menu_item + content[end_a:]

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
