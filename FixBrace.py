import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_Layout.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

target = '''<a href="#" class="text-decoration-none text-dark hover-orange">Hizmetler</a>
            </nav>'''
replace = '''<a href="#" class="text-decoration-none text-dark hover-orange">Hizmetler</a>
                }
            </nav>'''

content = content.replace(target, replace)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
