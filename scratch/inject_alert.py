import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

alert_html = """
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show shadow-sm text-white border-0" role="alert">
        <i class="bi bi-check-circle-fill me-2"></i> @Html.Raw(TempData["SuccessMessage"])
        <button type="button" class="btn-close text-white" data-bs-dismiss="alert"></button>
    </div>
}
"""

if "TempData[\"SuccessMessage\"]" not in content:
    content = re.sub(r'<div class="container-fluid py-4">', r'<div class="container-fluid py-4">\n' + alert_html, content, count=1)
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added SuccessMessage alert")
else:
    print("SuccessMessage already exists")
