import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Remove the leftover hidden fields precisely
content = re.sub(r'<input type="hidden" asp-for="ProjectOriginId" />\s*', '', content)
content = re.sub(r'<input type="hidden" asp-for="Name" />\s*', '', content)

# Now, add the onchange event to the ProjectOriginId select
old_select = '<select asp-for="ProjectOriginId" class="form-select form-select-lg rounded-3 border-danger" required>'
new_select = '<select asp-for="ProjectOriginId" id="ProjectOriginId" class="form-select form-select-lg rounded-3 border-danger" required onchange="document.getElementById(\'kentselDonusumOldBlocks\').style.display = this.value == \'1\' ? \'block\' : \'none\';">'

if old_select in content:
    content = content.replace(old_select, new_select)
    with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Fixed!")
else:
    print("Could not find the select tag to replace!")

