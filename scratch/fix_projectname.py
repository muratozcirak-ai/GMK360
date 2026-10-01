import codecs

# Fix Controller
path1 = 'GMK360.Web/Controllers/ConstructionProjectExpensesController.cs'
with codecs.open(path1, 'r', 'utf-8-sig') as f:
    content1 = f.read()

content1 = content1.replace('"ProjectName"', '"Name"')

with codecs.open(path1, 'w', 'utf-8-sig') as f:
    f.write(content1)

# Fix View
path2 = 'GMK360.Web/Views/ConstructionProjectExpenses/Index.cshtml'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    content2 = f.read()

content2 = content2.replace('item.Project?.ProjectName', 'item.Project?.Name')

with codecs.open(path2, 'w', 'utf-8-sig') as f:
    f.write(content2)

print('Fixed ProjectName to Name')