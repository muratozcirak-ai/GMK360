import codecs

path = 'GMK360.Web/Views/CompanyGarage/AllTasks.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix layout of Date and Status. I'll just write it nicely.
target = '''                            <tr>
                                <td>
                                    @item.TaskDate.ToShortDateString()<br/>
                                    <small class="text-muted">@(item.StartTime ?? "--:--") - @(item.EndTime ?? "--:--")</small>
                                </td>
                                <td>
                                    <span class="badge bg-dark">@item.Vehicle?.PlateNumber</span>
                                </td>
                                <td>@item.DriverName</td>
                                <td>@item.TaskDescription</td>
                                <td>
                                    @if(item.StartKm.HasValue || item.EndKm.HasValue) {
                                        <small class="d-block text-muted">KM: @(item.StartKm ?? 0) -> @(item.EndKm?.ToString() ?? "?")</small>
                                    }
                                    @if(item.WorkingHours.HasValue) {
                                        <small class="d-block text-info">Süre: @item.WorkingHours Saat</small>
                                    }
                                </td>
                                <td>
                                    @if(item.Status == "Tamamlandı") { <span class="badge bg-success">@item.Status</span> }
                                    else { <span class="badge bg-warning text-dark">@item.Status</span> }
                                </td>
                            </tr>'''

replacement = '''                            <tr>
                                <td>
                                    <strong>@item.TaskDate.ToShortDateString()</strong><br/>
                                    <span class="badge bg-light text-dark border"><i class="bi bi-clock"></i> @(item.StartTime ?? "--:--") - @(item.EndTime ?? "--:--")</span>
                                </td>
                                <td>
                                    <span class="badge bg-dark fs-6">@(item.Vehicle?.PlateNumber ?? "Bilinmiyor")</span>
                                </td>
                                <td class="fw-bold">@item.DriverName</td>
                                <td>@item.TaskDescription</td>
                                <td>
                                    @if(item.StartKm.HasValue || item.EndKm.HasValue) {
                                        <span class="badge bg-secondary mb-1">KM: @(item.StartKm ?? 0) <i class="bi bi-arrow-right"></i> @(item.EndKm?.ToString() ?? "?")</span><br/>
                                    }
                                    @if(item.WorkingHours.HasValue) {
                                        <span class="badge bg-info text-dark">Süre: @item.WorkingHours Saat</span>
                                    }
                                </td>
                                <td>
                                    @if(item.Status == "Tamamlandı" || item.Status == "Tamamland") { 
                                        <span class="badge bg-success fs-6"><i class="bi bi-check-circle-fill"></i> Tamamlandı</span> 
                                    }
                                    else { 
                                        <span class="badge bg-warning text-dark fs-6"><i class="bi bi-hourglass-split"></i> @item.Status</span> 
                                    }
                                </td>
                            </tr>'''
content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed AllTasks layout.')