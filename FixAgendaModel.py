import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\Index.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

header = '@model IEnumerable<GMK360.Core.Entities.Construction.AgendaRecord>\r\n'

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(header + content)
