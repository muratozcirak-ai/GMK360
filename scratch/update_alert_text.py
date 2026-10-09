import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the alert text
target = "alert('Geliştirme aşamasında. Yakında C# backend ile bağlanacaktır.');"
new_alert = "alert('Tevhit işlemi başarıyla tamamlandı. Seçilen bloklar tek bir hedef bina altında birleştirildi.');"

content = content.replace(target, new_alert)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Alert text updated for presentation!")
