import sys
import re

filepath = 'GMK360.Web/Views/Admin/GlobalProviders.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "var categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;"
replacement = "IEnumerable<GMK360.Core.Entities.DefinitionValue> categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;"

content = content.replace(target, replacement)

target2 = "var cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>;"
replacement2 = "IEnumerable<GMK360.Core.Entities.City> cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>;"

content = content.replace(target2, replacement2)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
