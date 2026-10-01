import io

filepath = r'GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Change card onclick to window.open
content = content.replace("onclick=\"location.href='/ConstructionProject/Details/@Model.Id'\"",
                          "onclick=\"window.open('/ConstructionProject/Details/@Model.Id', '_blank')\"")

# Fix 2: Add target="_blank" and stopPropagation to the Geçmiş Şantiye button
content = content.replace("class=\"btn btn-outline-secondary btn-sm rounded-pill w-100 fw-bold\">Gemi antiye",
                          "target=\"_blank\" onclick=\"event.stopPropagation();\" class=\"btn btn-outline-secondary btn-sm rounded-pill w-100 fw-bold\">Gemi antiye")

# Fix 3: Add target="_blank" and stopPropagation to the Şantiye Panosuna Git button
content = content.replace("class=\"btn btn-outline-primary btn-sm rounded-pill w-100 fw-bold\">antiye Panosuna Git",
                          "target=\"_blank\" onclick=\"event.stopPropagation();\" class=\"btn btn-outline-primary btn-sm rounded-pill w-100 fw-bold\">antiye Panosuna Git")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated _ProjectCardPartial.cshtml")
