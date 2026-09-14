import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

if '@RenderSection("Scripts", required: false)' not in content:
    content += '\n@RenderSection("Scripts", required: false)\n'

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
