import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()
    
# Remove everything starting from '<div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden mb-4" id="collapseFaz0">' until the end of the phases accordion or just rip out the whole accordion section
pattern = r'<div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden mb-4".*?Faz 0: Yasal Evrak, İzin ve Sözleşme Takibi.*?</div>\s*</div>\s*</div>\s*</div>\s*</div>'
content = re.sub(pattern, '', content, flags=re.DOTALL)

# Let's just remove anything referencing Model.Phases
content = re.sub(r'@foreach\s*\(var phase in Model\.Phases.*?\)\s*\{.*?(?:<div.*?</div>\s*)+\}', '', content, flags=re.DOTALL)
content = re.sub(r'Model\.Phases', 'new List<object>()', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Cleaned Details.cshtml")
