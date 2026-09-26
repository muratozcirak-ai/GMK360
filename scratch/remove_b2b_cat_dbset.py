import sys

filepath = 'GMK360.Data/Contexts/ApplicationDbContext.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("public DbSet<GMK360.Core.Entities.B2b.B2bCategory> B2bCategories { get; set; }", "// DefinitionValues kullanılıyor")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
