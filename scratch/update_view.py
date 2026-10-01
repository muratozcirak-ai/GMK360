import io

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('-- Daire / -- Dükkan', '@ViewBag.GivenFlats Daire / @ViewBag.GivenShops Dükkan', 1)
content = content.replace('-- Daire / -- Dükkan', '@ViewBag.LeftFlats Daire / @ViewBag.LeftShops Dükkan', 1)
content = content.replace('0,00 ₺', '@ViewBag.EstimatedValue', 1)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
