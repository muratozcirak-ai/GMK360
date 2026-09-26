import re
with open(r'GMK360.Data\Migrations\20260914083841_InitialCreate.cs', 'rb') as f:
    text = f.read()

# Let's decode with windows-1254 to see if it works
text_1254 = text.decode('windows-1254', errors='ignore')
if 'Türkiye' in text_1254:
    print('Found Türkiye in windows-1254')
if 'İlk' in text_1254:
    print('Found İlk in windows-1254')
    
# Let's decode with utf-8
text_utf8 = text.decode('utf-8', errors='ignore')
if 'Türkiye' in text_utf8:
    print('Found Türkiye in utf-8')
if 'İlk' in text_utf8:
    print('Found İlk in utf-8')
