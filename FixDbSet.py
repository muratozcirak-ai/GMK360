import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add the dbset if missing
if 'public DbSet<SubcontractorHakedis> SubcontractorHakedisler' not in content and 'public DbSet<GMK360.Core.Entities.Finance.SubcontractorHakedis> SubcontractorHakedisler' not in content:
    content = content.replace('public DbSet<GMK360.Core.Entities.Finance.SubcontractorContract> SubcontractorContracts { get; set; }', 'public DbSet<GMK360.Core.Entities.Finance.SubcontractorContract> SubcontractorContracts { get; set; }\n        public DbSet<GMK360.Core.Entities.Finance.SubcontractorHakedis> SubcontractorHakedisler { get; set; }')

# And delete old ProgressPayments dbset if it's there
content = content.replace('public DbSet<GMK360.Core.Entities.Finance.ProgressPayment> ProgressPayments { get; set; }', '')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
