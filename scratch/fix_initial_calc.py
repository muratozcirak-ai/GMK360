import io
import re

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Find the JS section and add calculateAndRender(); at the end of the DOMContentLoaded block
pattern = r"(totalShopRevenueInput\.addEventListener\('blur', saveToDatabase\);\s*)\}\);"
replacement = r"\1calculateAndRender();\n        });"

if re.search(pattern, content):
    content = re.sub(pattern, replacement, content)
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Fixed initial calculation.")
else:
    print("Could not find the pattern.")

