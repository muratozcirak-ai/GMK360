import codecs
import re

path = 'GMK360.Core/Entities/Construction/AgencyWorker.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add UserId
if 'public string? UserId' not in content:
    replacement = '''
        public string? UserId { get; set; }
        public GMK360.Core.Entities.Identity.ApplicationUser? User { get; set; }
        public string FirstName { get; set; }
'''
    content = content.replace('public string FirstName { get; set; }', replacement.strip() + '\n')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated AgencyWorker.cs")