import re

with open(r"GMK360.Web\Views\ConstructionProject\Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# 1. Remove Proje Kaynağı block completely
kaynak_pattern = r'<div class="col-md-6">\s*<label class="form-label fw-bold text-success"><i class="bi bi-diagram-3"></i> Proje Kaynağı / Mülkiyet</label>\s*<select asp-for="ProjectOriginId" .*?</select>\s*</div>'
content = re.sub(kaynak_pattern, '', content, flags=re.DOTALL)

# 2. Remove required from EndDate
end_date_pattern = r'(<input name="EndDate" id="EndDate" type="date" .*?) required( />)'
content = re.sub(end_date_pattern, r'\1\2', content)

# 3. Prevent auto-skipping to Step 2
# Find all occurrences of nextStep(2); inside DOMContentLoaded and remove them.
# The exact pattern is inside the try catch draftId > 0 block.
skip_pattern = r'if \(document\.getElementById\(\'blocksContainer\'\).*?setTimeout\(\(\) => \{.*?nextStep\(2\);.*?nextStep\(2\);.*?\}, 50\);'
# Wait, just replace nextStep(2); inside that block with nothing.
# Actually, I can just remove the whole draftId > 0 auto-skip block.
auto_skip_block = r'// On page load: if we have a draft id.*?document\.addEventListener\(\'DOMContentLoaded\', function\(\) \{\s*try \{\s*const draftIdField = document\.getElementById\(\'DraftProjectId\'\);.*?\}\s*\);'
content = re.sub(auto_skip_block, '', content, flags=re.DOTALL)

# But wait, we still need nextStep(2) for the AJAX success of SaveStep1.
# SaveStep1 success: nextStep(2); -> do NOT remove this one.
# So I'll just remove the DOMContentLoaded listeners that do nextStep(2)

dom_content_loaded_pattern = r"// On page load: if we have a draft id.*?document\.addEventListener\('DOMContentLoaded', function\(\) \{.*?nextStep\(2\);.*?nextStep\(2\);.*?\n\s*\}\);\n"

content = re.sub(dom_content_loaded_pattern, '', content, flags=re.DOTALL)


with open(r"GMK360.Web\Views\ConstructionProject\Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Create.cshtml updated.")
