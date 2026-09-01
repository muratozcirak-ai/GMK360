import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("Planlanan Başlangıç (Hafriyat)", "Planlanan Başlangıç Tarihi")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated label successfully.")
