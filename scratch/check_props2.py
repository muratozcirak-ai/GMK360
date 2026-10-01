import codecs
import re

path2 = 'GMK360.Core/Entities/Agency.cs'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    print('Agency: ' + ' '.join(re.findall(r'public string (\w+)', f.read())))

path3 = 'GMK360.Core/Entities/AgencyConsultant.cs'
with codecs.open(path3, 'r', 'utf-8-sig') as f:
    print('AgencyConsultant: ' + ' '.join(re.findall(r'public string (\w+)', f.read())))
