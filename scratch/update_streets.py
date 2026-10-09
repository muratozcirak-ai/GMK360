import re

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

pattern = r'builder\.Entity<Street>\(\)\.HasData\(.*?\);'
content = re.sub(pattern, '', content, flags=re.DOTALL)

with open(r'GMK360.Data\Contexts\ApplicationDbContext.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("ApplicationDbContext streets seed removed.")
