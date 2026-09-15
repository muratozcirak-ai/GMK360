import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\AgencyWorker.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('GMK360.Core.Entities.AgencyPhonebook', 'AgencyPhonebook')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
