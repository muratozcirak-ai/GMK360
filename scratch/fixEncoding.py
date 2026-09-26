import os

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

replacements = {
    'Ã‡': 'Ç',
    'Ã§': 'ç',
    'Ã–': 'Ö',
    'Ã¶': 'ö',
    'Ãœ': 'Ü',
    'Ã¼': 'ü',
    'Ä±': 'ı',
    'Ä°': 'İ',
    'ÅŸ': 'ş',
    'Åž': 'Ş',
    'ÄŸ': 'ğ',
    'Äž': 'Ğ',
    'Ã¢': 'â',
    'Ã®': 'î',
    'Ã»': 'û'
}

for bad, good in replacements.items():
    text = text.replace(bad, good)

# Also fix the weird ones I saw in previous logs like Y, Ǭ, 
bad_replacements2 = {
    'Y': 'ş',
    '-': 'Ö',
    'o': 'ü',
    'Ǭ': 'ü',
    'klamas': 'ıklaması',
    '': 'i', # wait, this is dangerous, let's be careful
}
# I will only fix the UTF-8 mojibake (Ã‡ etc.) first because that's what's currently in the file due to the recent python rewrite!

with open(path, 'w', encoding='utf-8') as f:
    f.write(text)

print("Fixed encoding mojibake")
