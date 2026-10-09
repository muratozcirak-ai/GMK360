import codecs

path = 'GMK360.Web/Views/Shared/_Layout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('Yapay Zeka Asistanı?</a>                <a', 'Yapay Zeka Asistanı</a>\n                <a')
content = content.replace('Yapay Zeka Asistanı?</a>', 'Yapay Zeka Asistanı</a>')
content = content.replace('Yapay Zeka Asistan?</a>', 'Yapay Zeka Asistanı</a>')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)