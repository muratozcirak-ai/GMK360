import re
regex = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']*)\',\s*\'([^\']*)\'\)')
line = "(46144, 1819, 'Gökeşme Köyü', '40302'),"
match = regex.match(line)
print("Match:", bool(match))
if match:
    print(match.groups())
