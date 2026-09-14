import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Agenda\GuestView.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@Model.TopicTitle', '@Model.Title')
content = content.replace('@Model.GeneralNotes', '@Model.Notes')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
