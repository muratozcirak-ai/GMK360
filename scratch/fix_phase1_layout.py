import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'Layout = "~/Views/Shared/_ConstructionLayout\.cshtml";'
replacement = 'Layout = "~/Views/Shared/_ProjectLayout.cshtml";'
content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)