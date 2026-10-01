import codecs

path = 'GMK360.Web/Controllers/AdminCRMController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the closing brace of the class with nothing, and add it at the very end
content = content.replace('        ViewBag.Properties = properties;\n\n            return View(user);\n        }\n    }', '        ViewBag.Properties = properties;\n\n            return View(user);\n        }')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed class braces!')