import re

filepath = r'GMK360.Web\Controllers\AdminController.B2b.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the else if block for tedarikci
old_block = """            else if (type == "tedarikci")
            {
                query = query.Where(c => c.IsSupplier == true);
                pageTitle = "Malzeme Tedarikçileri & Nalburlar";
            }"""

new_block = """            else if (type == "tedarikci")
            {
                query = query.Where(c => c.IsSupplier == true);
                pageTitle = "Malzeme Tedarikçileri & Nalburlar";
                categoryCode = "B2BMaterialCategories";
            }"""

# Fix encoding issues in old_block for accurate replacing
content = re.sub(r'else if \(type == "tedarikci"\)\s*\{\s*query = query\.Where\(c => c\.IsSupplier == true\);\s*pageTitle = "Malzeme Tedarik.*?leri & Nalburlar";\s*\}', new_block, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Controller categoryCode")
