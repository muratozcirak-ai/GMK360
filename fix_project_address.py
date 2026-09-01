import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace Address = model.Address, with Address = string.IsNullOrWhiteSpace(model.Address) ? "Adres belirtilmedi" : model.Address,
content = content.replace("Address = model.Address,", 'Address = string.IsNullOrWhiteSpace(model.Address) ? "Adres belirtilmedi" : model.Address,')
# Let's also do Description just in case
content = content.replace("Description = model.Description,", 'Description = string.IsNullOrWhiteSpace(model.Description) ? "" : model.Description,')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed Address NULL issue in ConstructionProjectController.")
