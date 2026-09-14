import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

config = '''
            builder.Entity<GMK360.Core.Entities.Construction.AgendaParticipant>()
                .HasOne(ap => ap.Phonebook)
                .WithMany()
                .HasForeignKey(ap => ap.PhonebookId)
                .OnDelete(DeleteBehavior.Restrict);
'''

# Find OnModelCreating
if 'builder.Entity<GMK360.Core.Entities.Construction.AgendaParticipant>' not in content:
    content = content.replace('base.OnModelCreating(builder);', 'base.OnModelCreating(builder);\r\n' + config)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
