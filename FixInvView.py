import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Inventory\Index.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

layout_old = 'Layout = "~/Views/Shared/_ConstructionLayout.cshtml";'
layout_new = 'Layout = ViewData["ProjectId"] != null ? "~/Views/Shared/_ProjectLayout.cshtml" : "~/Views/Shared/_ConstructionLayout.cshtml";'

content = content.replace(layout_old, layout_new)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
