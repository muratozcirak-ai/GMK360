import re

with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'r', encoding='utf-8') as f:
    code = f.read()

if 'public string? Stage { get; set; }' not in code:
    code = code.replace(
        'public string Name { get; set; }',
        'public string Name { get; set; }\n        public string? Stage { get; set; }'
    )
    with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'w', encoding='utf-8') as f:
        f.write(code)
    print("ADDED Stage TO SystemLegalDocumentTemplate")
else:
    print("ALREADY EXISTS")
