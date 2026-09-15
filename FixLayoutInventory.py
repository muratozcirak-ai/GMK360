import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add Inventory back under ŞANTİYE & PROJELER
old_link = '''                    <a href="/DailyTimesheets/ProjectTimesheet" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "DailyTimesheets" ? "active" : "")">
                        <i class="bi bi-clock-history me-2 text-primary"></i> Şantiye Puantaj Cetveli
                    </a>'''

new_link = '''                    <a href="/DailyTimesheets/ProjectTimesheet" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "DailyTimesheets" ? "active" : "")">
                        <i class="bi bi-clock-history me-2 text-primary"></i> Şantiye Puantaj Cetveli
                    </a>
                    <a href="/Inventory/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Inventory" ? "active" : "")">
                        <i class="bi bi-boxes me-2 text-primary"></i> Şantiye Depoları (Demirbaş)
                    </a>'''

content = content.replace(old_link, new_link)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
