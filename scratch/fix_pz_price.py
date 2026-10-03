import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@if(inv.OfferedPrice.HasValue) {', '@if(inv.OfferedPrice.HasValue && inv.OfferedPrice.Value > 0) {')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)