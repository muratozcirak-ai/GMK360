import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('public DbSet<GMK360.Core.Entities.Construction.DocumentArchive> DocumentArchives { get; set; }', '')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
