import codecs

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('    }\n}\n}', '    }\n}')
content = content.replace('    }\r\n}\r\n}', '    }\r\n}')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)