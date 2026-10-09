import codecs
import re

path = 'GMK360.Core/Entities/Construction/ConstructionProject.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'public string? BypassedPhases' not in content:
    content = content.replace(
        'public string? ProjectType { get; set; }',
        'public string? ProjectType { get; set; }\n        public string? BypassedPhases { get; set; } // Örn: "1,3,4" (Hard Lock Bypass edilen fazlar)'
    )
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)