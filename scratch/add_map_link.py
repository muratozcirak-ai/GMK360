import re

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]#collapseFaz0" class="list-group-item list-group-item-action py-3">
                                <i class="bi bi-folder-fill me-2 fs-5 text-warning"></i> Evrak Ynetimi
                            </a>'''

# use simple replace
target_alt = '''<a href="/ConstructionProject/Details/@ViewData["ProjectId"]#collapseFaz0" class="list-group-item list-group-item-action py-3">
                                <i class="bi bi-folder-fill me-2 fs-5 text-warning"></i> '''

idx = content.find(target_alt)
if idx != -1:
    end_a = content.find('</a>', idx) + 4
    
    new_menu_item = '''
                            
                            <a href="/ConstructionProject/Map/@ViewData["ProjectId"]" class="list-group-item list-group-item-action py-3 text-muted" target="_blank" title="Lojistik ve Yol Tarifi">
                                <i class="bi bi-geo-alt-fill me-2 fs-5 text-secondary"></i> Şantiye Konumu (Harita)
                            </a>'''
    
    content = content[:end_a] + new_menu_item + content[end_a:]

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
