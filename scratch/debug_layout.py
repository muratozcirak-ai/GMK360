with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

start_tag = '<div class="card shadow-sm border-0">'
end_tag = '<!-- SAĞ: İÇERİK ALANI -->'
print('Start:', content.find(start_tag))
print('End:', content.find(end_tag))
