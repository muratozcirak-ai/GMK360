import io
import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# We want to add the section UNDER the "Alınan Teklifler (Gölge Tedarikçiler)" table.
# So we look for the end of that table card.

# In Details.cshtml, there is a card containing the table.
import sys
if "Pazar Yerinden Önerilen Firmalar" in content:
    print("Already added.")
    sys.exit(0)

# Let's find the closing of the card for Invites.
# Actually, the user's screenshot showed the footer right below the table.
# Let's see the structure.
import io
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

end_of_invites = -1
for i, line in enumerate(lines):
    if "Alınan Teklifler (Gölge Tedarikçiler)" in line or "Alnan Teklifler (Glge Tedarikiler)" in line:
        # Find the next </table> or </div>
        pass

# It's easier to append before the final closing tags or just search for the end of the col-12 or row.
