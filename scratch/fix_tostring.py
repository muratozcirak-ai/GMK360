import io
filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('.GetValueOrDefault().ToString("N2")', ' ?? 0).ToString()')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Removed GetValueOrDefault().ToString(N2)")
