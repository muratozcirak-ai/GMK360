import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('builder.Entity<GMK360.Core.Entities.Finance.ProgressPayment>().HasOne(p => p.Contract).WithMany(c => c.Hakedisler).HasForeignKey(p => p.SubcontractorContractId).OnDelete(DeleteBehavior.Restrict);', 'builder.Entity<GMK360.Core.Entities.Finance.SubcontractorHakedis>().HasOne(p => p.Contract).WithMany(c => c.Hakedisler).HasForeignKey(p => p.ContractId).OnDelete(DeleteBehavior.Restrict);')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
