import re

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace ViewData["Title"]
html = re.sub(
    r'ViewData\["Title"\] = "Sosyal Donatılar ve Dış Alanlar";',
    r'ViewData["Title"] = ViewBag.IsExisting == true ? "Sosyal Donatılar (Eski Durum)" : "Sosyal Donatılar (Hedef Durum)";',
    html
)

# Replace h3 title
html = re.sub(
    r'<h3 class="fw-bolder text-dark mb-1">Sosyal Donatılar ve Dış Alanlar</h3>',
    r'<h3 class="fw-bolder text-dark mb-1">Sosyal Donatılar ve Dış Alanlar <span class="badge badge-light-@(ViewBag.IsExisting == true ? "danger" : "primary") ms-2 fs-5">@(ViewBag.IsExisting == true ? "ESKİ (YIKILACAK) DURUM" : "YENİ (HEDEF) DURUM")</span></h3>',
    html
)

with open('GMK360.Web/Views/ConstructionProject/Amenities.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
