import codecs
import re

path = 'GMK360.Web/Views/ProjectFinance/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'decimal iDiff = item\.ActualTotalCost - item\.PlannedTotalCost;.*?<td class="text-end pe-4 fw-bold @iColor">@Math\.Abs\(iDiff\)\.ToString\("N2"\) ₺ @\(iDiff > 0 \? "\(Aşıldı\)" : \(iDiff < 0 \? "\(Tasarruf\)" : ""\)\)</td>'

replacement = '''decimal iDiff = item.ActualTotalCost - item.PlannedTotalCost;
                                                string iColor = iDiff > 0 ? "text-danger" : (iDiff < 0 ? "text-success" : "text-muted");
                                                <tr>
                                                    <td class="ps-4">@item.ItemName <small class="text-muted d-block">@item.Quantity @item.Unit</small></td>
                                                    <td class="text-end">@item.PlannedTotalCost.ToString("N2") ₺</td>
                                                    <td class="text-end">@item.ActualTotalCost.ToString("N2") ₺</td>
                                                    @if (item.ActualTotalCost == 0)
                                                    {
                                                        <td class="text-end pe-4 text-muted"><small>Bekliyor</small></td>
                                                    }
                                                    else
                                                    {
                                                        <td class="text-end pe-4 fw-bold @iColor">@Math.Abs(iDiff).ToString("N2") ₺ @(iDiff > 0 ? "(Aşıldı)" : (iDiff < 0 ? "(Tasarruf)" : ""))</td>
                                                    }'''
# Wait, my regex target is capturing across multiple lines, let's use DOTALL.
# But there's a <tr> and <td>s in the middle. Let's make the regex exact.

target2 = r'decimal iDiff = item\.ActualTotalCost - item\.PlannedTotalCost;\s*string iColor = iDiff > 0 \? "text-danger" : \(iDiff < 0 \? "text-success" : "text-muted"\);\s*<tr>\s*<td class="ps-4">@item\.ItemName <small class="text-muted d-block">@item\.Quantity @item\.Unit</small></td>\s*<td class="text-end">@item\.PlannedTotalCost\.ToString\("N2"\) ₺</td>\s*<td class="text-end">@item\.ActualTotalCost\.ToString\("N2"\) ₺</td>\s*<td class="text-end pe-4 fw-bold @iColor">@Math\.Abs\(iDiff\)\.ToString\("N2"\) ₺ @\(iDiff > 0 \? "\(Aşıldı\)" : \(iDiff < 0 \? "\(Tasarruf\)" : ""\)\)</td>'

content = re.sub(target2, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)