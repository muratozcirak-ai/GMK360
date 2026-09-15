import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_Layout.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

# Topbar logic
nav_target = '''<img src="/images/logo.png" alt="GMK360 Logo" height="30" class="me-2" onerror="this.src='https://placehold.co/120x40/FF6B00/FFFFFF?text=GMK360'" />
                <span class="fw-bold text-orange fs-4" style="letter-spacing: -1px;">GMK360</span>'''

nav_replace = '''<img src="/images/logo.png" alt="GMK360 Logo" height="30" class="me-2" onerror="this.src='https://placehold.co/120x40/FF6B00/FFFFFF?text=GMK360'" />
                <span class="fw-bold text-orange fs-4" style="letter-spacing: -1px;">GMK360</span>
                @if(ViewData["ProjectName"] != null)
                {
                    <span class="fs-4 text-muted mx-3">|</span>
                    <i class="bi bi-building-check fs-5 text-primary me-2"></i>
                    <span class="fw-bold fs-5 text-uppercase text-dark" style="letter-spacing: 0px;">@ViewData["ProjectName"] ŞANTİYESİ</span>
                }'''

content = content.replace(nav_target, nav_replace)

# Hide main menu items if ProjectName is set
menu_target = '''<nav class="d-none d-md-flex gap-4 fw-medium text-secondary">'''
menu_replace = '''<nav class="d-none d-md-flex gap-4 fw-medium text-secondary">
                @if(ViewData["ProjectName"] == null)
                {'''

content = content.replace(menu_target, menu_replace)

menu_end_target = '''<a href="#" class="text-decoration-none text-dark hover-orange">Hizmetler</a>
            </nav>'''
menu_end_replace = '''<a href="#" class="text-decoration-none text-dark hover-orange">Hizmetler</a>
                }
            </nav>'''
content = content.replace(menu_end_target, menu_end_replace)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
