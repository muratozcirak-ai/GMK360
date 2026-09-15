import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('/ConstructionProject/Dashboard/', '/ConstructionProject/Details/')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
