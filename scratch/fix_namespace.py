import codecs

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('GMK360.Core.Entities.B2B.B2BNetworkConnection', 'GMK360.Core.Entities.B2b.B2BNetworkConnection')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed namespace case!')