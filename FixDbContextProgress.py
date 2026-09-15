import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('ProgressPayments', 'Hakedisler')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
