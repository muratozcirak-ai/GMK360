import io
filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
for line in lines:
    if 'TaskDocument' in line or 'TaskMessage' in line:
        continue
    new_lines.append(line)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Cleaned DbSets 2")
