import codecs
import re

path = 'GMK360.Core/Entities/Construction/ProjectLegalDocument.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('public string? FilePath { get; set; }', 'public string? FilePath { get; set; }\n        public string? OriginalLocation { get; set; } // YENİ: Orjinal Evrak Nerede (Çekmece vs.)')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)