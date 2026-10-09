import re

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active fw-bold" : "")">
                                <i class="bi bi-bar-chart-fill me-2 fs-5 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "text-white" : "text-primary")"></i> Şantiye Panosu
                            </a>'''

# Let's search by string fragment to be safe from encoding issues
target_alt = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active fw-bold" : "")">
                                <i class="bi bi-bar-chart-fill me-2 fs-5 @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "text-white" : "text-primary")"></i> '''

idx = content.find(target_alt)
if idx != -1:
    end_a = content.find('</a>', idx) + 4
    
    # insert Faz 0 right after Şantiye Panosu
    new_menu_item = '''
                            
                            <a href="/PhaseZero/Index/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PhaseZero" ? "active fw-bold bg-danger text-white border-danger" : "text-danger fw-bold")">
                                <i class="bi bi-file-earmark-lock-fill me-2 fs-5 @(ViewContext.RouteData.Values["Controller"]?.ToString() == "PhaseZero" ? "text-white" : "text-danger")"></i> Faz 0: Resmi Evrak
                            </a>'''
    
    content = content[:end_a] + new_menu_item + content[end_a:]

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
