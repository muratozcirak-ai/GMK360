import codecs

filepaths = [
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SupplierCurrentAccount\Index.cshtml',
    r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SupplierCurrentAccount\Details.cshtml'
]

for filepath in filepaths:
    with codecs.open(filepath, 'r', 'utf-8-sig') as f:
        content = f.read()
    
    if 'Layout = ' not in content:
        content = content.replace('ViewData["Title"] = ', 'Layout = "~/Views/Shared/_ConstructionLayout.cshtml";\n    ViewData["Title"] = ')
        with codecs.open(filepath, 'w', 'utf-8-sig') as f:
            f.write(content)
