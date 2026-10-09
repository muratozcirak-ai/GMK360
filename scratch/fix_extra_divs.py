with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

# Remove the extra </div> that follows the newly inserted <div class="sub-blocks-container...</div>
content = re.sub(r'(<div class="sub-blocks-container[^>]*>.*?</div>\s*)</div>', r'\1', content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Removed extra divs!")
