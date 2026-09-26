import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

replacements = {
    'Ã§': 'ç', 'Ã‡': 'Ç',
    'Ä±': 'ı', 'Ä°': 'İ',
    'ÅŸ': 'ş', 'Åž': 'Ş',
    'Ã¶': 'ö', 'Ã–': 'Ö',
    'Ã¼': 'ü', 'Ãœ': 'Ü',
    'ÄŸ': 'ğ', 'Äž': 'Ğ'
}

for corrupted, correct in replacements.items():
    text = text.replace(corrupted, correct)

with open('scratch/Details_test.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)

print("Done")
