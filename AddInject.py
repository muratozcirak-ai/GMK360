import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_top = '''@using Microsoft.AspNetCore.Identity
@using GMK360.Core.Entities.Identity
@inject UserManager<ApplicationUser> UserManager
@{
    Layout = "_Layout";
}'''

content = content.replace('@{\r\n    Layout = "_Layout";\r\n}', new_top).replace('@{\n    Layout = "_Layout";\n}', new_top)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
