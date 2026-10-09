import re

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Extract Maliyet ve Finans link
pattern_finance = re.compile(r'<a href="/ProjectFinance.*?</a>', re.DOTALL)
match_finance = pattern_finance.search(content)

if not match_finance:
    print("Could not find Maliyet ve Finans")
    exit()

finance_html = match_finance.group(0)

# 2. Remove it from its original place
content = content[:match_finance.start()] + content[match_finance.end():]

# 3. Find Faz 0
pattern_faz0 = re.compile(r'<a href="/PhaseZero.*?</a>', re.DOTALL)
match_faz0 = pattern_faz0.search(content)

if not match_faz0:
    print("Could not find Faz 0")
    exit()

# 4. Insert Maliyet ve Finans right BEFORE Faz 0
content = content[:match_faz0.start()] + finance_html + '\n                            ' + content[match_faz0.start():]

with open(r'GMK360.Web\Views\Shared\_ProjectLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Reordered left menu successfully.")
