import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('<a href="/Dashboard/Construction" class="list-group-item list-group-item-action">\n                        <i class="bi bi-calendar-event me-2"></i> Vadesi Yaklaşanlar\n                    </a>', '<a href="/Finance/UpcomingPayments" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "UpcomingPayments" ? "active" : "")">\n                        <i class="bi bi-calendar-event me-2"></i> Vadesi Yaklaşanlar\n                    </a>')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
