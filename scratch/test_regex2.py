import re
regex = re.compile(r'^\((\d+),\s*(\d+),\s*\'([^\']+)\'\)')
line = "(1, 1, 'Aladağ'),"
match = regex.match(line)
print("Match semt:", bool(match))
if match:
    print(match.groups())
