import codecs

path = 'GMK360.Data/Contexts/ApplicationDbContext.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = 'public DbSet<GMK360.Core.Entities.Construction.SiteDailyLog> SiteDailyLogs { get; set; }'
replacement = target + '\n        public DbSet<GMK360.Core.Entities.Construction.ConstructionProjectExpense> ConstructionProjectExpenses { get; set; }'

content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated DbContext safely!')