import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\CustomerDashboard\SetupCorporateProfile.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Update form tag
content = content.replace('<form asp-action="SetupCorporateProfile" asp-controller="CustomerDashboard" method="post">', '<form asp-action="SetupCorporateProfile" asp-controller="CustomerDashboard" method="post" enctype="multipart/form-data">')

# Replace LogoUrl with LogoFile
logo_replacement = """                        <div class="mb-4">
                            <label asp-for="LogoFile" class="form-label fw-bold">Firma Logosu</label>
                            <input asp-for="LogoFile" type="file" class="form-control form-control-lg rounded-3" accept="image/jpeg,image/png" />
                            <div class="form-text">Firmanızın logosunu bilgisayarınızdan seçin (JPG/PNG).</div>
                        </div>"""

content = re.sub(r'<div class="mb-4">\s*<label asp-for="LogoUrl"[^>]+>.*?</label>\s*<input asp-for="LogoUrl"[^>]+/>\s*<div class="form-text">.*?</div>\s*</div>', logo_replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated View for file upload.")
