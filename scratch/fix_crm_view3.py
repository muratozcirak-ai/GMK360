import codecs
path = 'GMK360.Web/Views/AdminCRM/UserDetails.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@r.Agency.Name', '@r.Agency.CompanyName')
content = content.replace('@r.AgencyRole', '@r.Role.ToString()')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed View properties!')