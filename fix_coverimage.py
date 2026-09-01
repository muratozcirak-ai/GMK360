import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will just replace "CoverImageUrl" with "CoverImageFile" directly everywhere.
content = content.replace('asp-for="CoverImageUrl"', 'asp-for="CoverImageFile"')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Replaced CoverImageUrl with CoverImageFile.")
