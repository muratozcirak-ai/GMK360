import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('<select asp-for="DistrictId" class="form-select rounded-3" asp-items="ViewBag.Districts" required>', '<select asp-for="DistrictId" class="form-select rounded-3" asp-items="ViewBag.Districts">')
content = content.replace('<select asp-for="NeighborhoodId" class="form-select rounded-3" asp-items="ViewBag.Neighborhoods" required>', '<select asp-for="NeighborhoodId" class="form-select rounded-3" asp-items="ViewBag.Neighborhoods">')
content = content.replace('<select asp-for="StreetId" class="form-select rounded-3" asp-items="ViewBag.Streets" required>', '<select asp-for="StreetId" class="form-select rounded-3" asp-items="ViewBag.Streets">')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
