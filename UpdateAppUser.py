import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Identity\ApplicationUser.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_fields = '''
        public bool HasMapConsent { get; set; } = false;
        public System.DateTime? MapConsentDate { get; set; }
'''

content = content.replace('public string? RecoveryQuestion { get; set; }', 'public string? RecoveryQuestion { get; set; }\n' + new_fields)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
