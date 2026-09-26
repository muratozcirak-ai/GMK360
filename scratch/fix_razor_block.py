import sys
import re

filepath = 'GMK360.Web/Views/Admin/GlobalProviders.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = """@{
    ViewData["Title"] = "GMK360 - Merkez Firma Havuzu (Gerçek Veri)";
    Layout = "~/Views/Shared/_AdminLayout.cshtml";
    IEnumerable<GMK360.Core.Entities.DefinitionValue> categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;
    IEnumerable<GMK360.Core.Entities.City> cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>; as IEnumerable<GMK360.Core.Entities.DefinitionValue>;
}"""

replacement = """@{
    ViewData["Title"] = "GMK360 - Merkez Firma Havuzu";
    Layout = "~/Views/Shared/_AdminLayout.cshtml";
    var categories = ViewBag.Categories as IEnumerable<GMK360.Core.Entities.DefinitionValue>;
    var cities = ViewBag.Cities as IEnumerable<GMK360.Core.Entities.City>;
}"""

# Fix turkish characters encoding issues if any
content = re.sub(r'@{.*?}', replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
