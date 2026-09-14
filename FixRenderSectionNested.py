import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the incorrectly added plain RenderSection with the proper nested section format
content = content.replace('@RenderSection("Scripts", required: false)', '')

content += '\n@section Scripts {\n    @RenderSection("Scripts", required: false)\n}\n'

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
