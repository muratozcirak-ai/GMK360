import re

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

target = '''builder.Entity<ModuleDocumentRulePrerequisite>()
                .HasOne(x => x.PrerequisiteTemplate)
                .WithMany()
                .HasForeignKey(x => x.PrerequisiteTemplateId)
                .OnDelete(DeleteBehavior.Cascade);'''

replacement = '''builder.Entity<ModuleDocumentRulePrerequisite>()
                .HasOne(x => x.PrerequisiteTemplate)
                .WithMany()
                .HasForeignKey(x => x.PrerequisiteTemplateId)
                .OnDelete(DeleteBehavior.Restrict);'''

content = content.replace(target, replacement)

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("ApplicationDbContext updated.")
