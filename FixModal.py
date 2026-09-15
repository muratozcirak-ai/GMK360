import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\DailyTimesheets\ProjectTimesheet.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('asp-controller="Timesheet" asp-action="RequestAdvance"', 'asp-controller="DailyTimesheets" asp-action="RequestAdvance"')

# The form needs a sourceProjectId hidden input to redirect back correctly
form_insert = '<input type="hidden" name="sourceProjectId" value="@ViewBag.Project?.Id" />\r\n                <div class="mb-3">'
content = content.replace('<div class="mb-3">\r\n                    <label class="form-label fw-bold">Personel / Usta Seçin</label>', form_insert + '\r\n                    <label class="form-label fw-bold">Personel / Usta Seçin</label>')
content = content.replace('<div class="mb-3">\n                    <label class="form-label fw-bold">Personel / Usta Seçin</label>', form_insert + '\n                    <label class="form-label fw-bold">Personel / Usta Seçin</label>')


with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
