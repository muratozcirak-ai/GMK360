import codecs

path = 'GMK360.Core/Entities/B2B/B2BQuoteInvite.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = 'public DateTime? RespondedAt { get; set; }'
replacement = '''public DateTime? RespondedAt { get; set; }
        public bool IsFeasibilitySelected { get; set; } = false; // Faz 0'da fizibilite için baz alınan teklif mi?'''

content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)