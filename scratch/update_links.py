import io

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Update PhaseZero link
content = content.replace('<a href="/PhaseZero/Index/@Model.Id" class="btn btn-sm btn-danger fw-bold rounded-pill text-nowrap shadow-sm">',
                          '<a href="/PhaseZero/Index/@Model.Id" target="_blank" class="btn btn-sm btn-danger fw-bold rounded-pill text-nowrap shadow-sm">')

# Update Sihirbaza Dön / Düzenle link
content = content.replace('<a href="/ConstructionProject/Create/@Model.Id" class="btn btn-warning text-dark fw-bold ms-2">',
                          '<a href="/ConstructionProject/Create/@Model.Id" target="_blank" class="btn btn-warning text-dark fw-bold ms-2">')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml links with target=_blank")
