import codecs

def update_layout(filepath):
    with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
        content = f.read()
    
    content = content.replace('Layout = "~/Views/Shared/_ConstructionLayout.cshtml";', 'Layout = "~/Views/Shared/_ProjectLayout.cshtml";')
    
    with codecs.open(filepath, 'w', 'utf-8') as f:
        f.write(content)

update_layout(r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml')
update_layout(r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManagePhases.cshtml')

