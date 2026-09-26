import sys

filepath = 'GMK360.Web/Views/Shared/_AdminLayout.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace GlobalProviders link with B2bMarketplace link
content = content.replace('href="@Url.Action("GlobalProviders", "Admin")"', 'href="@Url.Action("B2bMarketplace", "Admin")"')
content = content.replace('Merkez Firma Havuzu', 'B2B Pazar Yeri')
content = content.replace('ph-users-three', 'ph-storefront')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
