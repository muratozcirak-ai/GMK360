with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'r', encoding='utf-8') as f:
    text = f.read()

if 'using System.Collections.Generic;' not in text:
    text = 'using System.Collections.Generic;\n' + text
    with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print('Added using statement')
