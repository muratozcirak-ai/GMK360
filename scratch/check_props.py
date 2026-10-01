import codecs
import re

# Check StakeholderRole
path1 = 'GMK360.Core/Entities/Construction/ProjectStakeholder.cs'
with codecs.open(path1, 'r', 'utf-8-sig') as f:
    print('StakeholderRoles: ' + re.search(r'public enum StakeholderRole\s*\{([^}]+)\}', f.read()).group(1).replace('\n', ' '))

# Check Agency
path2 = 'GMK360.Core/Entities/B2B/Agency.cs'
try:
    with codecs.open(path2, 'r', 'utf-8-sig') as f:
        print('Agency: ' + ' '.join(re.findall(r'public string (\w+)', f.read())))
except:
    pass

# Check AgencyConsultant
path3 = 'GMK360.Core/Entities/B2B/AgencyConsultant.cs'
try:
    with codecs.open(path3, 'r', 'utf-8-sig') as f:
        print('AgencyConsultant: ' + ' '.join(re.findall(r'public string (\w+)', f.read())))
except:
    pass
