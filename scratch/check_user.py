import codecs
import re
with codecs.open('GMK360.Core/Entities/Identity/ApplicationUser.cs', 'r', 'utf-8-sig') as f:
    print(' '.join(re.findall(r'public string\?? (\w+)', f.read())))