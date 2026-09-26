import re

filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("public DbSet<ConstructionProject> ConstructionProjects { get; set; }", "public DbSet<ConstructionProject> ConstructionProjects { get; set; }\n        public DbSet<GMK360.Core.Entities.Construction.ConstructionBudgetItem> ConstructionBudgetItems { get; set; }")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
