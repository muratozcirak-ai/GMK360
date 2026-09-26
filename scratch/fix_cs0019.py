import io

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('b.BasementFloors ?? 0', 'b.BasementFloors')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed CS0019")
