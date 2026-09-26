import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

html = html.replace('!string.IsNullOrEmpty(Model.CoverImageUrl)', '!string.IsNullOrEmpty(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)')
html = html.replace('src="@Model.CoverImageUrl"', 'src="@(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)"')

# And also let's just make sure ViewBag.CurrentStateImageUrl has NO cast errors!
# ViewBag is dynamic, so string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl) is better.
html = html.replace('!string.IsNullOrEmpty(ViewBag.CurrentStateImageUrl)', '!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl)')

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
