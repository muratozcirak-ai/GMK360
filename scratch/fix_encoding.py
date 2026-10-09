with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re
# Replace the garbage string
content = re.sub(r'TempData\["SuccessMessage"\] = "ÃƒÆ’Ã†â€™.*?uruldu\.";', 'TempData["SuccessMessage"] = "Şantiye başarıyla başlatıldı ve bloklar oluşturuldu.";', content)

with open("GMK360.Web/Controllers/ConstructionProjectController.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Fixed encoding string in CreateWizard")
