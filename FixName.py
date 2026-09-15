import codecs
import os

old_filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Finance\PaymentTransaction.cs'
new_filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Finance\AgencyCashTransaction.cs'

with codecs.open(old_filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('PaymentTransactionType', 'AgencyCashTransactionType')
content = content.replace('public class PaymentTransaction : BaseEntity', 'public class AgencyCashTransaction : BaseEntity')

with codecs.open(new_filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

os.remove(old_filepath)

# Now fix ApplicationDbContext
db_filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(db_filepath, 'r', 'utf-8-sig') as f:
    db_content = f.read()

db_content = db_content.replace('public DbSet<GMK360.Core.Entities.Finance.PaymentTransaction> PaymentTransactions { get; set; }', 
                                'public DbSet<GMK360.Core.Entities.Finance.AgencyCashTransaction> AgencyCashTransactions { get; set; }')

with codecs.open(db_filepath, 'w', 'utf-8-sig') as f:
    f.write(db_content)
