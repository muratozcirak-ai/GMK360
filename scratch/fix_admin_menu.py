import codecs
import re

path = 'GMK360.Web/Views/Shared/_AdminLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<a href="/AdminLegalDocument/Index" class="nav-item @\(ViewContext\.RouteData\.Values\["Controller"\]\?\.ToString\(\) == "AdminLegalDocument" \? "active" : ""\)">\s*<i class="ph ph-file-doc"></i> Yasal Evrak Şablonları\s*</a>'
replacement = '''<a href="/AdminLegalDocument/Index" class="nav-item @(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminLegalDocument" ? "active" : "")">
                <i class="ph ph-file-doc"></i> Yasal Evrak Şablonları
            </a>
            <a href="/SystemPhaseTemplate/Index" class="nav-item @(ViewContext.RouteData.Values["Controller"]?.ToString() == "SystemPhaseTemplate" ? "active" : "")">
                <i class="ph ph-kanban"></i> İnşaat Faz Şablonları
            </a>'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)