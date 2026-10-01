import codecs

path = 'GMK360.Web/Views/AdminCRM/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = '''<button class="btn btn-sm btn-outline-primary rounded-pill px-3 py-1 fw-bold fs-7" type="button" data-bs-toggle="collapse" data-bs-target="#details-@user.Id" aria-expanded="false" aria-controls="details-@user.Id">
                                    Detay Gör
                                </button>'''

replacement = '''<a asp-action="UserDetails" asp-route-id="@user.Id" class="btn btn-sm btn-primary rounded-pill px-3 py-1 fw-bold fs-7 text-white shadow-sm me-1">
                                    <i class="bi bi-person-lines-fill me-1"></i>Tam Profil
                                </a>
                                <button class="btn btn-sm btn-outline-secondary rounded-pill px-3 py-1 fw-bold fs-7" type="button" data-bs-toggle="collapse" data-bs-target="#details-@user.Id" aria-expanded="false" aria-controls="details-@user.Id">
                                    Abonelikler
                                </button>'''

if target in content:
    content = content.replace(target, replacement)
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Added Full Profile button to CRM Index")
else:
    print("Target not found in CRM Index")