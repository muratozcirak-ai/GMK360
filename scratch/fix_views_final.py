import codecs
import re
import glob

files = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')
for path in files:
    with codecs.open(path, 'r', 'utf-8-sig') as f:
        content = f.read()

    # Find everything between '<td class="text-end">' and '</td>' that contains 'item.QuoteStatus'
    target = r'<td class="text-end" style="width:250px;">\s*@if \(item\.QuoteStatus == GMK360\.Core\.Entities\.Construction\.BudgetQuoteStatus\.WaitingForPrice\)[\s\S]*?<!-- Action Buttons -->[\s\S]*?</div>[\s\S]*?</div>[\s\S]*?</td>'
    
    # We will just replace it with the new cell
    p = re.search(r'Phase(.*?)/', path).group(1)
    
    replacement = f'''<td class="text-end" style="width:250px;">
                                                <div class="d-flex align-items-center justify-content-end">
                                                    @if (item.PlannedTotalCost == 0)
                                                    {{
                                                        <button class="btn btn-sm btn-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName', '0', '@item.Description', '1', 'Götürü')">
                                                            <i class="bi bi-currency-dollar"></i> Tutar Gir
                                                        </button>
                                                    }}
                                                    else
                                                    {{
                                                        <button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2" onclick="openManageModal(@item.Id, '@item.ItemName', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '@item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Unit')">
                                                            <i class="bi bi-pencil"></i> Düzenle
                                                        </button>
                                                    }}

                                                    <form action="/Phase{p}/DeleteItem/@item.Id" method="post" class="d-inline">
                                                        <button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>
                                                    </form>
                                                </div>
                                            </td>'''
    
    content = re.sub(target, replacement, content)
    
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)