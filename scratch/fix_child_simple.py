import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Remove if (isChildOfSomeone) { continue; } entirely
content = content.replace('if (isChildOfSomeone) { continue; }', '')

# Replace the child row rendering
# I'll use regex to match the child row TR from '<tr class="collapse child-of-@doc.Id bg-light"' to '</tr>'
target = r'<tr class="collapse child-of-@doc\.Id bg-light" style="border-left: 4px solid #ffc107;">.*?</tr>'
replacement = '''<tr class="collapse child-of-@doc.Id bg-light" style="border-left: 4px solid #ffc107;">
                                                        <td colspan="6" class="ps-5 py-2">
                                                            <i class="bi bi-arrow-return-right text-muted me-2 ms-4"></i>
                                                            <span class="text-secondary fw-semibold">Önkoşul: @childDoc.DocumentName</span>
                                                            @if (childDoc.Status == "Alındı") {
                                                                <span class="badge bg-success ms-2"><i class="bi bi-check-circle"></i> Tamamlandı</span>
                                                            } else {
                                                                <span class="badge bg-danger ms-2"><i class="bi bi-lock-fill"></i> Tamamlanmadı (Bekliyor)</span>
                                                            }
                                                        </td>
                                                    </tr>'''
content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)