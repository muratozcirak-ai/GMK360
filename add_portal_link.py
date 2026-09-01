import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

old_td = """                                        <td class="text-end">
                                            <button class="btn btn-sm btn-outline-secondary rounded-pill" title="Düzenle"><i class="bi bi-pencil"></i></button>
                                        </td>"""

new_td = """                                        <td class="text-end">
                                            <a asp-controller="CustomerPortal" asp-action="MyUnitMaterials" asp-route-unitId="@unit.Id" class="btn btn-sm btn-outline-primary rounded-pill me-1" title="Müşteri Seçim Portalı (Görünüm)"><i class="bi bi-shop"></i></a>
                                            <button class="btn btn-sm btn-outline-secondary rounded-pill" title="Düzenle"><i class="bi bi-pencil"></i></button>
                                        </td>"""

content = content.replace(old_td, new_td)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added portal link to ManageBlock.cshtml")
