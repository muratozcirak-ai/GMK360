import sys

filepath = 'GMK360.Web/Controllers/ConstructionProjectController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "ViewBag.Phase0Docs = phase0Raw;"
replacement = """ViewBag.Phase0Docs = phase0Raw;
            ViewBag.GlobalRules = globalRules;"""

content = content.replace(target, replacement)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
