import codecs
import re

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'ConstructionProjectExpenses' not in content:
    replacement = '''
        public DbSet<ConstructionBudgetItem> ConstructionBudgetItems { get; set; }
        public DbSet<ConstructionProjectExpense> ConstructionProjectExpenses { get; set; }
'''
    content = content.replace('public DbSet<ConstructionBudgetItem> ConstructionBudgetItems { get; set; }', replacement.strip() + '\n')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated ApplicationDbContext.cs")