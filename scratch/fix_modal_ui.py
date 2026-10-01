import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix Kimden Alınır column (main doc)
content = content.replace("@(string.IsNullOrEmpty(doc.InstitutionContact) ? \"Belirtilmedi\" : doc.InstitutionContact)", 
                          "@(doc.SystemTemplate?.IssuedBy ?? \"Belirtilmedi\")")

# Fix Kimden Alınır column (child doc)
content = content.replace("@(string.IsNullOrEmpty(childDoc.InstitutionContact) ? \"Belirtilmedi\" : childDoc.InstitutionContact)", 
                          "@(childDoc.SystemTemplate?.IssuedBy ?? \"Belirtilmedi\")")

# Remove value="0" from modal inputs
content = content.replace('id="modalDocFee" value="0"', 'id="modalDocFee"')
content = content.replace('id="modalAddCost" value="0"', 'id="modalAddCost"')

# Fix JS 0 logic
js_old = """document.getElementById('modalDocFee').value = data.documentFee || 0;
                    document.getElementById('modalAddCost').value = data.additionalCost || 0;"""
js_new = """document.getElementById('modalDocFee').value = data.documentFee == 0 ? '' : data.documentFee;
                    document.getElementById('modalAddCost').value = data.additionalCost == 0 ? '' : data.additionalCost;"""
content = content.replace(js_old, js_new)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZero/Index.cshtml")
