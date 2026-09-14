import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\_AgendaDetails.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@Model.Date.ToString(', '@Model.EventDate.ToString(')
content = content.replace('@Model.Description', '@Model.Notes')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
