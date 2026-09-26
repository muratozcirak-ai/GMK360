import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

html = html.replace('@Model.CoverImageUrl', '@(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)')
html = html.replace('!string.IsNullOrEmpty(Model.CoverImageUrl)', '!string.IsNullOrEmpty(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)')

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)

print("SUCCESS")
