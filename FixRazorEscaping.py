import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ProjectLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

content = content.replace('src="https://unpkg.com/@phosphor-icons/web"', 'src="https://unpkg.com/@@phosphor-icons/web"')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
