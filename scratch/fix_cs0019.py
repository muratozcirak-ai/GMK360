import io

filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("RequesterAgencyId = doc.ConstructionProject.AgencyId ?? 1,", "RequesterAgencyId = doc.ConstructionProject.AgencyId,")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed CS0019 in PhaseZeroController.")
