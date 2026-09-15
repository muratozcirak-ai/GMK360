import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\AgencyWorker.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add NetDailyWage and DailySgkCost, we can keep DefaultDailyWage or replace it.
content = content.replace('public decimal DefaultDailyWage { get; set; }', 
                          'public decimal DefaultDailyWage { get; set; }\n        public decimal NetDailyWage { get; set; } // İşçinin Cebine Giren\n        public decimal DailySgkCost { get; set; } // Şirketin SGK Yükü')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

filepath_consultant = r'C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\AgencyConsultant.cs'
with codecs.open(filepath_consultant, 'r', 'utf-8-sig') as f:
    consultant_content = f.read()

if 'MonthlySgkCost' not in consultant_content:
    consultant_content = consultant_content.replace('public decimal MonthlySalary { get; set; } = 0;',
                              'public decimal MonthlySalary { get; set; } = 0;\n        public decimal MonthlySgkCost { get; set; } = 0; // Şirketin SGK Yükü')
    with codecs.open(filepath_consultant, 'w', 'utf-8-sig') as f:
        f.write(consultant_content)

