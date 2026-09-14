import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'public DbSet<GMK360.Core.Entities.Construction.AgendaParticipant> AgendaParticipants { get; set; }' not in content:
    content = content.replace('public DbSet<GMK360.Core.Entities.Construction.AgendaItem> AgendaItems { get; set; }', 
                              'public DbSet<GMK360.Core.Entities.Construction.AgendaItem> AgendaItems { get; set; }\r\n          public DbSet<GMK360.Core.Entities.Construction.AgendaParticipant> AgendaParticipants { get; set; }')
    
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
