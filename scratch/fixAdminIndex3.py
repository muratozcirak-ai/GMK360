with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('<td class="ps-4 fw-bold text-dark">@item.Name</td>',
'''<td class="ps-4 fw-bold text-dark">@item.Name</td>
                                <td><span class="badge bg-info text-dark">@(item.Stage ?? "-")</span></td>''')

with open('GMK360.Web/Views/AdminLegalDocument/Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(text)
