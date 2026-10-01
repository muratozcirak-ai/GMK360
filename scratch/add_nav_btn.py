import io
import re

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Find the header section to inject the button
pattern = r'<h4 class="fw-bold text-dark mb-1"><i class="bi bi-building me-2 text-primary"></i> @ViewData\["ProjectName"\]</h4>'
replacement = r'<div class="d-flex justify-content-between align-items-start">\n                          <h4 class="fw-bold text-dark mb-1"><i class="bi bi-building me-2 text-primary"></i> @ViewData["ProjectName"]</h4>\n                          <a href="/PhaseZero/Index/@ViewData["ProjectId"]" class="btn btn-warning btn-sm fw-bold text-dark shadow-sm"><i class="bi bi-rocket-takeoff me-1"></i> Faz 0 Masasına Git</a>\n                      </div>'

if re.search(pattern, content):
    content = re.sub(pattern, replacement, content)
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added navigation button.")
else:
    print("Pattern not found!")
