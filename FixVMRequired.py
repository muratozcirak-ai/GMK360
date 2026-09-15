import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('[Required(ErrorMessage = "le seimi zorunludur.")]\n        public int DistrictId { get; set; }', 'public int? DistrictId { get; set; }')
content = content.replace('[Required(ErrorMessage = "Mahalle seimi zorunludur.")]\n        public int NeighborhoodId { get; set; }', 'public int? NeighborhoodId { get; set; }')
content = content.replace('[Required(ErrorMessage = "Sokak seimi zorunludur.")]\n        public int StreetId { get; set; }', 'public int? StreetId { get; set; }')
content = content.replace('[Required(ErrorMessage = "l seimi zorunludur.")]\n        public int CityId { get; set; }', 'public int? CityId { get; set; }')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
