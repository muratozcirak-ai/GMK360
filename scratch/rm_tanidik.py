import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Remove the header
content = content.replace("<th>Tanıdık</th>", "")

# Remove the main row td
main_pattern = r'<td>\s*@if\(!string\.IsNullOrEmpty\(doc\.InstitutionContact\)\) \{ <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @doc\.InstitutionContact</span> \} else \{ <span class="text-muted">-</span> \}\s*</td>'
content = re.sub(main_pattern, "", content)

# Remove the child row td
child_pattern = r'<td>\s*@if\(!string\.IsNullOrEmpty\(childDoc\.InstitutionContact\)\) \{ <span class="text-dark"><i class="bi bi-person-lines-fill text-muted"></i> @childDoc\.InstitutionContact</span> \} else \{ <span class="text-muted">-</span> \}\s*</td>'
content = re.sub(child_pattern, "", content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml (removed Tanıdık)")
