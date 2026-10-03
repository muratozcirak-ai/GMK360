import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/AllTasks.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<td class="fw-bold">@item.DriverName</td>'
replacement = '''<td class="fw-bold">
                                    @item.DriverName<br/>
                                    @if(item.DestinationProjectId.HasValue) {
                                        var pName = ((IEnumerable<dynamic>)ViewBag.Projects).FirstOrDefault(p => p.Id == item.DestinationProjectId.Value)?.Name;
                                        <span class="badge bg-primary mt-1"><i class="bi bi-building"></i> @pName</span>
                                    } else {
                                        <span class="badge bg-secondary mt-1"><i class="bi bi-building"></i> Merkez</span>
                                    }
                                </td>'''
content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)