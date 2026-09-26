import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace the first two occurrences (which are in the first block)
html = html.replace('Model.CoverImageUrl', 'Model.CurrentStateImageUrl', 2)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("Done")
