import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('<form id="wizardForm" asp-action="CreateWizard" asp-controller="ConstructionProject" method="post">', '<form id="wizardForm" asp-action="CreateWizard" asp-controller="ConstructionProject" method="post" enctype="multipart/form-data">')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added enctype multipart")
