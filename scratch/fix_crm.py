import codecs

path = 'GMK360.Web/Controllers/ProjectCrmController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('using Microsoft.AspNetCore.Identity;', 'using Microsoft.AspNetCore.Identity;\nusing GMK360.Core.Entities.Identity;')
content = content.replace('Identity.ApplicationUser', 'ApplicationUser')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed CRM Controller using.')