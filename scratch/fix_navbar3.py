import codecs
import re

path = 'GMK360.Web/Views/Shared/_Layout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the entire nav block
nav_pattern = re.compile(r'<nav class="d-none d-md-flex gap-4 fw-medium text-secondary">.*?</nav>', re.DOTALL)

new_nav = '''<nav class="d-none d-lg-flex gap-4 fw-medium text-secondary">
                <a href="/BuildingManager/Detail/1" class="text-decoration-none text-dark hover-orange">Bina Yönetimi</a>
                <a asp-controller="DigitalHome" asp-action="Index" class="text-decoration-none text-dark hover-orange">Dijital Evim</a>
                <div class="dropdown">
                    <a href="#" class="text-decoration-none text-primary hover-orange fw-bold dropdown-toggle" data-bs-toggle="dropdown"><i class="ph-fill ph-robot align-middle me-1"></i>Asistanlar</a>
                    <ul class="dropdown-menu shadow border-0 rounded-3">
                        <li><a class="dropdown-item" asp-controller="AiAssistant" asp-action="Index"><i class="ph-fill ph-robot text-primary me-2"></i>Yapay Zeka Asistanı</a></li>
                        <li><a class="dropdown-item" asp-controller="TaxAssistant" asp-action="Simulator"><i class="ph-fill ph-calculator text-success me-2"></i>Beyanname Asistanı</a></li>
                    </ul>
                </div>
                <a href="/Dashboard/Hub" class="text-decoration-none text-dark hover-orange">Tüm Hizmetler</a>
            </nav>'''

content = re.sub(nav_pattern, new_nav, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)