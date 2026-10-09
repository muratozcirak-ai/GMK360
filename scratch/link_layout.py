import re
with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig') as f:
    content = f.read()

content = content.replace('<a href="#" class="text-decoration-none text-dark hover-orange fw-bold">Biz Kimiz</a>', '<a asp-controller="Home" asp-action="About" class="text-decoration-none text-dark hover-orange fw-bold">Biz Kimiz</a>')
content = content.replace('<a href="#" class="text-decoration-none text-dark hover-orange fw-bold">İletişim</a>', '<a asp-controller="Home" asp-action="Contact" class="text-decoration-none text-dark hover-orange fw-bold">İletişim</a>')

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
