import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# The target block starts with <!-- ŞANTİYE OLAY GÜNLÜĞÜ --> and ends right before <!-- FİNANSAL RÖNTGEN -->
pattern = re.compile(r'<!-- ŞANTİYE OLAY GÜNLÜĞÜ -->.*?<!-- FİNANSAL RÖNTGEN -->', re.DOTALL)

# Let's double check if it uses turkish characters or not
pattern_alt = re.compile(r'<!-- \xdeANT\xddYE OLAY G\xdcNL\xdc\xdc -->.*?<!-- F\xddNANSAL R\xd6NTGEN -->', re.DOTALL) # in case it's some other encoding
# Better yet, search by fragments:

start_marker = '<!--'
end_marker = '<!--'

idx1 = content.find('ŞANTİYE OLAY')
if idx1 == -1:
    idx1 = content.find('ANTIYE OLAY')
if idx1 == -1:
    # Just use regex on the div structure if comments are mangled
    pass

