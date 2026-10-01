import codecs

path = 'GMK360.Web/Views/SubcontractorContract/Create.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace(
    'IEnumerable<GMK360.Core.Entities.B2B.B2BNetworkContact>', 
    'IEnumerable<GMK360.Core.Entities.Construction.AgencyPhonebook>'
)

content = content.replace('@sub.CompanyName', '@sub.Name')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Fixed Create.cshtml view!")