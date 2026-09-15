import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Finance\AgencyStaffAdvance.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

replacement = '''public int? AgencyConsultantId { get; set; }
        public AgencyConsultant? Consultant { get; set; }

        public int? AgencyWorkerId { get; set; }
        public GMK360.Core.Entities.Construction.AgencyWorker? Worker { get; set; }'''

content = content.replace('public int AgencyConsultantId { get; set; }\n        public AgencyConsultant Consultant { get; set; }', replacement)
# In case it uses different spacing
content = content.replace('public int AgencyConsultantId { get; set; }\r\n        public AgencyConsultant Consultant { get; set; }', replacement)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
