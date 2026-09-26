import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_str = '<div class="small text-muted">@item.CompanyType.ToString()</div>'
new_str = '<div class="small text-muted">@(item.LegalStatus == GMK360.Core.Entities.B2b.LegalEntityType.Corporate ? "Şirket (Tüzel)" : "Bireysel (Şahıs)")</div>'

content = content.replace(old_str, new_str)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
