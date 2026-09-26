import re

with open('GMK360.Data/Contexts/ApplicationDbContext.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Add DbSet
dbset_injection = '        public DbSet<SystemDocumentDependency> SystemDocumentDependencies { get; set; }\n'
if 'DbSet<SystemDocumentDependency>' not in text:
    target = 'public DbSet<SystemLegalDocumentTemplate> SystemLegalDocumentTemplates { get; set; }'
    text = text.replace(target, target + '\n' + dbset_injection)

# Add Fluent API in OnModelCreating
fluent_api = '''
            // Document Dependencies
            modelBuilder.Entity<SystemDocumentDependency>()
                .HasOne(d => d.TargetDocument)
                .WithMany(t => t.Prerequisites)
                .HasForeignKey(d => d.TargetDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SystemDocumentDependency>()
                .HasOne(d => d.PrerequisiteDocument)
                .WithMany(t => t.DependentDocuments)
                .HasForeignKey(d => d.PrerequisiteDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
'''

if 'modelBuilder.Entity<SystemDocumentDependency>()' not in text:
    target = 'base.OnModelCreating(modelBuilder);'
    text = text.replace(target, target + '\n' + fluent_api)

with open('GMK360.Data/Contexts/ApplicationDbContext.cs', 'w', encoding='utf-8-sig') as f:
    f.write(text)
print('DbContext updated')
