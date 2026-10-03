import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# The child quote TR is currently:
# <tr class="bg-light">
# We need to change it to:
# <tr class="collapse child-of-@doc.Id bg-light">
# But ONLY for the childQuote block!

parts = content.split('var childQuote =')
if len(parts) > 1:
    child_block = parts[1]
    # Replace the first <tr class="bg-light"> with <tr class="collapse child-of-@doc.Id bg-light">
    child_block = child_block.replace('<tr class="bg-light">', '<tr class="collapse child-of-@doc.Id bg-light">', 1)
    content = parts[0] + 'var childQuote =' + child_block
    
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Fixed childQuote collapse bug.")
else:
    print("childQuote not found.")