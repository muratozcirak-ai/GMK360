import codecs

# Entity Update: VehicleExpense (PhotoPath)
path1 = 'GMK360.Core/Entities/Logistics/VehicleExpense.cs'
with codecs.open(path1, 'r', 'utf-8-sig') as f:
    content1 = f.read()

if 'PhotoPath' not in content1:
    content1 = content1.replace('public int? OdometerAtExpense { get; set; }', '''public int? OdometerAtExpense { get; set; }
        
        // Yeni Eklenen: Fiş Fotoğrafı
        public string? PhotoPath { get; set; }''')
    with codecs.open(path1, 'w', 'utf-8-sig') as f:
        f.write(content1)

# Entity Update: VehicleTask (StartTime, EndTime)
path2 = 'GMK360.Core/Entities/Logistics/VehicleTask.cs'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    content2 = f.read()

if 'StartTime' not in content2:
    content2 = content2.replace('public int? StartKm { get; set; }', '''public string? StartTime { get; set; } // Örn: 09:00
        public string? EndTime { get; set; } // Örn: 12:30
        public int? StartKm { get; set; }''')
    with codecs.open(path2, 'w', 'utf-8-sig') as f:
        f.write(content2)

print('Entities updated with PhotoPath and Time ranges.')