import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

pattern = r'\s*\}\s*\}\s*</tbody>'
replacement = '\n                                  }\n                              }\n                          }\n                      </tbody>'

if re.search(pattern, html, flags=re.DOTALL):
    html = re.sub(pattern, replacement, html, count=1, flags=re.DOTALL)
    print("MATCH 2 SUCCESS")
else:
    print("MATCH 2 NOT FOUND")

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
