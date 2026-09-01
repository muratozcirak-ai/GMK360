import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix Block Image Upload form
old_block_upload = """                        <input type="hidden" name="entityType" value="Building" />
                        <input type="hidden" name="entityIdStr" value="@Model.Id.ToString()" />"""

new_block_upload = """                        <input type="hidden" name="entityType" value="Building" />
                        <input type="hidden" name="entityId" value="@Model.Id" />"""

content = content.replace(old_block_upload, new_block_upload)

# Fix Floor Plan finding
old_plan_find = """                            // Kat planını bul
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == $"{Model.Id}_{floorGroup.Key}");"""

new_plan_find = """                            // Kat planını bul
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == floorGroup.Key);"""

content = content.replace(old_plan_find, new_plan_find)

# Fix Floor Plan Upload form
old_plan_upload = """                                                        <input type="hidden" name="entityType" value="BuildingFloor" />
                                                        <input type="hidden" name="entityIdStr" value="@($"{Model.Id}_{floorGroup.Key}")" />"""

new_plan_upload = """                                                        <input type="hidden" name="entityType" value="BuildingFloor_@Model.Id" />
                                                        <input type="hidden" name="entityId" value="@floorGroup.Key" />"""

content = content.replace(old_plan_upload, new_plan_upload)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated ManageBlock.cshtml Dms bindings")
