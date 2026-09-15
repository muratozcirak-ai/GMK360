import re

semt = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']+)\'\)')
print(bool(semt.match("(1, 1, 'Aladağ')")))

mah = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']*)\',\s*\'([^\']*)\'\)')
print(bool(mah.match("(27701, 1006, 'Çimenli Mah', '25530')")))
