import codecs

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('if (renderedDocIds.Contains(doc.Id)) { continue; }', '')
# Also remove the Add calls just to be clean
content = content.replace('renderedDocIds.Add(doc.Id);', '')
content = content.replace('renderedDocIds.Add(childDoc.Id);', '')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)