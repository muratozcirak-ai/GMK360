import io

# Fix Controller
filepath_ctrl = r'GMK360.Web\Controllers\B2BPartnerPortalController.cs'
with io.open(filepath_ctrl, 'r', encoding='utf-8') as f:
    content = f.read()
content = content.replace('TotalAreaM2', 'TotalLandArea')
with io.open(filepath_ctrl, 'w', encoding='utf-8') as f:
    f.write(content)

# Fix View
filepath_view = r'GMK360.Web\Views\B2BPartnerPortal\Invite.cshtml'
with io.open(filepath_view, 'r', encoding='utf-8') as f:
    content = f.read()
content = content.replace('RequesterAgency?.Name', 'RequesterAgency?.CompanyName')
with io.open(filepath_view, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed compile errors.")
