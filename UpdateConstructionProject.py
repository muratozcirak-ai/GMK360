import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\ConstructionProject.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace StatusId and ProjectConstants
content = content.replace('public byte StatusId { get; set; } = ProjectConstants.StatusTeklif;', 'public ProjectStatus Status { get; set; } = ProjectStatus.Projelendirme_Teklif;\n        public string? PublicDescription { get; set; }\n        public string? GoogleMapsUrl { get; set; }')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
