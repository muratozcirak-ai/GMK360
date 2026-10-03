import codecs
import re

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public DbSet<SystemLegalDocumentTemplate> SystemLegalDocumentTemplates \{ get; set; \}'
replacement = 'public DbSet<SystemLegalDocumentTemplate> SystemLegalDocumentTemplates { get; set; }\n        public DbSet<SystemPhaseTemplate> SystemPhaseTemplates { get; set; }'

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)