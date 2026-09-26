import re

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace the h2 title
html = re.sub(
    r'<h2 class="fw-bold mb-1">Sosyal Donatılar ve Dış Alanlar.*?</h2>', 
    r'<h2 class="fw-bold mb-1">Sosyal Donatılar ve Dış Alanlar <span class="text-primary ms-2 fs-4">@(ViewBag.IsExisting == true ? "(Eski / Yıkılacak Durum)" : "(Yeni / Hedef Durum)")</span></h2>', 
    html
)

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
