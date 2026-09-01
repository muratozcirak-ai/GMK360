import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\CustomerDashboard\SetupCorporateProfile.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

tax_field = """
                        <div class="mb-4">
                            <label asp-for="TaxNumber" class="form-label fw-bold">Vergi Numarası</label>
                            <input asp-for="TaxNumber" class="form-control form-control-lg rounded-3" placeholder="Örn: 1234567890" />
                            <span asp-validation-for="TaxNumber" class="text-danger small"></span>
                        </div>
"""

# Insert it after CompanyName
content = content.replace('placeholder="Örn: Öztürk İnşaat A.Ş." />\n                            <span asp-validation-for="CompanyName" class="text-danger small"></span>\n                        </div>', 'placeholder="Örn: Öztürk İnşaat A.Ş." />\n                            <span asp-validation-for="CompanyName" class="text-danger small"></span>\n                        </div>\n' + tax_field)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated View with TaxNumber.")
