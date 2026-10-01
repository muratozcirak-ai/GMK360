import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("checkTemplateUpdates(@Model.Id)", "checkTemplateUpdates(@ViewData[\"ProjectId\"])")
content = content.replace("applyTemplateUpdates(@Model.Id)", "applyTemplateUpdates(@ViewData[\"ProjectId\"])")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
