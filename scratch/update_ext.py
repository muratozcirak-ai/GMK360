import re

with open(r'GMK360.Data\Contexts\ModelBuilderExtensions.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

pattern = r'modelBuilder\.Entity<City>\(\)\.HasData\(.*?\);\s*// 5\. Districts.*?modelBuilder\.Entity<District>\(\)\.HasData\(.*?\);'
content = re.sub(pattern, '// Cities and Districts are seeded in ApplicationDbContext directly.\n', content, flags=re.DOTALL)

with open(r'GMK360.Data\Contexts\ModelBuilderExtensions.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("ModelBuilderExtensions cleaned.")
