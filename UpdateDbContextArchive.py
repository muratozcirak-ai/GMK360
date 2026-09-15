import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

dbsets = '''
        public DbSet<GMK360.Core.Entities.Construction.DocumentArchive> DocumentArchives { get; set; }
        public DbSet<GMK360.Core.Entities.Construction.DocumentTemplate> DocumentTemplates { get; set; }
'''

if 'public DbSet<GMK360.Core.Entities.Construction.DocumentArchive>' not in content:
    content = content.replace('public DbSet<GMK360.Core.Entities.Construction.AgendaParticipant> AgendaParticipants { get; set; }', 'public DbSet<GMK360.Core.Entities.Construction.AgendaParticipant> AgendaParticipants { get; set; }\r\n' + dbsets)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
