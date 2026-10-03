import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<!-- FAZ 1 -->.*?<a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">'
replacement = '''<!-- FAZ 1 -->
          <div class="col-md-4 col-lg-3">
              <a href="/PhaseOne/Index/@Model.Id" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)