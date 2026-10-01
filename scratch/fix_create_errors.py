import codecs

path = 'GMK360.Web/Views/SubcontractorContract/Create.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@s.CompanyName', '@s.Name')
content = content.replace('@s.ContactPerson', '@s.AuthorizedPerson')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed compilation errors in Create.cshtml')