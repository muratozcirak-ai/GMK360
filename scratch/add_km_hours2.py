import codecs

path = 'GMK360.Core/Entities/Logistics/CompanyVehicle.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()
if 'CurrentKm' not in content:
    content = content.replace('public ICollection<VehicleAssignment> Assignments { get; set; }', '''
        // KM ve Saat Takibi
        public int CurrentKm { get; set; } = 0;
        public decimal CurrentWorkingHours { get; set; } = 0;
        
        public ICollection<VehicleAssignment> Assignments { get; set; }''')
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)

path2 = 'GMK360.Core/Entities/Logistics/VehicleTask.cs'
with codecs.open(path2, 'r', 'utf-8-sig') as f:
    content2 = f.read()
if 'StartKm' not in content2:
    idx = content2.rfind('}')
    idx2 = content2.rfind('}', 0, idx)
    insertion = '''
        public int? StartKm { get; set; }
        public int? EndKm { get; set; }
        public decimal? WorkingHours { get; set; }
'''
    content2 = content2[:idx2] + insertion + content2[idx2:]
    with codecs.open(path2, 'w', 'utf-8-sig') as f:
        f.write(content2)
print('Added KM properties.')