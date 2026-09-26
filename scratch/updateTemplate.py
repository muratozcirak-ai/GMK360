import re

with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'r', encoding='utf-8') as f:
    text = f.read()

injection = '''
        // Bu evrakı alabilmek için HANGİ EVRAKLAR önceden alınmış olmalı? (Kilit listesi)
        public ICollection<SystemDocumentDependency> Prerequisites { get; set; } = new List<SystemDocumentDependency>();

        // Bu evrak BAŞKA HANGİ evrakların kilidini açıyor? (Ben bitersem kimler açılır listesi)
        public ICollection<SystemDocumentDependency> DependentDocuments { get; set; } = new List<SystemDocumentDependency>();
'''

# Find the closing brace of the class
if 'public ICollection<SystemDocumentDependency> Prerequisites' not in text:
    last_brace_index = text.rfind('}')
    class_brace_index = text.rfind('}', 0, last_brace_index)
    
    text = text[:class_brace_index] + injection + text[class_brace_index:]
    
    with open('GMK360.Core/Entities/SystemLegalDocumentTemplate.cs', 'w', encoding='utf-8-sig') as f:
        f.write(text)
    print("Injected collections into SystemLegalDocumentTemplate")
else:
    print("Already injected")
