import codecs
import re

path = 'GMK360.Core/Entities/Construction/DailyTimesheet.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

if 'PendingFieldExpense' not in content:
    replacement = '''
        public decimal PendingFieldExpense { get; set; } = 0; // Belge Bekleyen Saha Gideri (Elden Nakit)
        public string? FieldExpenseDescription { get; set; } // Örn: Yağmur öncesi hızlı döküm
        public decimal EarnedWage { get; set; }
'''
    content = content.replace('public decimal EarnedWage { get; set; }', replacement.strip() + '\n')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print("Updated DailyTimesheet.cs")