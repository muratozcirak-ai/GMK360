import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\ConstructionProject.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('public int? TargetTotalShops { get; set; }', 'public int? TargetTotalShops { get; set; }\n        public int? TotalFloors { get; set; } // Bina Kaç Katlı\n')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
