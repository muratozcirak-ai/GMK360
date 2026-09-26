import re

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

pattern = r'<span class="badge bg-secondary">@item\.TargetModule</span>'
replacement = '''<span class="badge bg-secondary">
                                          @(item.TargetModule == "Construction" ? "İnşaat / Şantiye" :
                                            item.TargetModule == "RealEstate" ? "Emlak / Gayrimenkul" :
                                            item.TargetModule == "ServiceProvider" ? "Usta / Tedarikçi" : item.TargetModule)
                                      </span>'''

if re.search(pattern, text):
    text = re.sub(pattern, replacement, text, count=1)
    with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print("Replaced successfully")
else:
    print("Pattern not found")
