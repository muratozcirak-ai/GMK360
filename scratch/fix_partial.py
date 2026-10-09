import re

with open(r'GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('Layout = "~/Views/Shared/_ConstructionLayout.cshtml";', '')

with open(r'GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
