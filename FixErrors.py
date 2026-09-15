import codecs

# Fix Details.cshtml
filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('Replace(""_"", "" "")', 'Replace(""_"", "" "")') # Wait, Razor syntax in C# is Replace(""_"", "" "")? No it's Replace(""_"", "" ""). I need to just write Replace(""_"", "" ""). No, just one quote!
content = content.replace('Replace(\"\"_\"\", \"\" \"\")', 'Replace(\"_\", \" \")')
content = content.replace('\"\"Henüz açıklama girilmedi.\"\"', '\"Henüz açıklama girilmedi.\"')
content = content.replace('allowfullscreen=\"\"\"\"', 'allowfullscreen=\"\"')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

# Fix Layout
filepath_layout = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath_layout, 'r', 'utf-8-sig') as f:
    layout = f.read()

# I might have placed inject at the wrong place.
layout = layout.replace('@using Microsoft.AspNetCore.Identity\n@using GMK360.Core.Entities.Identity\n@inject UserManager<ApplicationUser> UserManager\n@using Microsoft.AspNetCore.Http', '@using Microsoft.AspNetCore.Http\n@using Microsoft.AspNetCore.Identity\n@using GMK360.Core.Entities.Identity\n@inject UserManager<ApplicationUser> UserManager')

layout = layout.replace('name=\"\"__RequestVerificationToken\"\"', 'name=\"__RequestVerificationToken\"')

with codecs.open(filepath_layout, 'w', 'utf-8-sig') as f:
    f.write(layout)
