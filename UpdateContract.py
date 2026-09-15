import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Finance\SubcontractorContract.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('public ICollection<ProgressPayment> ProgressPayments { get; set; }', 'public virtual ICollection<SubcontractorHakedis> Hakedisler { get; set; }')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
