import codecs

path = 'GMK360.Web/Views/Shared/_Layout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('?</a>
                <a asp-controller="TaxAssistant"', '</a>\n                <a asp-controller="TaxAssistant"')
# In case there's another occurrence of literal 

content = content.replace('
', '\n')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)