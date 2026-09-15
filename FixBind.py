import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('CreateSimple([Bind("Name,Address,StartDate,Status,TotalFloors,TargetTotalApartments,TargetTotalShops")]', 'CreateSimple([Bind("Name,Address,StartDate,EndDate,Status,TotalFloors,TargetTotalApartments,TargetTotalShops")]')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
