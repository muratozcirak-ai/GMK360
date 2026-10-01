import codecs
path = 'GMK360.Web/Views/AdminCRM/UserDetails.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@model GMK360.Core.Entities.ApplicationUser', '@model GMK360.Core.Entities.Identity.ApplicationUser')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed ApplicationUser namespace in view')