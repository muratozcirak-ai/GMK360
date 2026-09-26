import re

with open('GMK360.Core/Entities/Construction/ProjectAmenity.cs', 'r', encoding='utf-8') as f:
    code = f.read()

if 'public bool IsExisting' not in code:
    code = code.replace(
        'public bool IsCompleted { get; set; } = false;',
        'public bool IsCompleted { get; set; } = false;\n        public bool IsExisting { get; set; } = false;'
    )
    with open('GMK360.Core/Entities/Construction/ProjectAmenity.cs', 'w', encoding='utf-8') as f:
        f.write(code)
    print("ADDED IsExisting TO ProjectAmenity")
else:
    print("ALREADY EXISTS")
