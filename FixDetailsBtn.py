import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

content = content.replace('asp-controller="Inventory" asp-action="Index"', 'asp-controller="Inventory" asp-action="Index" asp-route-id="@Model.Id"')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
