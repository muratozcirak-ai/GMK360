import os

files_to_fix = [
    'GMK360.Web/Views/ConstructionProject/Details.cshtml',
    'GMK360.Web/Views/ConstructionProject/Amenities.cshtml'
]

replacements = {
    'Ã§': 'ç', 'Ã‡': 'Ç',
    'Ä±': 'ı', 'Ä°': 'İ',
    'ÅŸ': 'ş', 'Åž': 'Ş',
    'Ã¶': 'ö', 'Ã–': 'Ö',
    'Ã¼': 'ü', 'Ãœ': 'Ü',
    'ÄŸ': 'ğ', 'Äž': 'Ğ'
}

for file_path in files_to_fix:
    with open(file_path, 'r', encoding='utf-8') as f:
        text = f.read()
    
    for corrupted, correct in replacements.items():
        text = text.replace(corrupted, correct)
        
    with open(file_path, 'w', encoding='utf-8-sig') as f:
        f.write(text)
        
    print(f"Fixed Turkish characters in {file_path}")
