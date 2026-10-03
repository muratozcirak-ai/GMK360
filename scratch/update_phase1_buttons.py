import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<td class="text-end pe-4">\s*<form action="/PhaseOne/DeleteItem/@item\.Id" method="post" class="d-inline">\s*<button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>\s*</form>\s*</td>'
replacement = '''<td class="text-end pe-4">
                                                @if (item.QuoteStatus == GMK360.Core.Entities.Construction.BudgetQuoteStatus.WaitingForPrice)
                                                {
                                                    <button class="btn btn-sm btn-warning rounded-pill fw-bold px-3 me-2"><i class="bi bi-briefcase"></i> Teklif İste</button>
                                                }
                                                else
                                                {
                                                    <button class="btn btn-sm btn-outline-primary rounded-pill fw-bold px-3 me-2"><i class="bi bi-pencil"></i> Yönet</button>
                                                }
                                                <form action="/PhaseOne/DeleteItem/@item.Id" method="post" class="d-inline">
                                                    <button type="submit" class="btn btn-sm btn-outline-danger border-0"><i class="bi bi-trash"></i></button>
                                                </form>
                                            </td>'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)