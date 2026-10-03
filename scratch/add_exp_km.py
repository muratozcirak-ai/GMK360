import codecs

path = 'GMK360.Core/Entities/Logistics/VehicleExpense.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'OdometerAtExpense' not in content:
    content = content.replace('public decimal Amount { get; set; }', '''public decimal Amount { get; set; }
        
        // Yeni Eklenen: Masraf (Fiş/Fatura) anındaki Kilometre
        public int? OdometerAtExpense { get; set; }''')
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
print('Added OdometerAtExpense to Entity.')