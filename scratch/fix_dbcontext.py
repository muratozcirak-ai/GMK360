import sys
filepath = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """public DbSet<GMK360.Core.Entities.ModuleDocumentRule> ModuleDocumentRules { get; set; }
        public DbSet<GMK360.Core.Entities.ModuleDocumentRulePrerequisite> ModuleDocumentRulePrerequisites { get; set; }"""
content = content.replace("public DbSet<GMK360.Core.Entities.ModuleDocumentRule> ModuleDocumentRules { get; set; }", replacement)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
