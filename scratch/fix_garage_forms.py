import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace <form action="/CompanyGarage/..." method="post"> with <form asp-action="..." method="post">
content = re.sub(r'<form\s+action="/CompanyGarage/([^"]+)"\s+method="post">', r'<form asp-action="\1" asp-controller="CompanyGarage" method="post">', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed forms in CompanyGarage/Index.cshtml')